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
    public class VenueCommandService : IVenueCommandService
    {
        private readonly TelBotApplicationDbContext _dbContext;
        private readonly ILogger<VenueCommandService> _logger;
        public VenueCommandService(ILogger<VenueCommandService> logger, TelBotApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }
        public async Task AddAsync(VenueCommand entity)
        {
            _ = await _dbContext.VenueCommands.AddAsync(entity).ConfigureAwait(false);
            _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DeleteAsync(VenueCommand entity)
        {
            _ = _dbContext.VenueCommands.Remove(entity);
            _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<VenueCommand> GetByIdAsync(int id)
        {
            VenueCommand entity = await _dbContext.VenueCommands.FindAsync(id).ConfigureAwait(false);
            return entity;
        }

        public async Task<IEnumerable<VenueCommand>> GetAllAsync()
        {
            return await _dbContext.VenueCommands.ToListAsync().ConfigureAwait(false);
        }

        public async Task UpdateAsync(VenueCommand entity)
        {
            _ = _dbContext.VenueCommands.Update(entity);
            _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task UpdateListAsync(IEnumerable<VenueCommand> entities)
        {
            _dbContext.VenueCommands.UpdateRange(entities);
            _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<IEnumerable<VenueCommand>> GetAllAsync(Expression<Func<VenueCommand, bool>> predicate)
        {
            return await _dbContext.VenueCommands.Where(predicate).ToListAsync().ConfigureAwait(false);
        }
        public async Task<VenueCommand> FindAsync(Expression<Func<VenueCommand, bool>> predicate)
        {
            return await _dbContext.VenueCommands.Where(predicate).FirstOrDefaultAsync().ConfigureAwait(false);
        }
        public async Task DeleteRangeAsync(Expression<Func<VenueCommand, bool>> predicate)
        {
            var list = await _dbContext.VenueCommands.Where(predicate).ToListAsync().ConfigureAwait(false);
            _dbContext.VenueCommands.RemoveRange(list);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
