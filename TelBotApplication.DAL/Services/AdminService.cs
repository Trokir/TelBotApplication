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
    public class AdminService : IAdminService
    {

        private readonly TelBotApplicationDbContext _dbContext;
        private readonly ILogger<AdminService> _logger;
        public AdminService(ILogger<AdminService> logger, TelBotApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }
        public async Task AddAsync(Admin entity)
        {
            _ = await _dbContext.Admins.AddAsync(entity);
            _ = await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Admin entity)
        {
            _ = _dbContext.Admins.Remove(entity);
            _ = await _dbContext.SaveChangesAsync();
        }

        public async Task<Admin> GetByIdAsync(int id)
        {
            Admin entity = await _dbContext.Admins.FindAsync(id);
            return entity;
        }

        public async Task<IEnumerable<Admin>> GetAllAsync()
        {
            return await _dbContext.Admins.ToListAsync();
        }

        public async Task UpdateAsync(Admin entity)
        {
            _ = _dbContext.Admins.Update(entity);
            _ = await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateListAsync(IEnumerable<Admin> entities)
        {
            _dbContext.Admins.UpdateRange(entities);
            _ = await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Admin>> GetAllAsync(Expression<Func<Admin, bool>> predicate)
        {
            return await _dbContext.Admins.Where(predicate).ToListAsync();
        }
        public async Task<Admin> FindIdAsync(Expression<Func<Admin, bool>> predicate)
        {
            return await _dbContext.Admins.Where(predicate).FirstOrDefaultAsync();
        }
        public async Task DeleteRangeAsync(Expression<Func<Admin, bool>> predicate)
        {
            var list = await _dbContext.Admins.Where(predicate).ToListAsync();
            _dbContext.Admins.RemoveRange(list);
            await _dbContext.SaveChangesAsync();
        }
       
    }
}
