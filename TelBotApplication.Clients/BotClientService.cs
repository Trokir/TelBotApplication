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
        string[] _fruitsArr = new string[] { "🍎", "🍌", "🍒", "🍍" };
        private CallBackUser _callBackUser;
        private string _fruit;
        Random _rnd;

        private CancellationTokenSource cts;

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
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new Telegram.Bot.Types.Enums.UpdateType[]
                {

                },
                ThrowPendingUpdates = true
            };
            var updateReceiver = new QueuedUpdateReceiver(_bot, receiverOptions);
            await _bot.DeleteMyCommandsAsync(BotCommandScope.AllGroupChats(), languageCode: "en", cancellationToken: cancellationToken);
            try
            {
                var list = await _commandService.GetAllCommandsAsync();
                _botCommands = _mapper.Map<IEnumerable<BotCommandDto>>(list);
                if (_botCommands.Any())
                {
                    var commandsList = new List<BotCommand>();
                    foreach (var item in _botCommands)
                    {
                        commandsList.Add(new BotCommand
                        {
                            Command = item.Command,
                            Description = item.Description
                        });
                    }
                    await _bot.SetMyCommandsAsync(commandsList, BotCommandScope.AllGroupChats(), languageCode: "en", cancellationToken);
                }


                await foreach (Update update in updateReceiver.WithCancellation(cts.Token))
                {
                    if (update is Update message)
                    {
                        await HandleUpdateAsync(_bot, message, cancellationToken);

                    }
                }
            }
            catch (OperationCanceledException exception)
            {
                await HandleErrorAsync(_bot, exception, cancellationToken);
            }
        }


        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var chat_message = new ChatMessage(update);
            var user = chat_message.GetCurrentUser();
            var message = chat_message.GetCurrentMessage();
            var userId = user.GetUserId();
            var text = chat_message.GetCurrentMessageText();
            // Некоторые действия
            _logger.LogDebug(Newtonsoft.Json.JsonConvert.SerializeObject(update));

            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                if (_botCommands.Any(x => text.Contains(x.Command.ToLower().Trim())))
                {
                    await Task.Factory.StartNew(async () =>
                    {
                        var command = _botCommands.FirstOrDefault(x => text.Contains(x.Command.ToLower().Trim()));
                        switch (command.TypeOfreaction)
                        {
                            case TypeOfreactions.Text:
                                await SendReactionByBotCommandWithTextAsync(botClient, message.Chat, message, user, "", command.Caption, cancellationToken).ConfigureAwait(false);
                                return;
                            case TypeOfreactions.Animation:
                                await SendReactionByBotCommandWithAnimationAsync(botClient, message.Chat, message, command.Link, command.Caption, cancellationToken).ConfigureAwait(false);
                                return;
                        }
                    }, cancellationToken);
                }

                if (chat_message.GetCurrentMessageText().ToLower().Contains("/stat"))
                {
                    var count = await botClient.GetChatMemberCountAsync(message.Chat, cancellationToken);
                    var countAdmins = await botClient.GetChatAdministratorsAsync(message.Chat, cancellationToken);
                    await SendReactionByBotCommandWithTextAsync(botClient, message.Chat, message, user, "", $"В чате юзеров {count} ; администраторов {countAdmins.Length}  от ", cancellationToken);
                    return;
                }

                var ds = chat_message.GetCurrentMessageText();
                if (chat_message.GetCurrentMessageText().ToLower().Contains("/badword"))
                {
                    var splittedTextArr = chat_message.GetCurrentMessageText().ToLower().Split(new char[] { ' ', '_' });
                    await botClient.DeleteMessageAsync(message.Chat, int.Parse(splittedTextArr[1]), cancellationToken);
                    await botClient.DeleteMessageAsync(message.Chat, message.MessageId, cancellationToken);
                }

                if (chat_message.GetCurrentMessageText().ToLower().Contains("/admin"))
                {
                    var exList = new long[] { 5316258402, 416112286 };
                    if (_admins == null)
                    {
                        _admins = await botClient.GetChatAdministratorsAsync(message.Chat, cancellationToken);
                    }
                    var selectedAdmins = _admins.Where(u => !exList.Contains(u.User.Id)).ToArray();
                    SendInlineAdmins(botClient: botClient, selectedAdmins, message.Chat, message, chatId: message.Chat.Id, cancellationToken: cancellationToken);
                    return;
                }
                if (message?.Type != null && message.Type == Telegram.Bot.Types.Enums.MessageType.ChatMembersAdded)
                {
                    var index = _rnd.Next(_fruitsArr.Length);
                    _fruit = _fruitsArr[index];
                    _callBackUser = new CallBackUser { UserId = user.GetUserId() };
                    var messageHello = await SendInAntiSpamline(botClient: botClient, message: message, user, _fruit, cancellationToken: cancellationToken);
                    await RunTaskTimerAsync(botClient, messageHello.Chat.Id, messageHello.MessageId, messageHello.Chat.Username, new TimeSpan(0, 0, 15), user, cancellationToken);
                    return;
                }
            }

            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
            {
                var chatId = update.CallbackQuery.Message.Chat.Id;
                var messId = update.CallbackQuery.Message.MessageId;
                string codeOfButton = update.CallbackQuery.Data;
                string telegramMessage = codeOfButton;
                if (_fruitsArr.Contains(codeOfButton) && _callBackUser != null && _callBackUser.UserId == update.CallbackQuery.From.Id)
                {
                    await Task.Factory.StartNew(async () =>
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
                            var result = await botClient.SendTextMessageAsync(chatId: update.CallbackQuery.Message.Chat.Id,
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
                    }, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();


                    return;
                }
                else if (!_fruitsArr.Contains(codeOfButton))
                {
                    codeOfButton = codeOfButton.Substring(1);
                    var ds = botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, $@"https://t.me/{codeOfButton}", true, null, 5, cancellationToken);
                    //await botClient.SendTextMessageAsync(chatId: update.CallbackQuery.Message.Chat.Id, telegramMessage, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                    await botClient.DeleteMessageAsync(chatId, messId, cancellationToken);
                }
            }
        }

        private async Task RunTaskTimerAsync(ITelegramBotClient botClient, long chatId, int messageId, string userName, TimeSpan interval, ChatUser user, CancellationToken cancellationToken)
        {
            await Task.Factory.StartNew(async () =>
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(interval, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();
                    var result1 = await botClient.SendTextMessageAsync(chatId: chatId, $"@{user.UserName}, пожалуйста выполни проверку на антиспам https://t.me/{userName ?? " "}/{messageId}", cancellationToken: cancellationToken);
                    await Task.Delay(5000, cancellationToken);
                    await botClient.DeleteMessageAsync(result1.Chat.Id, result1.MessageId, cancellationToken);
                    await Task.Delay(interval, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();
                    await botClient.DeleteMessageAsync(chatId, messageId, cancellationToken);
                    await botClient.RestrictChatMemberAsync(chatId, userId: _callBackUser.UserId, new ChatPermissions { CanSendMessages = false, CanSendMediaMessages = false }, untilDate: DateTime.Now.AddMinutes(1), cancellationToken);
                    var result = await botClient.SendTextMessageAsync(chatId: chatId, $"ВАЖНО: Ты не нажал(а) кнопку, значит ты БОТ или СПАМЕР, БАН на 100 лет", cancellationToken: cancellationToken);
                    await Task.Delay(5000, cancellationToken);
                    await botClient.DeleteMessageAsync(result.Chat.Id, result.MessageId, cancellationToken);
                }
            }, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }

        private async Task SendReactionByBotCommandWithAnimationAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Chat chat, Message message, string link = "", string caption = "", CancellationToken cancellationToken = default)
        {
            var result = await botClient.SendAnimationAsync(chatId: chat, animation: link, caption: caption, cancellationToken: cancellationToken);
            await botClient.DeleteMessageAsync(chatId: chat, message.MessageId, cancellationToken);
            await Task.Delay(30000);
            await botClient.DeleteMessageAsync(chatId: result.Chat, result.MessageId, cancellationToken);
        }
        private async Task SendReactionByBotCommandWithTextAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Chat chat, Message message, ChatUser user, string link = "", string caption = "", CancellationToken cancellationToken = default)
        {
            var result = await botClient.SendTextMessageAsync(message.Chat, $" {caption} {user.GetFullName()} !", cancellationToken: cancellationToken);
            await botClient.DeleteMessageAsync(chatId: chat, message.MessageId, cancellationToken);
            await Task.Delay(30000);
            await botClient.DeleteMessageAsync(chatId: result.Chat, result.MessageId, cancellationToken);
        }


        private async Task SendHelloForNewMemberAsync(ITelegramBotClient botClient, string userName, Message message, CancellationToken cancellationToken)
        {
            var result = await botClient.SendTextMessageAsync(message.Chat, $"{userName}, приветствуем вас в ламповой и дружной флудилке! \n Здесь любят фудпорн," +
                                   $" троллить Джобса, сербскую еду, подгонять Данзана, а также йожек.\n Но есть тут и правила:\n https://t.me/winnersDV2022flood/3 \n Их необходимо неукоснительно соблюдать!", disableWebPagePreview: true);

            await Task.Delay(10000, cancellationToken);

            await botClient.DeleteMessageAsync(result.Chat.Id, result.MessageId, cancellationToken);


        }



        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is ApiRequestException apiRequestException)
            {
                _logger.LogError(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
            }
            // Некоторые действия
            return Task.CompletedTask;
        }

        public async void SendInlineAdmins(ITelegramBotClient botClient, ChatMember[] members, Telegram.Bot.Types.Chat chat, Message message, long chatId, CancellationToken cancellationToken)
        {
            var btnArr = GetButtons(members);
            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(btnArr);
            // keyboard
            await botClient.DeleteMessageAsync(chatId: chat, message.MessageId, cancellationToken);
            Message result = await botClient.SendTextMessageAsync(
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
        public async Task<Message> SendInAntiSpamline(ITelegramBotClient botClient, Message message, ChatUser user, string fruit, CancellationToken cancellationToken)
        {

            var keyboard = new InlineKeyboardMarkup(
                // keyboard
                new[]
                {
                    // first row
                    new[]
                    {
                       InlineKeyboardButton.WithCallbackData(text: "🍎", callbackData: "🍎"),
                        InlineKeyboardButton.WithCallbackData(text: "🍌", callbackData: "🍌"),
                        InlineKeyboardButton.WithCallbackData(text: "🍒", callbackData: "🍒"),
                        InlineKeyboardButton.WithCallbackData(text: "🍍", callbackData: "🍍"),
                    }
                });
            var messag = await botClient.SendTextMessageAsync(chatId: message.Chat.Id, $"ВАЖНО: {user.GetFullName()}  @{user.UserName}, если ты не БОТ и не СПАМЕР, пройди проверку, нажав на кнопку, где есть {fruit}", parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: keyboard,
               cancellationToken: cancellationToken);

            return messag;
        }

        public async void SendInline(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {




            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(
                // keyboard
                new[]
                {
                    // first row
                    new[]
                    {
                        // first button in row
                        InlineKeyboardButton.WithCallbackData(text: "Кнопка 1", callbackData: "post"),
                        // second button in row
                        InlineKeyboardButton.WithCallbackData(text: "Кнопка 2", callbackData: "12"),
                    },
                    // second row
                    new[]
                    {
                        // first button in row
                        InlineKeyboardButton.WithUrl(text: "Ссылка", url: "https://google.com"),
                        InlineKeyboardButton.WithCallbackData("CallbackData кнопка")
                    },

                });

            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "за что мне это??",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }




    }
}
