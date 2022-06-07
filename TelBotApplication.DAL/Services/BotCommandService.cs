using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TelBotApplication.DAL.Interfaces;
using TelBotApplication.Domain.Models;

namespace TelBotApplication.DAL.Services
{
    public class BotCommandService : IBotCommandService
    {
        private readonly TelBotApplicationDbContext _dbContext;
        private readonly ILogger<BotCommandService> _logger;
        public BotCommandService(ILogger<BotCommandService> logger,
            TelBotApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task AddAsync(BotCaller entity)
        {
            _ = await _dbContext.BotCallers.AddAsync(entity).ConfigureAwait(false);
            _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DeleteAsync(BotCaller entity)
        {
            _ = _dbContext.BotCallers.Remove(entity);
            _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        public async Task<BotCaller> GetByIdAsync(int id)
        {
                BotCaller entity = await _dbContext.BotCallers.FindAsync(id).ConfigureAwait(false);
            return entity;
        }

        public async Task<IEnumerable<BotCaller>> GetAllAsync()
        {
            return await _dbContext.BotCallers.ToListAsync().ConfigureAwait(false);
        }
        public async Task UpdateAsync(BotCaller entity)
        {
            _ = _dbContext.BotCallers.Update(entity);
                    _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task UpdateListAsync(IEnumerable<BotCaller> entities)
        {
            _dbContext.BotCallers.UpdateRange(entities);
                    _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<IEnumerable<BotCaller>> GetAllAsync(Expression<Func<BotCaller, bool>> predicate)
        {
            return await _dbContext.BotCallers.Where(predicate).ToListAsync();
        }
        public async Task<BotCaller> FindIdAsync(Expression<Func<BotCaller, bool>> predicate)
        {
            return await _dbContext.BotCallers.Where(predicate).FirstOrDefaultAsync(); 
        }
        public async Task DeleteRangeAsync(Expression<Func<BotCaller, bool>> predicate)
        {
        var list = await _dbContext.BotCallers.Where(predicate).ToListAsync();
            _dbContext.BotCallers.RemoveRange(list);
            await _dbContext.SaveChangesAsync();
        }
    }
}
