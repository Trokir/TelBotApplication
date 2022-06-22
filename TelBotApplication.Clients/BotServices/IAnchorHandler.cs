using System.Threading.Tasks;

namespace TelBotApplication.Clients.BotServices
{
    public interface IAnchorHandler
    {
        Task UpdateAchors();
        Task ExecuteAncor(string text);
    }
}