using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelBotApplication.Clients.Hubs
{
    public interface INewMember
    {
        Task SayHello(string message);
    }
}
