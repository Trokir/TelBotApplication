﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using TelBotApplication.DAL.Interfaces;
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
    }
}
