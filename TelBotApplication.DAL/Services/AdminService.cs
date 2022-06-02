using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
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
    }
}
