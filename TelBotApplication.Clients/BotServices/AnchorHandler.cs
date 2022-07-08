using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TelBotApplication.DAL.Interfaces;
using TelBotApplication.Domain.Dtos;
using TelBotApplication.Domain.Models.Anchors;
using TelBotApplication.Domain.NewFolder.Executors.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelBotApplication.Clients.BotServices
{
    public class AnchorHandler : IAnchorHandler
    {
        private readonly ILogger<AnchorHandler> _logger;
        private readonly IServiceProvider _factory;
        private HashSet<AnchorDTO> _anchors = default;
        private readonly IMapper _mapper;
        public AnchorHandler(IServiceProvider factory, ILogger<AnchorHandler> logger, IMapper mapper)
        {
            _factory = factory;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task UpdateAchors()
        {
            _logger.LogDebug("Start UpdateAchors");
            using IServiceScope scope = _factory.CreateScope();
            while (true)
            {

                var context =
                    scope.ServiceProvider
                        .GetRequiredService<IUnitOfWork>();

                var result = (await context.AnchorService.GetAllAsync()).ToHashSet();
                _anchors = _mapper.Map<HashSet<AnchorDTO>>(result);

                await Task.Delay(20000);
            }
        }

        public async Task ExecuteAncor(ITelegramBotClient botClient, Message message, string text, CancellationToken cancellationToken)
        {
            if (_anchors is HashSet<AnchorDTO> keys)
            {
                if (keys.Any(x => x.Tag.Equals(text.Trim(), StringComparison.InvariantCultureIgnoreCase)))
                {
                    var anchor = keys.SingleOrDefault(x => x.Tag.Equals(text.Trim(), StringComparison.InvariantCultureIgnoreCase));
                    switch (anchor.AnchorCallBackType)
                    {
                        case Domain.Enums.AnchorCallBack.Link:
                            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(
                            new[] { new[] { InlineKeyboardButton.WithUrl(anchor.ButtonText, anchor.ButtonCondition) } });
                            await botClient.SendTextMessageWhithDelayAsync(isEnabled: true, message, message.Chat, anchor.Message,
                                new TimeSpan(0, anchor.UntilMinutes, 0), ParseMode.Html, replyMarkup: keyboard, cancellationToken: cancellationToken);
                            break;


                        case Domain.Enums.AnchorCallBack.Reaction:
                            keyboard = new InlineKeyboardMarkup(
            new[]{new[]{
                         InlineKeyboardButton.WithCallbackData(text: "👍", callbackData: "👍"),
                         InlineKeyboardButton.WithCallbackData(text: "👎", callbackData: "👎"),
                        }
                       });
                          
                            await botClient.SendTextMessageWhithDelayAsync(isEnabled: true, message, message.Chat, anchor.Message,
                                                           new TimeSpan(0, anchor.UntilMinutes, 0), ParseMode.Html, replyMarkup: keyboard, cancellationToken: cancellationToken);

                            break;
                        case Domain.Enums.AnchorCallBack.Share:
                            break;
                        case Domain.Enums.AnchorCallBack.CallTrigger:
                            break;
                        case Domain.Enums.AnchorCallBack.None:
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
