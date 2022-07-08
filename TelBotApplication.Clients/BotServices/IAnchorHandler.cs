using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelBotApplication.Clients.BotServices
{
    public interface IAnchorHandler
    {
        Task UpdateAchors();
        Task ExecuteAncor(ITelegramBotClient botClient, Message message, string text, CancellationToken cancellationToken);
    }
}