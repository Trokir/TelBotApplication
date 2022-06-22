using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TelBotApplication.DAL.Interfaces;
using TelBotApplication.Domain.Dtos;
using TelBotApplication.Domain.Models.Anchors;

namespace TelBotApplication.DAL.Services
{
    public class AnchorService : IAnchorService
    {
        private readonly TelBotApplicationDbContext _dbContext;
        private readonly ILogger<AnchorService> _logger;
        public AnchorService(TelBotApplicationDbContext dbContext, ILogger<AnchorService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        public async Task AddAsync(Anchor entity)
        {
            await _dbContext.Anchors.AddAsync(entity).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        public async Task DeleteAsync(Anchor entity)
        {
            _dbContext.Anchors.Remove(entity);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        public async Task DeleteRangeAsync(Expression<Func<Anchor, bool>> predicate)
        {
            var list = await _dbContext.Anchors.Where(predicate).ToListAsync().ConfigureAwait(false);
            _dbContext.Anchors.RemoveRange(list);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        public async Task<Anchor> FindAsync(Expression<Func<Anchor, bool>> predicate)
        {
            return await _dbContext.Anchors.Where(predicate).FirstOrDefaultAsync().ConfigureAwait(false);
        }
        public async Task<IEnumerable<Anchor>> GetAllAsync()
        {
            var result = await _dbContext.Anchors.Include(x=>x.AnchorCallback).ToListAsync().ConfigureAwait(false);
            return result;
        }
        public async Task<IEnumerable<Anchor>> GetAllAsync(Expression<Func<Anchor, bool>> predicate)
        {
            return await _dbContext.Anchors.Where(predicate).ToListAsync().ConfigureAwait(false);
        }
        public async Task<Anchor> GetByIdAsync(int id)
        {
            return await _dbContext.Anchors.SingleOrDefaultAsync(x => x.Id == id).ConfigureAwait(false);
        }
        public async Task UpdateAsync(Anchor entity)
        {
            _ = _dbContext.Anchors.Update(entity);
            _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        public async Task UpdateListAsync(IEnumerable<Anchor> entities)
        {
            _dbContext.Anchors.UpdateRange(entities);
            _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
