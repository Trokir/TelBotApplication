using System.Threading.Tasks;

namespace TelBotApplication.Clients.Hubs
{
    public interface INewMember
    {
        Task SendLog(string message);
        Task EditLog(string message);
    }
    
}
