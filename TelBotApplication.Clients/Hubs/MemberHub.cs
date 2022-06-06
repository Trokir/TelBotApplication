using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelBotApplication.Clients.Hubs
{
    public class MemberHub : Hub<INewMember>
    {
        public async Task SendHelloFromNewMember(string message)
        {
            await Clients.All.SayHello(message);
        }
    }
}
