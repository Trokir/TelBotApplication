using System.Collections.Generic;
using System.Threading.Tasks;
using TelBotApplication.Domain.Models;

namespace TelBotApplication.DAL.Services
{
    public interface IBotCommandService
    {
        Task AddNewCommandAsync(BotCaller caller);
        Task DeleteCommandByCommandAsync(string command);
        Task DeleteCommandByIdAsync(int id);
        Task<IEnumerable<BotCaller>> GetAllCommandsAsync();
        Task<BotCaller> UpdateEntityAsync(BotCaller entity);
        Task<IEnumerable<BotCaller>> UpdateEntitiesListAsync(IEnumerable<BotCaller> commandsList);

    }
}