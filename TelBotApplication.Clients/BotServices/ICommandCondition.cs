using System.Collections.Generic;
using System.Threading.Tasks;
using TelBotApplication.Domain.Models;

namespace TelBotApplication.Clients.BotServices
{
    public interface ICommandCondition
    {
        Task<IEnumerable<BotCaller>> GetAllBotCommands();
        Task<IEnumerable<VenueCommand>> GetAllLocations();
    }
}