using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using TelBotApplication.Domain.Models;

namespace TelBotApplication.DAL.Services
{
    public class BotCommandService : IBotCommandService
    {
        private readonly TelBotApplicationDbContext _dbContext;
        private readonly ILogger<BotCommandService> _logger;
        public BotCommandService(ILogger<BotCommandService> logger, TelBotApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<BotCaller>> GetAllCommandsAsync()
        {
            return await _dbContext.BotCallers.AsNoTracking().ToListAsync();
        }

        public async Task AddNewCommandAsync(BotCaller caller)
        {
            _ = await _dbContext.BotCallers.AddAsync(caller);
            _ = await _dbContext.SaveChangesAsync();
        }
        public async Task DeleteCommandByCommandAsync(string command)
        {
            BotCaller entity = await _dbContext.BotCallers.SingleAsync(x => x.Command.Equals(command));
            if (entity != null)
            {
                _ = _dbContext.BotCallers.Remove(entity);
                _ = await _dbContext.SaveChangesAsync();
            }

        }
        public async Task DeleteCommandByIdAsync(int id)
        {
            BotCaller entity = await _dbContext.BotCallers.SingleAsync(x => x.Id == id);
            if (entity != null)
            {
                _ = _dbContext.BotCallers.Remove(entity);
                _ = await _dbContext.SaveChangesAsync();
            }

        }

        public async Task<BotCaller> UpdateEntityAsync(BotCaller entity)
        {
            _ = _dbContext.BotCallers.Update(entity);
            _ = await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<IEnumerable<BotCaller>> UpdateEntitiesListAsync(IEnumerable<BotCaller> commandsList)
        {
            _dbContext.BotCallers.UpdateRange(commandsList);
            _ = await _dbContext.SaveChangesAsync();
            return commandsList;
        }
    }
}
