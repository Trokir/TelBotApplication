using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelBotApplication.Clients.Hubs;

namespace TelBotApplication.Clients
{
    public static class Strings
    {
        public static string HubUrl => "https://localhost:5001/hubs/member";

        public static class Events
        {
            public static string MessageSent => nameof(INewMember.SayHello);
        }
    }
}
