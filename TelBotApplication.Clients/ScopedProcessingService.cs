﻿using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TelBotApplication.Clients.BotServices;
using TelBotApplication.Clients.helpers;
using TelBotApplication.Clients.Hubs;
using TelBotApplication.Domain.Abstraction;
using TelBotApplication.Domain.Chats;
using TelBotApplication.Domain.Commands;
using TelBotApplication.Domain.Dtos;
using TelBotApplication.Domain.Enums;
using TelBotApplication.Domain.Executors.Extensions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelBotApplication.Clients
{
    public class ScopedProcessingService : IScopedProcessingService
    {
        private readonly EnvironmentBotConfiguration _config;
        private readonly ILogger<BotClientService> _logger;
        private readonly IMapper _mapper;
        private IEnumerable<VenueRequest> _venueRequests;
        private IEnumerable<BotCommandDto> _botCommands;
        private ChatMember[] _admins;
        private readonly IHubContext<MemberHub, INewMember> _memberHub;
        private readonly IFilter _filter;
        private readonly IAnchorHandler _anchorHandler;
        private readonly ICommandCondition _commandCondition;
        private readonly ICurrencyConverter _currencyConverter;
        private ITelegramBotClient _bot { get; set; }
        private readonly string[] _fruitsArr;
        private CallBackUser _callBackUser;
        private string _fruit;
        private readonly Random _rnd;
        private ChatUser _incomingUser = default;
        private CancellationTokenSource cts;
        private CancellationTokenSource _ctsHello;
        private readonly List<UserRepeater> _counterKessages;
        private readonly IMemberExecutor _newmembersService;
        private List<long> _adminsIds;
        public ScopedProcessingService(IServiceProvider serviceProvider,
        IOptions<EnvironmentBotConfiguration> options,
        ILogger<BotClientService> logger,
        IMapper mapper,
        IHubContext<MemberHub, INewMember> memberHub,
        IMemberExecutor newmembersService,
        IFilter filter,
        ICommandCondition commandCondition,
        IAnchorHandler anchorHandler,
        ICurrencyConverter currencyConverter)
        {
            _config = options.Value;
            _bot = new TelegramBotClient(_config.Token);/*flud*/
            _logger = logger;
            _mapper = mapper;
            _rnd = new Random();
            _fruitsArr = new string[] { "🍎", "🍌", "🍒", "🍍", "🍋", "🍉" };
            _memberHub = memberHub;
            _counterKessages = new List<UserRepeater>();
            _newmembersService = newmembersService;
            _newmembersService.AlertEvent += _newmembersService_AlertEvent;
            _newmembersService.RestrictEvent += _newmembersService_RestrictEvent;
            _anchorHandler = anchorHandler;
            _filter = filter;
            _commandCondition = commandCondition;
            Task.Factory.StartNew(() =>
            {
                _newmembersService.RunAlertPolling();
            });
            _anchorHandler = anchorHandler;
            _currencyConverter = currencyConverter;
        }



        private async Task _newmembersService_RestrictEvent(Message message, CancellationToken cancellationToken = default)
        {

            try
            {
                await _bot.DeleteMessageAsync(message.Chat.Id, message.MessageId);
            }
            catch { }

            await _bot.RestrictChatMemberAsync(message.Chat.Id, userId: _callBackUser.UserId,
                new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false }, untilDate: DateTime.Now.AddMinutes(10));
            var result = await _bot.SendTextMessageAsync(chatId: message.Chat.Id, $"ВАЖНО: Ты не нажал(а)" +
                $" кнопку, значит ты БОТ или СПАМЕР, в тестовом режиме РО на пять минут \n Пока можно изучить правила чата \n" +
                $@"https://t.me/winnersDV2022flood/3");
            await Task.Delay(5000);
            await _bot.DeleteMessageAsync(result.Chat.Id, result.MessageId);
            //_newmembersService.ClearMembersList();
        }
        private async Task _newmembersService_AlertEvent(Message message, CancellationToken cancellationToken = default)
        {
            var result = await _bot.SendTextMessageAsync(chatId: message.Chat, $"@{_incomingUser.FirstName}   {_incomingUser.UserName}," +
                                             $" пожалуйста выполни проверку на антиспам https://t.me/{message.Chat.Username}/{_incomingUser.MessageId}", disableWebPagePreview: true);
            await Task.Delay(10000);
            await _bot.DeleteMessageAsync(result.Chat.Id, result.MessageId);
        }

        public async Task GetCallBackFromNewMemeber(string message)
        {
            _logger.LogInformation("{CurrentCallBack}", message);
            var arr = message.Split(':');
            await _bot.SendTextMessageAsync(chatId: arr[0], $"@ You {arr[5]}", replyToMessageId: int.Parse(arr[2]), disableWebPagePreview: true);

            // await _memberHub.Clients.All.SayHello($"{message.Chat.Id}:{message.MessageId}:{user.MessageId}:{user.FullName}:{user.UserName}:{text}");
        }
        public async Task StartChatPolling(CancellationToken stoppingToken)
        {
            cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var pollingTask = RunBotPolling(cancellationToken);
            var dbUpdaterTask = AddCommandsListForBot(cancellationToken);
            var filters = _filter.UpdateFilters();
            var anchors = _anchorHandler.UpdateAchors();

            await Task.WhenAll(pollingTask, dbUpdaterTask, filters, anchors);

        }

        #region Start
        private async Task RunBotPolling(CancellationToken cancellationToken)
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>(),
                ThrowPendingUpdates = true
            };
            var updateReceiver = new QueuedUpdateReceiver(_bot, receiverOptions);

            try
            {
                if (cts.Token.IsCancellationRequested)
                {
                    cts = new CancellationTokenSource();

                }
                await foreach (var update in updateReceiver.WithCancellation(cts.Token))
                {

                    if (update is Update message)
                    {

                        await HandleUpdateAsync(_bot, message, cancellationToken);
                    }

                }

            }
            catch (ApiRequestException ex)
            {
                await HandleErrorAsync(ex, cancellationToken);
            }
        }
        private async Task AddCommandsListForBot(CancellationToken cancellationToken)
        {
            await Task.Delay(1000);
            while (true)
            {
                var locations = await _commandCondition.GetAllLocations();
                var list = await _commandCondition.GetAllBotCommands();
                _botCommands = _mapper.Map<IEnumerable<BotCommandDto>>(list);
                _venueRequests = _mapper.Map<IEnumerable<VenueRequest>>(locations);
                var commandsList = new List<BotCommand>();
                foreach (var item in _botCommands)
                {
                    commandsList.Add(new BotCommand
                    {
                        Command = item.Command,
                        Description = item.Description
                    });
                }
                foreach (var item in _venueRequests)
                {
                    commandsList.Add(new BotCommand
                    {
                        Command = item.Command,
                        Description = item.Title
                    });
                }
                await _bot.SetMyCommandsAsync(commandsList, BotCommandScope.AllGroupChats(), languageCode: "en", cancellationToken);
                await Task.Delay(new TimeSpan(0, 5, 0), cancellationToken);
            }
        }
        #endregion

        public Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
        {
            if (exception is ApiRequestException)
            {
                _logger.LogError(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
            }
            // Некоторые действия
            return Task.CompletedTask;
        }


        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (_ctsHello == null)
            {
                _ctsHello = new CancellationTokenSource();
            }

            var chat_message = new ChatMessage(update);
            var user = chat_message.GetCurrentUser();
            var message = chat_message.GetCurrentMessage();
            var userId = user.UserId;
            var text = chat_message.GetCurrentMessageText();


            _logger.LogDebug(Newtonsoft.Json.JsonConvert.SerializeObject(update));

            if (update.Type == UpdateType.Message)
            {
                //if (user.UserName.Equals("yoga_victoriya") || user.UserName.Equals("Zazdravnaya"))
                //{
                ////    var lisst = new List<string>
                ////{
                ////             /*"https://i.gifer.com/I22M.gif"*/,
                ////             //"https://i.gifer.com/JdS7.gif",
                ////             //"https://i.gifer.com/j9.gif",
                ////             //"https://i.gifer.com/2JKo.gif",
                ////             //"https://i.gifer.com/72fW.gif",
                ////             //"https://i.gifer.com/W3A4.gif"
                ////        };
                //    var caption = user.UserName.Equals("Zazdravnaya") ? $"А ты возьми казан, дорогой(ая) {user.UserName}" : $"А ты купи слона, дорогая {user.UserName}";
                //   // var link = lisst[_rnd.Next(lisst.Count)];
                //    var result = await botClient.SendAnimationAsync(message.Chat, animation: "https://i.gifer.com/I22M.gif", caption: user.UserName, replyToMessageId: message.MessageId, allowSendingWithoutReply: true, cancellationToken: cancellationToken);
                //    await Task.Delay(5000, cancellationToken);
                //    await botClient.DeleteMessageAsync(chatId: result.Chat, result.MessageId, cancellationToken);
                //}
                if (_admins is null || !_admins.Any())
                {
                    _admins = await _bot.GetChatAdministratorsAsync(update.Message.Chat);
                    _adminsIds = _admins.Select(u => u.User.Id).ToList();
                    _adminsIds.Add(1087968824);
                }

                if (text != null && _botCommands != null && (_botCommands.Any(x => text.Contains($"{x.Command.Trim()}@", StringComparison.OrdinalIgnoreCase)) || _botCommands.Any(x => text.Equals(x.Command.Trim(), StringComparison.OrdinalIgnoreCase))))
                {

                    var command = _botCommands.FirstOrDefault(x => text.Contains($"{x.Command.Trim()}@", StringComparison.OrdinalIgnoreCase) || text.Equals(x.Command.Trim(), StringComparison.OrdinalIgnoreCase));
                    switch (command.TypeOfreaction)
                    {
                        case TypeOfreactions.Text:
                            await botClient.SendTextMessageWhithDelayAsync(isEnabled: true, message, message.Chat, $" {command.Caption} {user.FullName} !", new TimeSpan(0, 0, 30), ParseMode.Html, cancellationToken: cancellationToken);
                            return;
                        case TypeOfreactions.Animation:
                            await botClient.SendAnimationWhithDelayAsync(isEnabled: true, message, message.Chat, animation: command.Link, new TimeSpan(0, 0, 30), caption: command.Caption, cancellationToken: cancellationToken);
                            return;
                        case TypeOfreactions.Photo:
                            await botClient.SendPhotoWithDelayAsync(message: message, delay: new TimeSpan(0, 0, 30), message.Chat, photo: command.Link, isEnabled: true, caption: command.Caption, ParseMode.Html, cancellationToken: cancellationToken);
                            return;
                    }

                }


                if (text != null && _venueRequests != null && _venueRequests.Any(x => text.Contains(x.Command.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    var location = _venueRequests.FirstOrDefault(x => text.Contains(x.Command.Trim(), StringComparison.OrdinalIgnoreCase));
                    await botClient.SendVenueWithDelayAsync(true, new TimeSpan(0, 0, 30), message, location, message.Chat, cancellationToken: cancellationToken);

                    return;
                }

                #region Anchors
                if (message?.Type != null && message?.Entities != null && message.Entities.Any(x => x.Type == MessageEntityType.Hashtag))
                {
                    await _anchorHandler.ExecuteAncor(botClient, message, text, cancellationToken);
                }
                #endregion Anchors
                if (message?.Type != null && message.Type == MessageType.ChatMembersAdded)
                {
                    var index = _rnd.Next(_fruitsArr.Length);
                    _fruit = _fruitsArr[index];
                    _callBackUser = new CallBackUser { UserId = user.UserId };
                    var messageHello = await SendInAntiSpamline(botClient: botClient, message: message, user, _fruit, update, cancellationToken: cancellationToken);
                    _newmembersService.AddNewMember(_callBackUser, messageHello, DateTime.Now);
                    return;
                }
                if (message?.Type != null && message.Type == MessageType.ChatMemberLeft)
                {
                    await botClient.SendAnimationWhithDelayAsync(isEnabled: true, message, message.Chat, animation: "https://i.gifer.com/ABMO.gif",
                        new TimeSpan(0, 0, 30), caption: $"Прощай дорогой {user.FullName}.\n Нам будет тебя не хватать (((((", cancellationToken: cancellationToken);
                    return;
                }
            }

            if (update.Type == UpdateType.EditedMessage)
            {
                await _memberHub.Clients.All.EditLog($"{update.EditedMessage.Chat.Id}:{user.MessageId}:{update.EditedMessage.From.FirstName ?? "no firstName"} {update.EditedMessage.From.LastName ?? "no lastName"}:{update.EditedMessage.From.Username ?? "no userName"}:{text}:{message?.Chat?.Title ?? "no title"}");
                return;
            }

            if (message?.Type != null && message.Type == MessageType.Photo)
            {
                await _memberHub.Clients.All.SendPhotoLog($"{update.Message.Chat.Id}:{update.Message.MessageId}:{message.From.FirstName ?? "no firstName"} {message.From.LastName ?? "no lastName"}:{message.From.Username ?? "no userName"}:{text ?? "Photo"}:{message.Chat.Title}");
                return;
            }
            if (message?.Type != null && message.Type == MessageType.Video)
            {
                await _memberHub.Clients.All.SendVideoLog($"{update.Message.Chat.Id}:{update.Message.MessageId}:{message.From.FirstName ?? "no firstName"} {message.From.LastName ?? "no lastName"}:{message.From.Username ?? "no userName"}:{text ?? "Video"}:{message.Chat.Title}");
                return;
            }
            if (message?.Type != null && message.Type == MessageType.Document)
            {
                await _memberHub.Clients.All.SendDocumentlLog($"{update.Message.Chat.Id}:{update.Message.MessageId}:{message.From.FirstName ?? "no firstName"} {message.From.LastName ?? "no lastName"}:{message.From.Username ?? "no userName"}:{text ?? "Gif"}:{message.Chat.Title}");
                return;
            }
            if (message?.Type != null && message.Type == MessageType.Sticker)
            {
                await _memberHub.Clients.All.SendStickerLog($"{update.Message.Chat.Id}:{update.Message.MessageId}:{message.From.FirstName ?? "no firstName"} {message.From.LastName ?? "no lastName"}:{message.From.Username ?? "no userName"}:{message.Sticker.FileUniqueId}:{message.Chat.Title}");
                return;
            }

            if (message?.Type != null && message.Type == MessageType.Text)
            {



                #region Currency
                if ((text.EndsWith("rub") || text.EndsWith("eur") || text.EndsWith("euro") || text.EndsWith("usd") || text.EndsWith("brl")))
                {

                    var arr = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (arr.Length == 2 && float.TryParse(arr[0], out var value))
                    {
                        decimal answer;
                        switch (arr[1])
                        {
                            case "rub":
                                answer = await _currencyConverter.GetCurrencyFromRubles(value);
                                await botClient.SendTextMessageWhithDelayAsync(isEnabled: true, message, message.Chat, $" {text} = {String.Format("{0:F2}", answer)} USD для {user.FullName} !",
                                    new TimeSpan(0, 0, 15), parseMode: ParseMode.Html, replyToMessageId: message.ReplyToMessage?.MessageId ?? -1,
                       allowSendingWithoutReply: true, disableWebPagePreview: true, cancellationToken: cancellationToken).ConfigureAwait(false);
                                return;
                            case "usd":
                                answer = await _currencyConverter.GetCurrencyFromUSD(value);
                                await botClient.SendTextMessageWhithDelayAsync(isEnabled: true, message, message.Chat, $" {text} = {String.Format("{0:F2}", answer)} RUB для {user.FullName} !",
                                   new TimeSpan(0, 0, 15), parseMode: ParseMode.Html, replyToMessageId: message.ReplyToMessage?.MessageId ?? -1,
                      allowSendingWithoutReply: true, disableWebPagePreview: true, cancellationToken: cancellationToken).ConfigureAwait(false);
                                return;
                            case "eur":
                            case "euro":
                                answer = await _currencyConverter.GetCurrencyFromEUR(value);
                                await botClient.SendTextMessageWhithDelayAsync(isEnabled: true, message, message.Chat, $" {text} = {String.Format("{0:F2}", answer)} RUB для {user.FullName} !",
                                  new TimeSpan(0, 0, 15), parseMode: ParseMode.Html, replyToMessageId: message.ReplyToMessage?.MessageId ?? -1,
                     allowSendingWithoutReply: true, disableWebPagePreview: true, cancellationToken: cancellationToken).ConfigureAwait(false);
                                return;
                            case "brl":
                                answer = await _currencyConverter.GetCurrencyFromReal(value);
                                await botClient.SendTextMessageWhithDelayAsync(isEnabled: true, message, message.Chat, $" {text} = {String.Format("{0:F2}", answer)} RUB для {user.FullName} !",
                                  new TimeSpan(0, 0, 15), parseMode: ParseMode.Html, replyToMessageId: message.ReplyToMessage?.MessageId ?? -1,
                     allowSendingWithoutReply: true, disableWebPagePreview: true, cancellationToken: cancellationToken).ConfigureAwait(false);
                                return;
                            default:
                                return;
                        }
                    }

                }
                #endregion Currency


                if (_adminsIds != null && !_adminsIds.Contains(message.From.Id) && StringUtil.IsMatched(text))
                {
                    text = StringUtil.CorrectMatching(text);
                    var result = await botClient.SendTextMessageAsync(message.Chat, $"{user.FullName},- {text}", ParseMode.Html, disableWebPagePreview: true, replyToMessageId: message.MessageId, allowSendingWithoutReply: true, cancellationToken: cancellationToken);
                    await Task.Delay(5000, cancellationToken);
                    await botClient.DeleteMessageAsync(chatId: result.Chat, result.MessageId, cancellationToken);

                }


                await _memberHub.Clients.All.SendLog($"{message.Chat.Id}:{user.MessageId}:{user.FullName}:{user.UserName}:{text}:{message.Chat.Title}");
                //if (_counterKessages.Count == 0 && message.ReplyToMessage?.MessageId != null)
                //{
                //    _counterKessages.Add(new UserRepeater { UserId = user.UserId, ReplyToMessageId = message.ReplyToMessage?.MessageId ?? 1 });

                //}

                //else if (_counterKessages.Count == 1 && message.ReplyToMessage?.MessageId != null && _counterKessages.Any(x => x.UserId == user.UserId && x.ReplyToMessageId == message.ReplyToMessage?.MessageId))
                //{
                //    await botClient.SendTextMessageWhithDelayAsync(isEnabled: true, message, message.Chat, @$"<b>{user.FullName}</b>, это нарушение: несколько сообщений подряд в
                //        чат запрещено отправлять пунктом 2.2 правил.! https://t.me/winnersDV2022flood/3", new TimeSpan(0, 0, 10), parseMode: ParseMode.Html, replyToMessageId: message.MessageId,
                //   allowSendingWithoutReply: false, disableWebPagePreview: true, cancellationToken: cancellationToken).ConfigureAwait(false);
                //    _counterKessages.Clear();
                //    return;
                //}
                //else if (_counterKessages.Count == 1 && !_counterKessages.Any(x => x.UserId == user.UserId))
                //{
                //    _counterKessages.Clear();

                //}
                //if (_textFilter.IsAlertFrase(text.Trim().ToLower(CultureInfo.InvariantCulture)))
                //{
                //    await botClient.SendTextMessageWhithDelayAsync(isEnabled: true, message, message.Chat, $@"<b>{user.FullName}</b>, это нарушение п. 13 правил." +
                //$" Вопросы основного чата обсуждаем только там.В следующий раз будет РО.! https://t.me/winnersDV2022flood/3", new TimeSpan(0, 0, 10),
                // disableWebPagePreview: true, cancellationToken: cancellationToken);
                //    return;
                //}
                var alert = _filter.FindAnswerForAlertFrase(text);
                var isAlert = !string.IsNullOrEmpty(alert);
                if (_adminsIds != null && text.Length == 1 && text.Equals(".", StringComparison.InvariantCultureIgnoreCase) && _adminsIds.Contains(message.From.Id))
                {
                    await botClient.SendTextMessageWhithDelayAsync(isEnabled: true, message, message.Chat, $"Нарушение п. 13 правил." +
                       $" Вопросы основного чата обсуждаем только там.В следующий раз будет РО.! https://t.me/winnersDV2022flood/3", new TimeSpan(0, 0, 30), parseMode: ParseMode.Html, replyToMessageId: message.ReplyToMessage?.MessageId ?? -1,
                        allowSendingWithoutReply: true, disableWebPagePreview: true, cancellationToken: cancellationToken).ConfigureAwait(false);
                    return;
                }
                else if (_adminsIds != null && text.Length == 1 && text.Equals(".", StringComparison.InvariantCultureIgnoreCase) && !_adminsIds.Contains(message.From.Id))
                {
                    await botClient.DeleteMessageAsync(message.Chat, message.MessageId, cancellationToken);
                    return;
                }
                else if (_adminsIds != null && isAlert && !_adminsIds.Contains(message.From.Id))
                {
                    await botClient.SendTextMessageWhithDelayAsync(isEnabled: true, message, message.Chat, alert, new TimeSpan(0, 0, 10), parseMode: ParseMode.Html, replyToMessageId: message.ReplyToMessage?.MessageId ?? -1,
                         allowSendingWithoutReply: true, disableWebPagePreview: true, cancellationToken: cancellationToken).ConfigureAwait(false);
                    return;
                }

                else
                {
                    var trimmedText = text.Remove(text.Length - 1, 1);
                    var res = long.TryParse(trimmedText.Trim(), out var temp);
                    if (res && message.Type == MessageType.Text)
                    {
                        _ = await Task.Factory.StartNew(async () =>
                        {
                            if (res && text.EndsWith('f') || text.EndsWith('F') || text.EndsWith('ф') || text.EndsWith('Ф'))
                            {
                                var result = await botClient.SendTextMessageAsync(message.Chat, $" {text} = {(temp - 32) * 5 / 9}C° для {user.FullName} !", cancellationToken: cancellationToken);
                                await botClient.DeleteMessageAsync(chatId: message.Chat, message.MessageId, cancellationToken);
                                await Task.Delay(3000, cancellationToken);
                                await botClient.DeleteMessageAsync(chatId: result.Chat, result.MessageId, cancellationToken);
                                return;
                            }
                            else if (res && text.EndsWith('c') || text.EndsWith('C') || text.EndsWith('с') || text.EndsWith('С'))

                            {
                                var result = await botClient.SendTextMessageAsync(message.Chat, $" {text} = {(temp * 9 / 5) + 32}F° для {user.FullName} !", cancellationToken: cancellationToken);
                                await botClient.DeleteMessageAsync(chatId: message.Chat, message.MessageId, cancellationToken);
                                await Task.Delay(3000, cancellationToken);
                                await botClient.DeleteMessageAsync(chatId: result.Chat, result.MessageId, cancellationToken);
                                return;
                            }
                        }, cancellationToken);
                        return;
                    }



                }

                return;
            }
            #region Auto answers


            #endregion Auto answer


            if (update.Type == UpdateType.CallbackQuery)
            {
                var chatId = update.CallbackQuery.Message.Chat.Id;
                var messId = update.CallbackQuery.Message.MessageId;
                var codeOfButton = update.CallbackQuery.Data;
                var telegramMessage = codeOfButton;
                if (_fruitsArr.Contains(codeOfButton, StringComparer.Ordinal) && _callBackUser != null && _callBackUser.UserId == update.CallbackQuery.From.Id)
                {
                    _newmembersService.DropNewMember(update.CallbackQuery);
                    if (codeOfButton.Equals(_fruit))
                    {

                        await SuccessfullNewMemberAddedAsync(botClient, update, chatId, messId, cancellationToken);
                    }
                    else
                    {
                        await UnsuccessfullNewMemberAddedWithRestrictAsync(botClient, update, chatId, messId, cancellationToken);
                    }

                    return;
                }
                else if (_fruitsArr.Contains(codeOfButton, StringComparer.Ordinal) && _callBackUser != null && _callBackUser.UserId != update.CallbackQuery.From.Id)
                {
                    await botClient.AnswerCallbackQueryAsync(callbackQueryId: update.CallbackQuery.Id, text: $@"{update.CallbackQuery.From.FirstName} {update.CallbackQuery.From.LastName} , не трогай чужие фрукты!"
                                , showAlert: true, url: null, null, cancellationToken: cancellationToken);
                    return;
                }
                //якорь с реакцией
                else if (!_fruitsArr.Contains(codeOfButton, StringComparer.OrdinalIgnoreCase))
                {
                    codeOfButton = codeOfButton.Substring(1);
                    //  await botClient.EditMessageReplyMarkupAsync(update.CallbackQuery.Message.Chat, update.CallbackQuery.Message.MessageId, InlineKeyboardMarkup);
                    await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, $@"https://t.me/{codeOfButton}", true, null, 5, cancellationToken);
                    await botClient.DeleteMessageAsync(chatId, messId, cancellationToken);
                }
            }
        }






        #region Inline buttons
        private async void SendInlineAdmins(ITelegramBotClient botClient, ChatMember[] members, Telegram.Bot.Types.Chat chat, Message message, long chatId, CancellationToken cancellationToken)
        {
            var btnArr = GetButtons(members);
            var inlineKeyboard = new InlineKeyboardMarkup(btnArr);
            // keyboard
            await botClient.DeleteMessageAsync(chatId: chat, message.MessageId, cancellationToken);
            _ = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Вызов администратора",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);


        }
        private IEnumerable<IEnumerable<InlineKeyboardButton>> GetButtons(ChatMember[] members)
        {
            foreach (var member in members)
            {
                yield return new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData(text: $"{member.User.FirstName} {member.User.LastName}", callbackData: $"@{member.User.Username}")
                };
            }
        }
        private IEnumerable<IEnumerable<InlineKeyboardButton>> GetButtonsForHello()
        {
            foreach (var fruit in _fruitsArr)
            {
                yield return new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData(text: fruit, callbackData: fruit)
                };
            }
        }
        private async Task<Message> SendInAntiSpamline(ITelegramBotClient botClient, Message message, ChatUser user, string fruit, Update update, CancellationToken cancellationToken)
        {

            var keyboard = new InlineKeyboardMarkup(
               new[]
               {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(text: "🍎", callbackData: "🍎"),
                        InlineKeyboardButton.WithCallbackData(text: "🍌", callbackData: "🍌"),
                        InlineKeyboardButton.WithCallbackData(text: "🍒", callbackData: "🍒"),
                        InlineKeyboardButton.WithCallbackData(text: "🍍", callbackData: "🍍"),
                        InlineKeyboardButton.WithCallbackData(text: "🍋", callbackData: "🍋"),
                        InlineKeyboardButton.WithCallbackData(text: "🍉", callbackData: "🍉")
                    }
               });
            _incomingUser = new ChatUser(update: update);
            var messag = await botClient.SendTextMessageAsync(chatId: message.Chat.Id, $"ВАЖНО: {user.FullName}  @{user.UserName}, если ты " +
                $"не БОТ и не СПАМЕР, пройди проверку, нажав на кнопку, где есть {fruit}", parseMode: ParseMode.Html, replyMarkup: keyboard,
               cancellationToken: cancellationToken);

            return messag;
        }
        #endregion




        #region income
        private async Task UnsuccessfullNewMemberAddedWithRestrictAsync(ITelegramBotClient botClient, Update update, long chatId, int messId, CancellationToken cancellationToken)
        {
            await botClient.AnswerCallbackQueryAsync(callbackQueryId: update.CallbackQuery.Id, text: $"{update.CallbackQuery.From.FirstName} {update.CallbackQuery.From.LastName}, " +
                $"Вы не прошли проверку на антиспам, вы будете лишены возможности комментировать в группе в течение одного часа."
                , showAlert: true, url: null, cacheTime: 15, cancellationToken: cancellationToken);
            await botClient.DeleteMessageAsync(chatId, messId, cancellationToken);
            await Task.Delay(2000, cancellationToken);
            await botClient.RestrictChatMemberAsync(chatId, userId: _callBackUser.UserId,
               new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false }, untilDate: DateTime.Now.AddSeconds(31), cancellationToken);
        }

        private static async Task SuccessfullNewMemberAddedAsync(ITelegramBotClient botClient, Update update, long chatId, int messId, CancellationToken cancellationToken)
        {
            try
            {
                await botClient.AnswerCallbackQueryAsync(callbackQueryId: update.CallbackQuery.Id, text: $@"{update.CallbackQuery.From.FirstName} {update.CallbackQuery.From.LastName} , приветствуем вас в ламповой и дружной флудилке! Здесь любят фудпорн, "
                               + $" троллить Джобса, сербскую еду, подгонять Данзана, а также йожек!"
                                 , showAlert: true, url: null, null, cancellationToken: cancellationToken);
                await botClient.DeleteMessageAsync(chatId, messId, cancellationToken);
            }
            catch
            {
            }

        }


        #endregion income
    }
}
