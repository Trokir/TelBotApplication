using System.Threading.Tasks;

namespace TelBotApplication.Clients.BotServices
{
    public interface IFilter
    {
        Task UpdateFilters();
        string FindAnswerForAlertFrase(string text);
    }
}