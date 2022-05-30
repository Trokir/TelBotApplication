using AutoMapper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TelBotApplication.DAL.Services;
using TelBotApplication.Domain.Abstraction;
using TelBotApplication.Domain.Chats;
using TelBotApplication.Domain.Dtos;
using TelBotApplication.Domain.Enums;
using TelBotApplication.Domain.Models;
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
        private readonly IMapper _mapper;
        private readonly IBotCommandService _commandService;
        private ChatMember[] _admins;
        private ITelegramBotClient _bot { get; set; }

        readonly string[] _fruitsArr = new string[] { "🍎", "🍌", "🍒", "🍍" };
        private CallBackUser _callBackUser;
        private string _fruit;
        private readonly Random _rnd;

        private CancellationTokenSource cts;
        private CancellationTokenSource _ctsHello;

        public BotClientService(IOptions<EnvironmentBotConfiguration> options, ILogger<BotClientService> logger, IMapper mapper, IBotCommandService commandService)
        {
            _config = options.Value;
            _bot = new TelegramBotClient(_config.Token);/*flud*/
            _logger = logger;
            _mapper = mapper;
            _commandService = commandService;
            _rnd = new Random();

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            cts = new CancellationTokenSource();
            CancellationToken cancellationToken = cts.Token;
            Task pollingTask = RunBotPolling(cancellationToken);
            Task dbUpdaterTask = AddCommandsListForBot(cancellationToken);
            _ = await Task.WhenAny(pollingTask, dbUpdaterTask);
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

            await foreach (Update update in updateReceiver.WithCancellation(cts.Token))
            {
                if (update is Update message)
                {
                    await HandleUpdateAsync(_bot, message, cancellationToken);
                }
            }
        }
        private async Task AddCommandsListForBot(CancellationToken cancellationToken)
        {
            while (true)
            {
                IEnumerable<BotCaller> list = await _commandService.GetAllCommandsAsync();
                _botCommands = _mapper.Map<IEnumerable<BotCommandDto>>(list);
                List<BotCommand> commandsList = new List<BotCommand>();
                foreach (BotCommandDto item in _botCommands)
                {
                    commandsList.Add(new BotCommand
                    {
                        Command = item.Command,
                        Description = item.Description
                    });
                }
                await _bot.SetMyCommandsAsync(commandsList, BotCommandScope.AllGroupChats(), languageCode: "en", cancellationToken);
                await Task.Delay(new TimeSpan(0, 15, 0), cancellationToken);
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
                    }
                });
            Message messag = await botClient.SendTextMessageAsync(chatId: message.Chat.Id, $"ВАЖНО: {user.GetFullName()}  @{user.UserName}, если ты не БОТ и не СПАМЕР, пройди проверку, нажав на кнопку, где есть {fruit}", parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: keyboard,
               cancellationToken: cancellationToken);

            return messag;
        }
        #endregion

        #region AddNewUser
        private async Task RunTaskTimerAsync(ITelegramBotClient botClient, long chatId, int messageId, string userName, TimeSpan interval, ChatUser user, CancellationToken cancellationToken)
        {
            _ = await Task.Factory.StartNew(async () =>
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(interval, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();
                    Message result1 = await botClient.SendTextMessageAsync(chatId: chatId, $"@{user.UserName}, пожалуйста выполни проверку на антиспам https://t.me/{userName ?? " "}/{messageId}", disableWebPagePreview: true, cancellationToken: cancellationToken);
                    await Task.Delay(5000, cancellationToken);
                    await botClient.DeleteMessageAsync(result1.Chat.Id, result1.MessageId, cancellationToken);
                    await Task.Delay(interval, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();
                    await botClient.DeleteMessageAsync(chatId, messageId, cancellationToken);
                    await botClient.RestrictChatMemberAsync(chatId, userId: _callBackUser.UserId, new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false }, untilDate: DateTime.Now.AddMinutes(1), cancellationToken);
                    Message result = await botClient.SendTextMessageAsync(chatId: chatId, $"ВАЖНО: Ты не нажал(а) кнопку, значит ты БОТ или СПАМЕР, БАН на 100 лет", cancellationToken: cancellationToken);
                    await Task.Delay(5000, cancellationToken);
                    await botClient.DeleteMessageAsync(result.Chat.Id, result.MessageId, cancellationToken);
                }
            }, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }
        private async Task SendHelloForNewMemberAsync(ITelegramBotClient botClient, string userName, Message message, CancellationToken cancellationToken)
        {
            Message result = await botClient.SendTextMessageAsync(message.Chat, $"{userName}, приветствуем вас в ламповой и дружной флудилке! \n Здесь любят фудпорн," +
                                   $" троллить Джобса, сербскую еду, подгонять Данзана, а также йожек.\n Но есть тут и правила:\n https://t.me/winnersDV2022flood/3 \n Их необходимо неукоснительно соблюдать!", disableWebPagePreview: true);

            await Task.Delay(10000, cancellationToken);

            await botClient.DeleteMessageAsync(result.Chat.Id, result.MessageId, cancellationToken);


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
                if (_botCommands.Any(x => text.Contains(x.Command.ToLower().Trim())))
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

                if (chat_message.GetCurrentMessageText().ToLower().Contains("/stat"))
                {
                    int count = await botClient.GetChatMemberCountAsync(message.Chat, cancellationToken);
                    ChatMember[] countAdmins = await botClient.GetChatAdministratorsAsync(message.Chat, cancellationToken);
                    await SendReactionByBotCommandWithTextAsync(botClient, message.Chat, message, user, "", $"В чате юзеров {count} ; администраторов {countAdmins.Length}  от ", cancellationToken);
                    return;
                }

                string ds = chat_message.GetCurrentMessageText();
                if (chat_message.GetCurrentMessageText().ToLower().Contains("/badword"))
                {
                    string[] splittedTextArr = chat_message.GetCurrentMessageText().ToLower().Split(new char[] { ' ', '_' });
                    await botClient.DeleteMessageAsync(message.Chat, int.Parse(splittedTextArr[1]), cancellationToken);
                    await botClient.DeleteMessageAsync(message.Chat, message.MessageId, cancellationToken);
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
                    await RunTaskTimerAsync(botClient, messageHello.Chat.Id, messageHello.MessageId, messageHello.Chat.Username, new TimeSpan(0, 0, 40), user, token);
                    return;
                }
            }
            if (message?.Type != null && message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
            {
                await Task.Factory.StartNew(async () =>
                {
                    var result = await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                   $"Спасибо за сообщение, дорогой(ая) {message.From.FirstName} {message.From.LastName} ",
                                                  parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, null, disableWebPagePreview: true, cancellationToken: cancellationToken);

                    await Task.Delay(2000, cancellationToken);
                    await botClient.DeleteMessageAsync(result.Chat.Id, result.MessageId, cancellationToken);
                }, cancellationToken).ConfigureAwait(false);
                return;
            }

            if (message?.Type != null && message.Type == Telegram.Bot.Types.Enums.MessageType.Photo)
            {
                await Task.Factory.StartNew(async () =>
                {
                    var result = await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                     $"Спасибо за фото, дорогой(ая) {message.From.FirstName} {message.From.LastName} ",
                                                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, null, disableWebPagePreview: true, cancellationToken: cancellationToken);

                    await Task.Delay(2000, cancellationToken);
                    await botClient.DeleteMessageAsync(result.Chat.Id, result.MessageId, cancellationToken);
                }, cancellationToken).ConfigureAwait(false);
                return;
            }

            if (message?.Type != null && message.Type == Telegram.Bot.Types.Enums.MessageType.Video)
            {
                await Task.Factory.StartNew(async () =>
                {
                    var result = await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                    $"Спасибо за видео, дорогой(ая) {message.From.FirstName} {message.From.LastName} ",
                                                   parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, null, disableWebPagePreview: true, cancellationToken: cancellationToken);

                    await Task.Delay(2000, cancellationToken);
                    await botClient.DeleteMessageAsync(result.Chat.Id, result.MessageId, cancellationToken);
                }, cancellationToken).ConfigureAwait(false);
                return;
            }
            if (message?.Type != null && message.Type == Telegram.Bot.Types.Enums.MessageType.Document)
            {
                await Task.Factory.StartNew(async () =>
                {
                    var result = await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                    $"Спасибо за документ, дорогой(ая) {message.From.FirstName} {message.From.LastName} ",
                                                   parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, null, disableWebPagePreview: true, cancellationToken: cancellationToken);

                    await Task.Delay(2000, cancellationToken);
                    await botClient.DeleteMessageAsync(result.Chat.Id, result.MessageId, cancellationToken);
                }, cancellationToken).ConfigureAwait(false);
                return;
            }

            if (message?.Type != null && message.Type == Telegram.Bot.Types.Enums.MessageType.Sticker)
            {
                await Task.Factory.StartNew(async () =>
                {
                    var result = await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                    $"Спасибо за стикер, дорогой(ая) {message.From.FirstName} {message.From.LastName} ",
                                                   parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, null, disableWebPagePreview: true, cancellationToken: cancellationToken);

                    await Task.Delay(2000, cancellationToken);
                    await botClient.DeleteMessageAsync(result.Chat.Id, result.MessageId, cancellationToken);
                }, cancellationToken).ConfigureAwait(false);
                return;
            }
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.EditedMessage)
            {
                await Task.Factory.StartNew(async () =>
                {
                    var result = await botClient.SendTextMessageAsync(chatId: update.EditedMessage.Chat.Id,
                                                    $"Спасибо за исправление сообщения, дорогой(ая) {update.EditedMessage.From.FirstName} {update.EditedMessage.From.LastName} ",
                                                   parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, null, disableWebPagePreview: true, cancellationToken: cancellationToken);

                    await Task.Delay(2000, cancellationToken);
                    await botClient.DeleteMessageAsync(result.Chat.Id, result.MessageId, cancellationToken);
                }, cancellationToken).ConfigureAwait(false);
                return;
            }

            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
            {
                long chatId = update.CallbackQuery.Message.Chat.Id;
                int messId = update.CallbackQuery.Message.MessageId;
                string codeOfButton = update.CallbackQuery.Data;
                string telegramMessage = codeOfButton;
                if (_fruitsArr.Contains(codeOfButton) && _callBackUser != null && _callBackUser.UserId == update.CallbackQuery.From.Id)
                {
                    _ = await Task.Factory.StartNew(async () =>
                    {
                        if (codeOfButton.Equals(_fruit))
                        {

                            await botClient.DeleteMessageAsync(chatId, messId, cancellationToken);
                            await SendHelloForNewMemberAsync(botClient, $"{update.CallbackQuery.From.FirstName} {update.CallbackQuery.From.LastName} ", update.CallbackQuery.Message, cancellationToken);

                            return;
                        }
                        else
                        {
                            await botClient.DeleteMessageAsync(chatId, messId, cancellationToken);
                            Message result = await botClient.SendTextMessageAsync(chatId: update.CallbackQuery.Message.Chat.Id,
                                $"Вы не прошли проверку на антиспам, вы будете лишены возможности комментировать в группе в течение одного часа." +
                                $" У вас есть возможность за это время ознакомиться с правилами группы по этой ссылке \n https://t.me/winnersDV2022flood/3 ",
                               parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, null, disableWebPagePreview: true, cancellationToken: cancellationToken);
                            await Task.Delay(10000, cancellationToken);
                            await botClient.DeleteMessageAsync(result.Chat.Id, result.MessageId, cancellationToken);
                            try
                            {
                                await botClient.RestrictChatMemberAsync(chatId, userId: _callBackUser.UserId,
                                    new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false }, untilDate: DateTime.Now.AddMinutes(1), cancellationToken);
                                _callBackUser = null;
                                return;
                            }
                            catch
                            {
                                _logger.LogError("Something with RO went wrong");
                            }
                            //await botClient.BanChatMemberAsync(chatId, userId: user.GetUserId(), untilDate: DateTime.Now.AddSeconds(40), revokeMessages: false, cancellationToken);
                        }

                        _callBackUser = null;
                    }, token);
                    _ctsHello.Cancel();


                    return;
                }
                else if (!_fruitsArr.Contains(codeOfButton))
                {
                    codeOfButton = codeOfButton.Substring(1);
                    Task ds = botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, $@"https://t.me/{codeOfButton}", true, null, 5, cancellationToken);
                    //await botClient.SendTextMessageAsync(chatId: update.CallbackQuery.Message.Chat.Id, telegramMessage, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                    await botClient.DeleteMessageAsync(chatId, messId, cancellationToken);
                }
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
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
