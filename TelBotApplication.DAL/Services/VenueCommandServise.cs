using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using TelBotApplication.DAL.Interfaces;
using TelBotApplication.Domain.Models;

namespace TelBotApplication.DAL.Services
{
    public class VenueCommandServise : IVenueCommandServise
    {
        private readonly TelBotApplicationDbContext _dbContext;
        private readonly ILogger<VenueCommandServise> _logger;
        public VenueCommandServise(ILogger<VenueCommandServise> logger, TelBotApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }
        public async Task AddAsync(VenueCommand entity)
        {
            _ = await _dbContext.VenueCommands.AddAsync(entity);
            _ = await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(VenueCommand entity)
        {
            _ = _dbContext.VenueCommands.Remove(entity);
            _ = await _dbContext.SaveChangesAsync();
        }

        public async Task<VenueCommand> GetByIdAsync(int id)
        {
            VenueCommand entity = await _dbContext.VenueCommands.FindAsync(id);
            return entity;
        }

        public async Task<IEnumerable<VenueCommand>> GetAllAsync()
        {
            return await _dbContext.VenueCommands.ToListAsync();
        }

        public async Task UpdateAsync(VenueCommand entity)
        {
            _ = _dbContext.VenueCommands.Update(entity);
            _ = await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateListAsync(IEnumerable<VenueCommand> entities)
        {
            _dbContext.VenueCommands.UpdateRange(entities);
            _ = await _dbContext.SaveChangesAsync();
        }
    }
}
