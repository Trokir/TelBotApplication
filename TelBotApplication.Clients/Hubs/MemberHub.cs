using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace TelBotApplication.Clients.Hubs
{
    public class MemberHub : Hub<INewMember>
    {
        public async Task SendLog(string message)
        {
            await Clients.All.SendLog(message);
        }
        public async Task EditLog(string message)
        {
            await Clients.All.EditLog(message);
        }
    }
   
}
