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
            await _dbContext.BotCallers.AddAsync(caller);
            await _dbContext.SaveChangesAsync();
        }
        public async Task DeleteCommandByCommandAsync(string command)
        {
            var entity = await _dbContext.BotCallers.SingleAsync(x => x.Command.Equals(command));
            if (entity != null)
            {
                _dbContext.BotCallers.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }

        }
        public async Task DeleteCommandByIdAsync(int id)
        {
            var entity = await _dbContext.BotCallers.SingleAsync(x => x.Id ==id);
            if (entity != null)
            {
                _dbContext.BotCallers.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }

        }

        public async Task<BotCaller> UpdateEntityAsync(BotCaller entity)
        {
            _dbContext.BotCallers.Update(entity);
                await _dbContext.SaveChangesAsync();
                return entity;
        }

        public async Task<IEnumerable<BotCaller>> UpdateEntitiesListAsync(IEnumerable<BotCaller> commandsList)
        {
            _dbContext.BotCallers.UpdateRange(commandsList);
            await _dbContext.SaveChangesAsync();
            return commandsList;
        }
    }
}
