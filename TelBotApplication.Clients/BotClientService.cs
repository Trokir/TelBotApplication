using AutoMapper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TelBotApplication.DAL;
using TelBotApplication.Domain.Abstraction;
using TelBotApplication.Domain.Chats;
using TelBotApplication.Domain.Dtos;
using TelBotApplication.Domain.Enums;
using TelBotApplication.Domain.Models;
using TelBotApplication.Filters;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelBotApplication.Clients
{
    public class BotClientService : BackgroundService
    {
        private readonly EnvironmentBotConfiguration _config;
        private readonly ILogger<BotClientService> _logger;
        private IEnumerable<BotCommandDto> _botCommands;
        private IEnumerable<VenueRequest> _venueRequests;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _commandService;
        private readonly IFludFilter _fludFilter;
        private readonly IMemberExecutor _newmembersService;
        private ChatMember[] _admins;
        private ITelegramBotClient _bot { get; set; }
        private readonly string[] _fruitsArr;
        private CallBackUser _callBackUser;
        private string _fruit;
        private readonly Random _rnd;

        private CancellationTokenSource cts;
        private CancellationTokenSource _ctsHello;

        public BotClientService(IOptions<EnvironmentBotConfiguration> options,
            ILogger<BotClientService> logger,
            IMapper mapper,
            IUnitOfWork commandService,
            IFludFilter fludFilter,
            IMemberExecutor newmembersService
            )
        {
            _config = options.Value;
            _bot = new TelegramBotClient(_config.Token);/*flud*/
            _logger = logger;
            _mapper = mapper;
            _newmembersService = newmembersService;
            _commandService = commandService;
            _rnd = new Random();
            _fludFilter = fludFilter;
            _fruitsArr = new string[] { "🍎", "🍌", "🍒", "🍍", "🍋", "🍉" };
            _newmembersService.AlertEvent += _newmembersService_AlertEvent;
            _newmembersService.RestrictEvent += _newmembersService_RestrictEvent;
            _newmembersService.RunAlertPolling();
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            cts = new CancellationTokenSource();
            CancellationToken cancellationToken = cts.Token;
            Task pollingTask = RunBotPolling(cancellationToken);
            Task dbUpdaterTask = AddCommandsListForBot(cancellationToken);


            _ = await Task.WhenAny(pollingTask, dbUpdaterTask);

        }
        private async Task _newmembersService_RestrictEvent(Message message, CancellationToken cancellationToken = default)
        {
            await _bot.DeleteMessageAsync(message.Chat.Id, message.MessageId, cancellationToken);
            await _bot.RestrictChatMemberAsync(message.Chat.Id, userId: _callBackUser.UserId, new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false }, untilDate: DateTime.Now.AddMinutes(5), cancellationToken);
            var result = await _bot.SendTextMessageAsync(chatId: message.Chat.Id, $"ВАЖНО: Ты не нажал(а) кнопку, значит ты БОТ или СПАМЕР, в тестовом режиме РО на пять минут \n Пока можно изучить правила чата \n" +
                $@" < a href = 'https://t.me/winnersDV2022flood/3' >< /a > ", cancellationToken: cancellationToken);
            await Task.Delay(5000, cancellationToken);
            await _bot.DeleteMessageAsync(result.Chat.Id, result.MessageId, cancellationToken);
        }

        private async Task _newmembersService_AlertEvent(Message message, CancellationToken cancellationToken = default)
        {
            var result = await _bot.SendTextMessageAsync(chatId: message.Chat, $"@{message.From.Username}," +
                        $" пожалуйста выполни проверку на антиспам https://t.me/{message.From.Username ?? " "}/{message.MessageId}", disableWebPagePreview: true, cancellationToken: cancellationToken);
            await Task.Delay(10000, cancellationToken);
            await _bot.DeleteMessageAsync(result.Chat.Id, result.MessageId, cancellationToken);
        }


        #region Start
        private async Task RunBotPolling(CancellationToken cancellationToken)
        {
            ReceiverOptions receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new Telegram.Bot.Types.Enums.UpdateType[]
                {

                },
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
            catch { }
        }
        private async Task AddCommandsListForBot(CancellationToken cancellationToken)
        {
            while (true)
            {
                IEnumerable<VenueCommand> locations = await _commandService.VenueCommandServise.GetAllAsync();
                IEnumerable<BotCaller> list = await _commandService.BotCommandService.GetAllAsync();
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

        #region Reactions
        private async Task SendReactionByBotCommandWithAnimationAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Chat chat, Message message, string link = "", string caption = "", CancellationToken cancellationToken = default)
        {
            Message result = await botClient.SendAnimationAsync(chatId: chat, animation: link, caption: caption, cancellationToken: cancellationToken);
            await botClient.DeleteMessageAsync(chatId: chat, message.MessageId, cancellationToken);
            await Task.Delay(30000);
            await botClient.DeleteMessageAsync(chatId: result.Chat, result.MessageId, cancellationToken);
        }
        private async Task SendReactionByBotCommandWithTextAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Chat chat, Message message, ChatUser user, string link = "", string caption = "", CancellationToken cancellationToken = default)
        {
            Message result = await botClient.SendTextMessageAsync(message.Chat, $" {caption} {user.GetFullName()} !", cancellationToken: cancellationToken);
            await botClient.DeleteMessageAsync(chatId: chat, message.MessageId, cancellationToken);
            await Task.Delay(30000);
            await botClient.DeleteMessageAsync(chatId: result.Chat, result.MessageId, cancellationToken);
        }
        private async Task SendReactionByBotCommandWithLocationAsync(ITelegramBotClient botClient, Message message, string text, CancellationToken cancellationToken)
        {
            _ = await Task.Factory.StartNew(async () =>
            {
                VenueRequest location = _venueRequests.FirstOrDefault(x => text.Contains(x.Command.ToLower().Trim()));
                Message result = await botClient.SendVenueAsync(chatId: message.Chat, latitude: location.Latitude, longitude: location.Longitude, title: location.Title, address: location.Address, cancellationToken: cancellationToken);
                await botClient.DeleteMessageAsync(chatId: message.Chat, message.MessageId, cancellationToken);
                await Task.Delay(30000);
                await botClient.DeleteMessageAsync(chatId: result.Chat, result.MessageId, cancellationToken);
            }, cancellationToken);
        }
        #endregion

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
        private async Task<Message> SendInAntiSpamline(ITelegramBotClient botClient, Message message, ChatUser user, string fruit, CancellationToken cancellationToken)
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

            Message messag = await botClient.SendTextMessageAsync(chatId: message.Chat.Id, $"ВАЖНО: {user.GetFullName()}  @{user.UserName}, если ты " +
                $"не БОТ и не СПАМЕР, пройди проверку, нажав на кнопку, где есть {fruit}", parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: keyboard,
               cancellationToken: cancellationToken);

            return messag;
        }
        #endregion


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
            long userId = user.GetUserId();
            string text = chat_message.GetCurrentMessageText();
            // Некоторые действия
            _logger.LogDebug(Newtonsoft.Json.JsonConvert.SerializeObject(update));

            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                if (text != null && _botCommands.Any(x => text.Contains(x.Command.ToLower().Trim())))
                {
                    _ = await Task.Factory.StartNew(async () =>
                    {
                        BotCommandDto command = _botCommands.FirstOrDefault(x => text.Contains(x.Command.ToLower().Trim()));
                        switch (command.TypeOfreaction)
                        {
                            case TypeOfreactions.Text:
                                await SendReactionByBotCommandWithTextAsync(botClient, message.Chat, message, user, "", command.Caption, cancellationToken);
                                return;
                            case TypeOfreactions.Animation:
                                await SendReactionByBotCommandWithAnimationAsync(botClient, message.Chat, message, command.Link, command.Caption, cancellationToken);
                                return;
                        }
                    }, cancellationToken);
                }

                if (text != null && _venueRequests.Any(x => text.Contains(x.Command.ToLower().Trim())))
                {
                    await SendReactionByBotCommandWithLocationAsync(botClient, message, text, cancellationToken);
                    return;
                }


                if (chat_message.GetCurrentMessageText().ToLower().Contains("/stat"))
                {
                    int count = await botClient.GetChatMemberCountAsync(message.Chat, cancellationToken);
                    ChatMember[] countAdmins = await botClient.GetChatAdministratorsAsync(message.Chat, cancellationToken);
                    await SendReactionByBotCommandWithTextAsync(botClient, message.Chat, message, user, "", $"В чате юзеров {count} ; администраторов {countAdmins.Length}  от ", cancellationToken);
                    return;
                }

                if (chat_message.GetCurrentMessageText().ToLower().Contains("/admin"))
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
                if (message?.Type != null && message.Type == Telegram.Bot.Types.Enums.MessageType.ChatMembersAdded)
                {
                    int index = _rnd.Next(_fruitsArr.Length);
                    _fruit = _fruitsArr[index];
                    _callBackUser = new CallBackUser { UserId = user.GetUserId() };
                    Message messageHello = await SendInAntiSpamline(botClient: botClient, message: message, user, _fruit, cancellationToken: cancellationToken);
                    _newmembersService.AddNewMember(messageHello, DateTime.Now);
                    return;
                }
                if (message?.Type != null && message.Type == Telegram.Bot.Types.Enums.MessageType.ChatMemberLeft)
                { 
                    await SendReactionByBotCommandWithAnimationAsync(botClient, message.Chat, message, "https://i.gifer.com/ABMO.gif", $"Прощай дорогой {user.GetFullName()}.\n Нам будет тебя не хватать (((((");
                    return;
                }
            }

            if (message?.Type != null && message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
            {
                //if (text.Trim() == "poll")
                //{
                //    Message pollMessage = await botClient.SendPollAsync(
                //     chatId: message.Chat,
                //     question: "Что выбрать : Рио или Сантьяго ?",
                //     options: new[]
                //     {
                //         "Рио",
                //         "Сантьяго"
                //     },
                //     cancellationToken: cancellationToken);

                //}
                if ((text.Trim().Length < 2))
                {
                    _ = await Task.Factory.StartNew(async () =>
                    {
                        // string texto = @"<b>bold</b>, <strong> bold </strong>" +
                        //@"<i> italic </i>, <em> italic </em>" +
                        //@"<a href = 'https://t.me/winnersDV2022flood/3'></a>"+
                        //@"<a href = 'tg://user?id=123456789'> inline mention of a user</a>" +
                        //@"<code> inline fixed-width code </code>" +
                        //@"<pre> pre - formatted fixed-width code block</pre>";

                        //Message result = await botClient.SendTextMessageAsync(message.Chat, texto, disableWebPagePreview: true, cancellationToken: cancellationToken);
                        //await botClient.DeleteMessageAsync(chatId: message.Chat, message.MessageId, cancellationToken);
                        //await Task.Delay(10000);
                        //await botClient.DeleteMessageAsync(chatId: result.Chat, result.MessageId, cancellationToken);

                        Message result = await botClient.SendTextMessageAsync(message.Chat, $"Нарушение п. 13 правил." +
                       $" Вопросы основного чата обсуждаем только там.В следующий раз будет РО.! https://t.me/winnersDV2022flood/3", disableWebPagePreview: true, cancellationToken: cancellationToken);
                        await botClient.DeleteMessageAsync(chatId: message.Chat, message.MessageId, cancellationToken);
                        await Task.Delay(10000);
                        await botClient.DeleteMessageAsync(chatId: result.Chat, result.MessageId, cancellationToken);
                    });

                    return;
                }
                else
                {
                    string trimmedText = text.Remove(text.Length - 1, 1);
                    bool res = long.TryParse(trimmedText.Trim(), out long temp);
                    if (res && message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
                    {
                        _ = await Task.Factory.StartNew(async () =>
                        {
                            if (res && text.EndsWith('f') || text.EndsWith('F'))
                            {
                                Message result = await botClient.SendTextMessageAsync(message.Chat, $" {text} = {(temp - 32) * 5 / 9}C° для {user.GetFullName()} !", cancellationToken: cancellationToken);
                                await botClient.DeleteMessageAsync(chatId: message.Chat, message.MessageId, cancellationToken);
                                await Task.Delay(3000);
                                await botClient.DeleteMessageAsync(chatId: result.Chat, result.MessageId, cancellationToken);
                                return;
                            }
                            else if (res && text.EndsWith('c') || text.EndsWith('C') || text.EndsWith('с') || text.EndsWith('С'))

                            {
                                Message result = await botClient.SendTextMessageAsync(message.Chat, $" {text} = {(temp * 9 / 5) + 32}F° для {user.GetFullName()} !", cancellationToken: cancellationToken);
                                await botClient.DeleteMessageAsync(chatId: message.Chat, message.MessageId, cancellationToken);
                                await Task.Delay(3000);
                                await botClient.DeleteMessageAsync(chatId: result.Chat, result.MessageId, cancellationToken);
                                return;
                            }
                        });
                    }
                    #region Text Auto Answer
                    //_ = await Task.Factory.StartNew(async () =>
                    //{
                    //    //         string texto = @"<b>bold</b>, <strong> bold </strong>" +
                    //    //@"<i> italic </i>, <em> italic </em>" +
                    //    //@"<a href = 'https://t.me/winnersDV2022flood/3'></a>"
                    //    //@"<a href = 'tg://user?id=123456789'> inline mention of a user</a>" +
                    //    //@"<code> inline fixed-width code </code>" +
                    //    //@"<pre> pre - formatted fixed-width code block</pre>";


                    //    bool spam = _fludFilter.CheckIsSpam(update.Message.Text.Trim());
                    //    bool spam_or_ham = _fludFilter.CheckIsSpamOrHam(update.Message.Text.Trim());
                    //    Message result = await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                    //                                   $@"<pre>Спасибо за сообщение, дорогой(ая) <b>{message.From.FirstName} {message.From.LastName}</b>  is spam = {spam}; is spam_or_ham= {spam_or_ham}</pre>",
                    //                                  parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, null, disableWebPagePreview: true, cancellationToken: cancellationToken);

                    //    await Task.Delay(2000, cancellationToken);
                    //    await botClient.DeleteMessageAsync(result.Chat.Id, result.MessageId, cancellationToken);
                    //}, cancellationToken).ConfigureAwait(false);
                    #endregion Text Auto Answer
                }
                //////////


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
            //if (update.Type == Telegram.Bot.Types.Enums.UpdateType.EditedMessage)
            //{
            //    _ = await Task.Factory.StartNew(async () =>
            //    {
            //        Message result = await botClient.SendTextMessageAsync(chatId: update.EditedMessage.Chat.Id,
            //                                        $"Спасибо за исправление сообщения, дорогой(ая) {update.EditedMessage.From.FirstName} {update.EditedMessage.From.LastName} ",
            //                                       parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, null, disableWebPagePreview: true, cancellationToken: cancellationToken);

            //        await Task.Delay(2000, cancellationToken);
            //        await botClient.DeleteMessageAsync(result.Chat.Id, result.MessageId, cancellationToken);
            //    }, cancellationToken).ConfigureAwait(false);
            //    return;
            //}
            #endregion Auto answer


            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
            {
                long chatId = update.CallbackQuery.Message.Chat.Id;
                int messId = update.CallbackQuery.Message.MessageId;
                string codeOfButton = update.CallbackQuery.Data;
                string telegramMessage = codeOfButton;
                if (_fruitsArr.Contains(codeOfButton) && _callBackUser != null && _callBackUser.UserId == update.CallbackQuery.From.Id)
                {

                    if (codeOfButton.Equals(_fruit))
                    {
                        await SuccessfullNewMemberAddedAsync(botClient, update, chatId, messId, cancellationToken);
                    }
                    else
                    {
                        await UnsuccessfullNewMemberAddedWithRestrictAsync(botClient, update, chatId, messId, cancellationToken);
                    }
                    _newmembersService.DropNewMember(update.CallbackQuery.Message);
                    _callBackUser = null;
                    return;
                }
                else if (!_fruitsArr.Contains(codeOfButton))
                {
                    codeOfButton = codeOfButton.Substring(1);
                    await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, $@"https://t.me/{codeOfButton}", true, null, 5, cancellationToken);
                    await botClient.DeleteMessageAsync(chatId, messId, cancellationToken);
                }
            }
        }




        #region income
        private async Task UnsuccessfullNewMemberAddedWithRestrictAsync(ITelegramBotClient botClient, Update update, long chatId, int messId, CancellationToken cancellationToken)
        {
            await botClient.AnswerCallbackQueryAsync(callbackQueryId: update.CallbackQuery.Id, text: $"<b>{update.CallbackQuery.From.FirstName} {update.CallbackQuery.From.LastName}</b>, " +
                $"Вы не прошли проверку на антиспам, вы будете лишены возможности комментировать в группе в течение одного часа."
                , showAlert: true, url: null, cacheTime: 15, cancellationToken: cancellationToken);
            await botClient.DeleteMessageAsync(chatId, messId, cancellationToken);
            await Task.Delay(2000);
            await botClient.RestrictChatMemberAsync(chatId, userId: _callBackUser.UserId,
               new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false }, untilDate: DateTime.Now.AddSeconds(31), cancellationToken);
        }

        private static async Task SuccessfullNewMemberAddedAsync(ITelegramBotClient botClient, Update update, long chatId, int messId, CancellationToken cancellationToken)
        {
            await botClient.AnswerCallbackQueryAsync(callbackQueryId: update.CallbackQuery.Id, text: $@"{update.CallbackQuery.From.FirstName} {update.CallbackQuery.From.LastName} , приветствуем вас в ламповой и дружной флудилке! Здесь любят фудпорн, "
                + $" троллить Джобса, сербскую еду, подгонять Данзана, а также йожек. Правила чата необходимо неукоснительно соблюдать!"
                  , showAlert: true, url: null, cacheTime: 15, cancellationToken: cancellationToken);
            await botClient.DeleteMessageAsync(chatId, messId, cancellationToken);
        }


        #endregion income




        public Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
        {
            if (exception is ApiRequestException)
            {
                _logger.LogError(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
            }
            // Некоторые действия
            return Task.CompletedTask;
        }





    }
}
