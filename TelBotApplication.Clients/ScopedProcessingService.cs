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
        private readonly IUnitOfWork _unitOfWork;
        private IEnumerable<VenueRequest> _venueRequests;
        private IEnumerable<BotCommandDto> _botCommands;
        private ChatMember[] _admins;
        private readonly ITextFilter _textFilter;
        private readonly IHubContext<MemberHub, INewMember> _memberHub;
        private ITelegramBotClient _bot { get; set; }
        private readonly string[] _fruitsArr;
        private CallBackUser _callBackUser;
        private string _fruit;
        private readonly Random _rnd;
        private ChatUser _incomingUser = default;
        private CancellationTokenSource cts;
        private CancellationTokenSource _ctsHello;
        private List<UserRepeater> _counterKessages;


        public ScopedProcessingService(IServiceProvider serviceProvider,
        IOptions<EnvironmentBotConfiguration> options,
        ILogger<BotClientService> logger,
        IMapper mapper,
        ITextFilter textFilter,
        IUnitOfWork unitOfWork,
        IHubContext<MemberHub, INewMember> memberHub)
        {
            _config = options.Value;
            _bot = new TelegramBotClient(_config.Token);/*flud*/
            _logger = logger;
            _mapper = mapper;
            _textFilter = textFilter;
            _rnd = new Random();
            _fruitsArr = new string[] { "🍎", "🍌", "🍒", "🍍", "🍋", "🍉" };
            _unitOfWork = unitOfWork;
            _memberHub = memberHub;
            _counterKessages= new List<UserRepeater>();


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

             await Task.WhenAll(pollingTask, dbUpdaterTask);
           
        }


        private void _memberExecutor_RestrictEvent(Message message, CancellationToken cancellationToken = default)
        {
            Task.Factory.StartNew(async () =>
            {
                await _bot.DeleteMessageAsync(message.Chat.Id, message.MessageId, cancellationToken);
                await _bot.RestrictChatMemberAsync(message.Chat.Id, userId: _callBackUser.UserId, new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false }, untilDate: DateTime.Now.AddMinutes(5), cancellationToken);
                Message result = await _bot.SendTextMessageAsync(chatId: message.Chat.Id, $"ВАЖНО: Ты не нажал(а) кнопку, значит ты БОТ или СПАМЕР, в тестовом режиме РО на пять минут \n Пока можно изучить правила чата \n" +
                    $@"https://t.me/winnersDV2022flood/3", cancellationToken: cancellationToken);
                await Task.Delay(5000, cancellationToken);
                await _bot.DeleteMessageAsync(result.Chat.Id, result.MessageId, cancellationToken);
            });

        }
        private void _memberExecutor_AlertEvent(Message message, CancellationToken cancellationToken = default)
        {
            Task.Factory.StartNew(async () =>
            {
                Message result = await _bot.SendTextMessageAsync(chatId: message.Chat, $"@{_incomingUser.FirstName}   {_incomingUser.UserName}," +
                                          $" пожалуйста выполни проверку на антиспам https://t.me/{message.Chat.Username}/{_incomingUser.MessageId}", disableWebPagePreview: true, cancellationToken: cancellationToken);
                await Task.Delay(10000, cancellationToken);
                await _bot.DeleteMessageAsync(result.Chat.Id, result.MessageId, cancellationToken);
            });

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
                IEnumerable<VenueCommand> locations = await _unitOfWork.VenueCommandServise.GetAllAsync();
                IEnumerable<BotCaller> list = await _unitOfWork.BotCommandService.GetAllAsync();
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


            CancellationToken token = _ctsHello.Token;
            ChatMessage chat_message = new ChatMessage(update);
            ChatUser user = chat_message.GetCurrentUser();
            Message message = chat_message.GetCurrentMessage();
            long userId = user.UserId;
            string text = chat_message.GetCurrentMessageText();
            // Некоторые действия
            _logger.LogDebug(Newtonsoft.Json.JsonConvert.SerializeObject(update));

            if (update.Type == UpdateType.Message)
            {
                _admins = await botClient.GetChatAdministratorsAsync(update.Message.Chat);
                if (text != null && _botCommands!=null && _botCommands.Any(x => text.Contains(x.Command.Trim(), StringComparison.OrdinalIgnoreCase)))
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

                if (text != null && _venueRequests!=null && _venueRequests.Any(x => text.Contains(x.Command.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    VenueRequest location = _venueRequests.FirstOrDefault(x => text.Contains(x.Command.Trim(), StringComparison.OrdinalIgnoreCase));
                    await botClient.SendVenueWithDelayAsync(true, new TimeSpan(0, 0, 30), message, location, message.Chat, cancellationToken: cancellationToken);

                    return;
                }
              

                if (chat_message.GetCurrentMessageText().Contains("/gay", StringComparison.OrdinalIgnoreCase))
                {
                    var number = _rnd.Next(1, 100);
                    var linl = string.Empty;
                    if (number >= 50)
                    {
                        linl = $"Вероятность того , что вы, уважаемый, {user.UserName} - гей <b>высокая</b> -  {number}%";
                    }
                    else
                    {
                        linl = $"Вероятность того , что вы, уважаемый, {user.UserName} -  гей <b>низкая</b> -   {number}%";
                    }
                    await botClient.SendTextMessageWhithDelayAsync(isEnabled: true, message, message.Chat, linl, new TimeSpan(0, 0, 30), parseMode: ParseMode.Html, replyToMessageId: message.ReplyToMessage?.MessageId ?? -1,
                       allowSendingWithoutReply: true, disableWebPagePreview: true, cancellationToken: cancellationToken).ConfigureAwait(false);
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
                if (message?.Type != null && message.Type == MessageType.ChatMembersAdded)
                {
                    int index = _rnd.Next(_fruitsArr.Length);
                    _fruit = _fruitsArr[index];
                    _callBackUser = new CallBackUser { UserId = user.UserId };
                    Message messageHello = await SendInAntiSpamline(botClient: botClient, message: message, user, _fruit, update, cancellationToken: cancellationToken);
                    return;
                }
                if (message?.Type != null && message.Type == MessageType.ChatMemberLeft)
                {
                    await botClient.SendAnimationWhithDelayAsync(isEnabled: true, message, message.Chat, animation: "https://i.gifer.com/ABMO.gif",
                        new TimeSpan(0, 0, 30), caption: $"Прощай дорогой {user.FullName}.\n Нам будет тебя не хватать (((((", cancellationToken: cancellationToken);
                    return;
                }
            }
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.EditedMessage)
            {
                await _memberHub.Clients.All.EditLog($"{update.EditedMessage.Chat.Id}:{user.MessageId}:{update.EditedMessage.From.FirstName ?? "no firstName"} {update.EditedMessage.From.LastName ?? "no lastName"}:{update.EditedMessage.From.Username??"no userName"}:{text}");
                return;
            }

            if (message?.Type != null && message.Type == MessageType.Text)
            {
                await _memberHub.Clients.All.SendLog($"{message.Chat.Id}:{user.MessageId}:{user.FullName}:{user.UserName}:{text}");
                if (_counterKessages.Count == 0 && message.ReplyToMessage?.MessageId != null)
                {
                    _counterKessages.Add(new UserRepeater { UserId = user.UserId, ReplyToMessageId = message.ReplyToMessage?.MessageId ?? 1 });
                    
                }

                else if (_counterKessages.Count == 1 && message.ReplyToMessage?.MessageId != null && _counterKessages.Any(x => x.UserId == user.UserId && x.ReplyToMessageId == message.ReplyToMessage?.MessageId))
                {
                    await botClient.SendTextMessageWhithDelayAsync(isEnabled: true, message, message.Chat, @$"<b>{user.FullName}</b>, это нарушение: несколько сообщений подряд в
                        чат запрещено отправлять пунктом 2.2 правил.! https://t.me/winnersDV2022flood/3", new TimeSpan(0, 0, 10), parseMode: ParseMode.Html, replyToMessageId: message.MessageId,
                   allowSendingWithoutReply: false, disableWebPagePreview: true, cancellationToken: cancellationToken).ConfigureAwait(false);
                    _counterKessages.Clear();
                    return;
                }
                else if (_counterKessages.Count == 1 && !_counterKessages.Any(x => x.UserId == user.UserId))
                {
                    _counterKessages.Clear();
                   
                }
                if (_textFilter.IsAlertFrase(text.Trim().ToLower(CultureInfo.InvariantCulture)))
                {
                    await botClient.SendTextMessageWhithDelayAsync(isEnabled: true, message, message.Chat, $@"<b>{user.FullName}</b>, это нарушение п. 13 правил." +
                $" Вопросы основного чата обсуждаем только там.В следующий раз будет РО.! https://t.me/winnersDV2022flood/3", new TimeSpan(0, 0, 10),
                 disableWebPagePreview: true, cancellationToken: cancellationToken);
                    return;
                }
                if ((text.Length == 1 && text.Equals(".", StringComparison.InvariantCultureIgnoreCase) && /*message.From.Id == 1087968824 || */_admins.Any(x => x.User.Id == message.From.Id)))
                {
                    await botClient.SendTextMessageWhithDelayAsync(isEnabled: true, message, message.Chat, $"Нарушение п. 13 правил." +
                       $" Вопросы основного чата обсуждаем только там.В следующий раз будет РО.! https://t.me/winnersDV2022flood/3", new TimeSpan(0, 0, 30), parseMode: ParseMode.Html, replyToMessageId: message.ReplyToMessage?.MessageId ?? -1,
                        allowSendingWithoutReply: true, disableWebPagePreview: true, cancellationToken: cancellationToken).ConfigureAwait(false);
                    return;
                }
                else if ((text.Length == 1 && text.Equals(".", StringComparison.InvariantCultureIgnoreCase)  && !_admins.Any(x => x.User.Id == message.From.Id)))
                {
                    await botClient.DeleteMessageAsync(message.Chat, message.MessageId, cancellationToken);
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
            //if (message?.Type != null && message.Type == Telegram.Bot.Types.Enums.MessageType.Photo)
            //{
            //    _ = await Task.Factory.StartNew(async () =>
            //    {
            //        Message result = await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
            //                                         $"Спасибо за фото, дорогой(ая) {message.From.FirstName} {message.From.LastName} ",
            //                                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, null, disableWebPagePreview: true, cancellationToken: cancellationToken);

            //        await Task.Delay(2000, cancellationToken);
            //        await botClient.DeleteMessageAsync(result.Chat.Id, result.MessageId, cancellationToken);
            //    }, cancellationToken).ConfigureAwait(false);
            //    return;
            //}
            //if (message?.Type != null && message.Type == Telegram.Bot.Types.Enums.MessageType.Video)
            //{
            //    _ = await Task.Factory.StartNew(async () =>
            //    {
            //        Message result = await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
            //                                        $"Спасибо за видео, дорогой(ая) {message.From.FirstName} {message.From.LastName} ",
            //                                       parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, null, disableWebPagePreview: true, cancellationToken: cancellationToken);

            //        await Task.Delay(2000, cancellationToken);
            //        await botClient.DeleteMessageAsync(result.Chat.Id, result.MessageId, cancellationToken);
            //    }, cancellationToken).ConfigureAwait(false);
            //    return;
            //}
            //if (message?.Type != null && message.Type == Telegram.Bot.Types.Enums.MessageType.Document)
            //{
            //    _ = await Task.Factory.StartNew(async () =>
            //    {
            //        Message result = await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
            //                                        $"Спасибо за документ, дорогой(ая) {message.From.FirstName} {message.From.LastName} ",
            //                                       parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, null, disableWebPagePreview: true, cancellationToken: cancellationToken);

            //        await Task.Delay(2000, cancellationToken);
            //        await botClient.DeleteMessageAsync(result.Chat.Id, result.MessageId, cancellationToken);
            //    }, cancellationToken).ConfigureAwait(false);
            //    return;
            //}
            //if (message?.Type != null && message.Type == Telegram.Bot.Types.Enums.MessageType.Sticker)
            //{
            //    _ = await Task.Factory.StartNew(async () =>
            //    {
            //        Message result = await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
            //                                        $"Спасибо за стикер, дорогой(ая) {message.From.FirstName} {message.From.LastName} ",
            //                                       parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, null, disableWebPagePreview: true, cancellationToken: cancellationToken);

            //        await Task.Delay(2000, cancellationToken);
            //        await botClient.DeleteMessageAsync(result.Chat.Id, result.MessageId, cancellationToken);
            //    }, cancellationToken).ConfigureAwait(false);
            //    return;
            //}
            
            #endregion Auto answer


            if (update.Type == UpdateType.CallbackQuery)
            {
                long chatId = update.CallbackQuery.Message.Chat.Id;
                int messId = update.CallbackQuery.Message.MessageId;
                string codeOfButton = update.CallbackQuery.Data;
                string telegramMessage = codeOfButton;
                if (_fruitsArr.Contains(codeOfButton, StringComparer.Ordinal) && _callBackUser != null && _callBackUser.UserId == update.CallbackQuery.From.Id)
                {

                    if (codeOfButton.Equals(_fruit))
                    {
                        await SuccessfullNewMemberAddedAsync(botClient, update, chatId, messId, cancellationToken);
                    }
                    else
                    {
                        await UnsuccessfullNewMemberAddedWithRestrictAsync(botClient, update, chatId, messId, cancellationToken);
                    }
                    _callBackUser = null;
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
                               + $" троллить Джобса, сербскую еду, подгонять Данзана, а также йожек. Правила чата необходимо неукоснительно соблюдать!"
                                 , showAlert: true, url: null, cacheTime: 600, cancellationToken: cancellationToken);
                await botClient.DeleteMessageAsync(chatId, messId, cancellationToken);
            }
            catch 
            {
            }
           
        }


        #endregion income
    }
}
