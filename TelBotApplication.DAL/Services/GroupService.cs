
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using TelBotApplication.DAL.Interfaces;
using TelBotApplication.Domain.Models;

namespace TelBotApplication.DAL.Services
{
    public class GroupService : IGroupService
    {
        private readonly TelBotApplicationDbContext _dbContext;
        private readonly ILogger<Group> _logger;
        public GroupService(ILogger<Group> logger, TelBotApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }
        public async Task AddAsync(Group entity)
        {
            _ = await _dbContext.Groups.AddAsync(entity);
            _ = await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Group entity)
        {
            _ = _dbContext.Groups.Remove(entity);
            _ = await _dbContext.SaveChangesAsync();
        }

        public async Task<Group> GetByIdAsync(int id)
        {
            Group entity = await _dbContext.Groups.FindAsync(id);
            return entity;
        }

        public async Task<IEnumerable<Group>> GetAllAsync()
        {
            return await _dbContext.Groups.ToListAsync();
        }

        public async Task UpdateAsync(Group entity)
        {
            _ = _dbContext.Groups.Update(entity);
            _ = await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateListAsync(IEnumerable<Group> entities)
        {
            _dbContext.Groups.UpdateRange(entities);
            _ = await _dbContext.SaveChangesAsync();
        }
    }
}
