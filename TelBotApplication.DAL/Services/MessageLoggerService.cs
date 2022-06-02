using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using TelBotApplication.DAL.Interfaces;
using TelBotApplication.Domain.Models;

namespace TelBotApplication.DAL.Services
{
    public class MessageLoggerService: IMessageLoggerService
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
            _ = await _dbContext.MessageLoggers.AddAsync(entity);
            _ = await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(MessageLogger entity)
        {
            _ = _dbContext.MessageLoggers.Remove(entity);
            _ = await _dbContext.SaveChangesAsync();
        }

        public async Task<MessageLogger> GetByIdAsync(int id)
        {
            MessageLogger entity = await _dbContext.MessageLoggers.FindAsync(id);
            return entity;
        }

        public async Task<IEnumerable<MessageLogger>> GetAllAsync()
        {
            return await _dbContext.MessageLoggers.ToListAsync();
        }

        public async Task UpdateAsync(MessageLogger entity)
        {
            _ = _dbContext.MessageLoggers.Update(entity);
            _ = await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateListAsync(IEnumerable<MessageLogger> entities)
        {
            _dbContext.MessageLoggers.UpdateRange(entities);
            _ = await _dbContext.SaveChangesAsync();
        }
    }
}
