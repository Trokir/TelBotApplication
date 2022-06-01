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
        private readonly IMapper _mapper;
        private readonly IBotCommandService _commandService;
        private readonly IFludFilter _fludFilter;
        private ChatMember[] _admins;
        private ITelegramBotClient _bot { get; set; }

        readonly string[] _fruitsArr = new string[] { "🍎", "🍌", "🍒", "🍍", "🍋", "🍉" };
        private CallBackUser _callBackUser;
        private string _fruit;
        private readonly Random _rnd;

        private CancellationTokenSource cts;
        private CancellationTokenSource _ctsHello;

        public BotClientService(IOptions<EnvironmentBotConfiguration> options,
            ILogger<BotClientService> logger,
            IMapper mapper,
            IBotCommandService commandService,
            IFludFilter fludFilter
            )
        {
            _config = options.Value;
            _bot = new TelegramBotClient(_config.Token);/*flud*/
            _logger = logger;
            _mapper = mapper;
            _commandService = commandService;
            _rnd = new Random();
            _fludFilter = fludFilter;
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

            var messag = await botClient.SendTextMessageAsync(chatId: message.Chat.Id, $"ВАЖНО: {user.GetFullName()}  @{user.UserName}, если ты " +
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
                    //   await RunTaskTimerAsync(botClient, messageHello.Chat.Id, messageHello.MessageId, messageHello.Chat.Username, new TimeSpan(0, 0, 40), user, token);
                    return;
                }
            }
            if (message?.Type != null && message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
            {
                ////////////
                if ((text.Trim().Length < 2))
                {
                    await Task.Factory.StartNew(async () =>
                    {
                        Message result = await botClient.SendTextMessageAsync(message.Chat, $"Нарушение п. 13 правил." +
                       $" Вопросы основного чата обсуждаем только там.В следующий раз будет РО.!  https://t.me/winnersDV2022flood/3 ", disableWebPagePreview: true, cancellationToken: cancellationToken);
                        await botClient.DeleteMessageAsync(chatId: message.Chat, message.MessageId, cancellationToken);
                        await Task.Delay(10000);
                        await botClient.DeleteMessageAsync(chatId: result.Chat, result.MessageId, cancellationToken);
                    });

                    return;
                }
                else
                {
                    var trimmedText = text.Remove(text.Length - 1, 1);
                    var res = long.TryParse(trimmedText.Trim(), out var temp);
                    if (res && message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
                    {
                        await Task.Factory.StartNew(async () =>
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

                    await Task.Factory.StartNew(async () =>
                    {

                        var spam = _fludFilter.CheckIsSpam(update.Message.Text.Trim());
                        var result = await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                                                       $"Спасибо за сообщение, дорогой(ая) {message.From.FirstName} {message.From.LastName}  is spam = {spam}",
                                                      parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, null, disableWebPagePreview: true, cancellationToken: cancellationToken);

                        await Task.Delay(2000, cancellationToken);
                        await botClient.DeleteMessageAsync(result.Chat.Id, result.MessageId, cancellationToken);
                    }, cancellationToken).ConfigureAwait(false);
                }
                //////////


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
            await botClient.AnswerCallbackQueryAsync(callbackQueryId: update.CallbackQuery.Id, text: $"{update.CallbackQuery.From.FirstName} {update.CallbackQuery.From.LastName}, " +
                $"Вы не прошли проверку на антиспам, вы будете лишены возможности комментировать в группе в течение одного часа."
                , showAlert: true, url: null, cacheTime: 15, cancellationToken: cancellationToken);
            await botClient.DeleteMessageAsync(chatId, messId, cancellationToken);
            await Task.Delay(2000);
            await botClient.RestrictChatMemberAsync(chatId, userId: _callBackUser.UserId,
               new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false }, untilDate: DateTime.Now.AddSeconds(31), cancellationToken);
        }

        private static async Task SuccessfullNewMemberAddedAsync(ITelegramBotClient botClient, Update update, long chatId, int messId, CancellationToken cancellationToken)
        {
            await botClient.AnswerCallbackQueryAsync(callbackQueryId: update.CallbackQuery.Id, text: $"{update.CallbackQuery.From.FirstName} {update.CallbackQuery.From.LastName}, приветствуем вас в ламповой и дружной флудилке! Здесь любят фудпорн, "
                + $" троллить Джобса, сербскую еду, подгонять Данзана, а также йожек. Правила чата необходимо неукоснительно соблюдать!"
                  , showAlert: true, url: null, cacheTime: 15, cancellationToken: cancellationToken);
            await botClient.DeleteMessageAsync(chatId, messId, cancellationToken);
        }

        private async Task RunTaskTimerAsync(ITelegramBotClient botClient, long chatId, int messageId, string userName, TimeSpan interval, ChatUser user, CancellationToken cancellationToken)
        {
            _ = await Task.Factory.StartNew(async () =>
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(interval, cancellationToken);
                    Message result1 = await botClient.SendTextMessageAsync(chatId: chatId, $"@{user.UserName}, пожалуйста выполни проверку на антиспам https://t.me/{userName ?? " "}/{messageId}", disableWebPagePreview: true, cancellationToken: cancellationToken);
                    await Task.Delay(5000, cancellationToken);
                    await botClient.DeleteMessageAsync(result1.Chat.Id, result1.MessageId, cancellationToken);
                    await Task.Delay(interval, cancellationToken);
                    await botClient.DeleteMessageAsync(chatId, messageId, cancellationToken);
                    await botClient.RestrictChatMemberAsync(chatId, userId: _callBackUser.UserId, new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false }, untilDate: DateTime.Now.AddMinutes(1), cancellationToken);
                    Message result = await botClient.SendTextMessageAsync(chatId: chatId, $"ВАЖНО: Ты не нажал(а) кнопку, значит ты БОТ или СПАМЕР, БАН на 100 лет", cancellationToken: cancellationToken);
                    await Task.Delay(5000, cancellationToken);
                    await botClient.DeleteMessageAsync(result.Chat.Id, result.MessageId, cancellationToken);
                }
            }, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }

        #endregion income

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
