using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TelBotApplication.Clients.BotServices;
using TelBotApplication.Clients.Hubs;
using TelBotApplication.DAL.Interfaces;
using TelBotApplication.Domain.Abstraction;
using TelBotApplication.Domain.Chats;
using TelBotApplication.Domain.Commands;
using TelBotApplication.Domain.Dtos;
using TelBotApplication.Domain.Enums;
using TelBotApplication.Domain.Models;
using TelBotApplication.Domain.NewFolder.Executors.Extensions;
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
        private ITelegramBotClient _bot { get; set; }
        private readonly string[] _fruitsArr;
        private CallBackUser _callBackUser;
        private string _fruit;
        private readonly Random _rnd;
        private ChatUser _incomingUser = default;
        private CancellationTokenSource cts;
        private CancellationTokenSource _ctsHello;
        private List<UserRepeater> _counterKessages;
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
        IAnchorHandler anchorHandler)
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
        }



        private async Task _newmembersService_RestrictEvent(Message message, CancellationToken cancellationToken = default) 
        {
          
            try
            {
                await _bot.DeleteMessageAsync(message.Chat.Id, message.MessageId);
            }
            catch { }

            await _bot.RestrictChatMemberAsync(message.Chat.Id, userId: _callBackUser.UserId,
                new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false }, untilDate: DateTime.Now.AddSeconds(60));
            Message result = await _bot.SendTextMessageAsync(chatId: message.Chat.Id, $"ВАЖНО: Ты не нажал(а)" +
                $" кнопку, значит ты БОТ или СПАМЕР, в тестовом режиме РО на пять минут \n Пока можно изучить правила чата \n" +
                $@"https://t.me/winnersDV2022flood/3");
            await Task.Delay(5000);
            await _bot.DeleteMessageAsync(result.Chat.Id, result.MessageId);
            //_newmembersService.ClearMembersList();
        }
        private async Task _newmembersService_AlertEvent(Message message, CancellationToken cancellationToken = default) 
        {
            Message result = await _bot.SendTextMessageAsync(chatId: message.Chat, $"@{_incomingUser.FirstName}   {_incomingUser.UserName}," +
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
            CancellationToken cancellationToken = cts.Token;
            Task pollingTask = RunBotPolling(cancellationToken);
            Task dbUpdaterTask = AddCommandsListForBot(cancellationToken);
            Task filters = _filter.UpdateFilters();
            Task anchors = _anchorHandler.UpdateAchors();
            await Task.WhenAll(pollingTask, dbUpdaterTask, filters, anchors);
          
        }

        #region Start
        private async Task RunBotPolling(CancellationToken cancellationToken)
        {
            ReceiverOptions receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>(),
                ThrowPendingUpdates = true
            };
            QueuedUpdateReceiver updateReceiver = new QueuedUpdateReceiver(_bot, receiverOptions);

            try
            {
                await foreach (Update update in updateReceiver.WithCancellation(cts.Token))
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
                List<BotCommand> commandsList = new List<BotCommand>();
                foreach (BotCommandDto item in _botCommands)
                {
                    commandsList.Add(new BotCommand
                    {
                        Command = item.Command,
                        Description = item.Description
                    });
                }
                foreach (VenueRequest item in _venueRequests)
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
            
            ChatMessage chat_message = new ChatMessage(update);
            ChatUser user = chat_message.GetCurrentUser();
            Message message = chat_message.GetCurrentMessage();
            long userId = user.UserId;
            string text = chat_message.GetCurrentMessageText();
          
         
            _logger.LogDebug(Newtonsoft.Json.JsonConvert.SerializeObject(update));

            if (update.Type == UpdateType.Message)
            {
                _admins = await botClient.GetChatAdministratorsAsync(update.Message.Chat);
                 _adminsIds = _admins.Select(u => u.User.Id).ToList();
                _adminsIds.Add(1087968824);
                if (text != null && _botCommands != null && _botCommands.Any(x => text.Contains(x.Command.Trim(), StringComparison.OrdinalIgnoreCase)))
                {

                    BotCommandDto command = _botCommands.FirstOrDefault(x => text.Contains(x.Command.Trim(), StringComparison.OrdinalIgnoreCase));
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
                    VenueRequest location = _venueRequests.FirstOrDefault(x => text.Contains(x.Command.Trim(), StringComparison.OrdinalIgnoreCase));
                    await botClient.SendVenueWithDelayAsync(true, new TimeSpan(0, 0, 30), message, location, message.Chat, cancellationToken: cancellationToken);

                    return;
                }




                if (chat_message.GetCurrentMessageText().Contains("/stat", StringComparison.OrdinalIgnoreCase))
                {
                    int count = await botClient.GetChatMemberCountAsync(message.Chat, cancellationToken);
                    ChatMember[] countAdmins = await botClient.GetChatAdministratorsAsync(message.Chat, cancellationToken);
                    await botClient.SendTextMessageWhithDelayAsync(isEnabled: true, message, message.Chat, $"В чате юзеров {count} ; администраторов {countAdmins.Length} " +
                        $" от {user.FullName}", new TimeSpan(0, 0, 30), ParseMode.Html, cancellationToken: cancellationToken);
                    return;
                }

                if (chat_message.GetCurrentMessageText().Contains("/admin", StringComparison.OrdinalIgnoreCase))
                {
                    long[] exList = new long[] { 5316258402, 416112286 };
                    if (_admins == null)
                    {
                        _admins = await botClient.GetChatAdministratorsAsync(message.Chat, cancellationToken);
                    }
                    ChatMember[] selectedAdmins = _admins.Where(u => !exList.Contains(u.User.Id)).ToArray();
                    SendInlineAdmins(botClient: botClient, selectedAdmins, message.Chat, message, chatId: message.Chat.Id, cancellationToken: cancellationToken);
                    return;
                }
                #region Anchors
                if (message?.Type != null && message?.Entities != null && message.Entities.Any(x=>x.Type ==MessageEntityType.Hashtag) )
                {
                   await _anchorHandler.ExecuteAncor(text);
                }
                #endregion Anchors
                if (message?.Type != null && message.Type == MessageType.ChatMembersAdded)
                {
                    int index = _rnd.Next(_fruitsArr.Length);
                    _fruit = _fruitsArr[index];
                    _callBackUser = new CallBackUser { UserId = user.UserId };
                    Message messageHello = await SendInAntiSpamline(botClient: botClient, message: message, user, _fruit, update, cancellationToken: cancellationToken);
                    _newmembersService.AddNewMember(_callBackUser,messageHello, DateTime.Now);
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
                await _memberHub.Clients.All.EditLog($"{update.EditedMessage.Chat.Id}:{user.MessageId}:{update.EditedMessage.From.FirstName ?? "no firstName"} {update.EditedMessage.From.LastName ?? "no lastName"}:{update.EditedMessage.From.Username ?? "no userName"}:{text}:{message.Chat.Title}");
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
                if (_adminsIds!=null && text.Length == 1 && text.Equals(".", StringComparison.InvariantCultureIgnoreCase) && _adminsIds.Contains(message.From.Id))
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
                    await botClient.SendTextMessageWhithDelayAsync(isEnabled: true, message, message.Chat,alert, new TimeSpan(0, 0, 10), parseMode: ParseMode.Html, replyToMessageId: message.ReplyToMessage?.MessageId ?? -1,
                         allowSendingWithoutReply: true, disableWebPagePreview: true, cancellationToken: cancellationToken).ConfigureAwait(false);
                    return;
                }

                else
                {
                    string trimmedText = text.Remove(text.Length - 1, 1);
                    bool res = long.TryParse(trimmedText.Trim(), out long temp);
                    if (res && message.Type == MessageType.Text)
                    {
                        _ = await Task.Factory.StartNew(async () =>
                        {
                            if (res && text.EndsWith('f') || text.EndsWith('F'))
                            {
                                Message result = await botClient.SendTextMessageAsync(message.Chat, $" {text} = {(temp - 32) * 5 / 9}C° для {user.FullName} !", cancellationToken: cancellationToken);
                                await botClient.DeleteMessageAsync(chatId: message.Chat, message.MessageId, cancellationToken);
                                await Task.Delay(3000, cancellationToken);
                                await botClient.DeleteMessageAsync(chatId: result.Chat, result.MessageId, cancellationToken);
                                return;
                            }
                            else if (res && text.EndsWith('c') || text.EndsWith('C') || text.EndsWith('с') || text.EndsWith('С'))

                            {
                                Message result = await botClient.SendTextMessageAsync(message.Chat, $" {text} = {(temp * 9 / 5) + 32}F° для {user.FullName} !", cancellationToken: cancellationToken);
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
                long chatId = update.CallbackQuery.Message.Chat.Id;
                int messId = update.CallbackQuery.Message.MessageId;
                string codeOfButton = update.CallbackQuery.Data;
                string telegramMessage = codeOfButton;
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
                else if (!_fruitsArr.Contains(codeOfButton, StringComparer.OrdinalIgnoreCase))
                {
                    codeOfButton = codeOfButton.Substring(1);
                    await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, $@"https://t.me/{codeOfButton}", true, null, 5, cancellationToken);
                    await botClient.DeleteMessageAsync(chatId, messId, cancellationToken);
                }
            }
        }
      
       

      


        #region Inline buttons
        private async void SendInlineAdmins(ITelegramBotClient botClient, ChatMember[] members, Telegram.Bot.Types.Chat chat, Message message, long chatId, CancellationToken cancellationToken)
        {
            IEnumerable<IEnumerable<InlineKeyboardButton>> btnArr = GetButtons(members);
            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(btnArr);
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
            foreach (ChatMember member in members)
            {
                yield return new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData(text: $"{member.User.FirstName} {member.User.LastName}", callbackData: $"@{member.User.Username}")
                };
            }
        }
        private IEnumerable<IEnumerable<InlineKeyboardButton>> GetButtonsForHello()
        {
            foreach (string fruit in _fruitsArr)
            {
                yield return new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData(text: fruit, callbackData: fruit)
                };
            }
        }
        private async Task<Message> SendInAntiSpamline(ITelegramBotClient botClient, Message message, ChatUser user, string fruit, Update update, CancellationToken cancellationToken)
        {

            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(
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
            Message messag = await botClient.SendTextMessageAsync(chatId: message.Chat.Id, $"ВАЖНО: {user.FullName}  @{user.UserName}, если ты " +
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
                                 , showAlert: true, url: null,null, cancellationToken: cancellationToken);
                await botClient.DeleteMessageAsync(chatId, messId, cancellationToken);
            }
            catch
            {
            }

        }


        #endregion income
    }
}
