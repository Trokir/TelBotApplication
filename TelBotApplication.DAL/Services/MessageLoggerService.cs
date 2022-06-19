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
    public class MessageLoggerService : IMessageLoggerService
    {
        private readonly TelBotApplicationDbContext _dbContext;
        private readonly ILogger<MessageLogger> _logger;
        public MessageLoggerService(ILogger<MessageLogger> logger, TelBotApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }
        public async Task AddAsync(MessageLogger entity)
        {
            _ = await _dbContext.MessageLoggers.AddAsync(entity).ConfigureAwait(false);
            _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DeleteAsync(MessageLogger entity)
        {
            _ = _dbContext.MessageLoggers.Remove(entity);
            _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<MessageLogger> GetByIdAsync(int id)
        {
            MessageLogger entity = await _dbContext.MessageLoggers.FindAsync(id).ConfigureAwait(false);
            return entity;
        }

        public async Task<IEnumerable<MessageLogger>> GetAllAsync()
        {
            return await _dbContext.MessageLoggers.ToListAsync().ConfigureAwait(false);
        }

        public async Task UpdateAsync(MessageLogger entity)
        {
            _ = _dbContext.MessageLoggers.Update(entity);
            _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task UpdateListAsync(IEnumerable<MessageLogger> entities)
        {
            _dbContext.MessageLoggers.UpdateRange(entities);
            _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<IEnumerable<MessageLogger>> GetAllAsync(Expression<Func<MessageLogger, bool>> predicate)
        {
            return await _dbContext.MessageLoggers.Where(predicate).ToListAsync().ConfigureAwait(false);
        }
        public async Task<MessageLogger> FindAsync(Expression<Func<MessageLogger, bool>> predicate)
        {
            return await _dbContext.MessageLoggers.Where(predicate).FirstOrDefaultAsync().ConfigureAwait(false);
        }
        public async Task DeleteRangeAsync(Expression<Func<MessageLogger, bool>> predicate)
        {
            var list = await _dbContext.MessageLoggers.Where(predicate).ToListAsync().ConfigureAwait(false);
            _dbContext.MessageLoggers.RemoveRange(list);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }


    }
}
