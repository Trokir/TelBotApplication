
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
            _ = await _dbContext.Groups.AddAsync(entity).ConfigureAwait(false);
            _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DeleteAsync(Group entity)
        {
            _ = _dbContext.Groups.Remove(entity);
            _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<Group> GetByIdAsync(int id)
        {
            Group entity = await _dbContext.Groups.FindAsync(id).ConfigureAwait(false);
            return entity;
        }

        public async Task<IEnumerable<Group>> GetAllAsync()
        {
            return await _dbContext.Groups.ToListAsync().ConfigureAwait(false);
        }

        public async Task UpdateAsync(Group entity)
        {
            _ = _dbContext.Groups.Update(entity);
            _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task UpdateListAsync(IEnumerable<Group> entities)
        {
            _dbContext.Groups.UpdateRange(entities);
            _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        public async Task<IEnumerable<Group>> GetAllAsync(Expression<Func<Group, bool>> predicate)
        {
            return await _dbContext.Groups.Where(predicate).ToListAsync().ConfigureAwait(false);
        }
        public async Task<Group> FindAsync(Expression<Func<Group, bool>> predicate)
        {
            return await _dbContext.Groups.Where(predicate).FirstOrDefaultAsync().ConfigureAwait(false);
        }
        public async Task DeleteRangeAsync(Expression<Func<Group, bool>> predicate)
        {
            var list = await _dbContext.Groups.Where(predicate).ToListAsync().ConfigureAwait(false);
            _dbContext.Groups.RemoveRange(list);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
       
    }
}
