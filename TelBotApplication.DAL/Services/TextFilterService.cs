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
    public class TextFilterService : ITextFilterService
    {
        private readonly TelBotApplicationDbContext _dbContext;
        private readonly ILogger<TextFilter> _logger;
        public TextFilterService(ILogger<TextFilter> logger, TelBotApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }
        public async Task AddAsync(TextFilter entity)
        {
            _ = await _dbContext.TextFilters.AddAsync(entity).ConfigureAwait(false);
            _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DeleteAsync(TextFilter entity)
        {
            _ = _dbContext.TextFilters.Remove(entity);
            _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<TextFilter> GetByIdAsync(int id)
        {
            var entity = await _dbContext.TextFilters.FindAsync(id).ConfigureAwait(false);
            return entity;
        }

        public async Task<IEnumerable<TextFilter>> GetAllAsync()
        {
            return await _dbContext.TextFilters.ToListAsync().ConfigureAwait(false);
        }

        public async Task UpdateAsync(TextFilter entity)
        {
            _ = _dbContext.TextFilters.Update(entity);
            _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task UpdateListAsync(IEnumerable<TextFilter> entities)
        {
            _dbContext.TextFilters.UpdateRange(entities);
            _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<IEnumerable<TextFilter>> GetAllAsync(Expression<Func<TextFilter, bool>> predicate)
        {
            return await _dbContext.TextFilters.Where(predicate).ToListAsync().ConfigureAwait(false);
        }
        public async Task<TextFilter> FindAsync(Expression<Func<TextFilter, bool>> predicate)
        {
            return await _dbContext.TextFilters.Where(predicate).FirstOrDefaultAsync().ConfigureAwait(false);
        }
        public async Task DeleteRangeAsync(Expression<Func<TextFilter, bool>> predicate)
        {
            var list = await _dbContext.TextFilters.Where(predicate).ToListAsync().ConfigureAwait(false);
            _dbContext.TextFilters.RemoveRange(list);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
