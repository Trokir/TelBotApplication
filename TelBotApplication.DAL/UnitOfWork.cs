using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TelBotApplication.DAL.Services;

namespace TelBotApplication.DAL
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TelBotApplicationDbContext _dbContext;
        private readonly ILogger<UnitOfWork> _logger;
        public IBotCommandService BotCommandService { get; }
        public IVenueCommandServise VenueCommandServise { get; }
        public UnitOfWork(TelBotApplicationDbContext dbContext,
           IBotCommandService botCommandService,
            ILogger<UnitOfWork> logger,
            IVenueCommandServise venueCommandServise)
        {
            _dbContext = dbContext;
            BotCommandService = botCommandService;
            _logger = logger;
            VenueCommandServise = venueCommandServise;
        }
        public async Task<int> Complete()
        {
            return await _dbContext.SaveChangesAsync();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dbContext.Dispose();
            }
        }
    }
}
