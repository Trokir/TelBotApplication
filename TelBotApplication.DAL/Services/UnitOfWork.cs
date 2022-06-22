using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TelBotApplication.DAL.Interfaces;

namespace TelBotApplication.DAL.Services
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TelBotApplicationDbContext _dbContext;
        private readonly ILogger<UnitOfWork> _logger;
        public IBotCommandService BotCommandService { get; }
        public IVenueCommandService VenueCommandServise { get; }
        public IMessageLoggerService MessageLoggerService { get; }
        public ITextFilterService TextFilterService { get; }
        public IAnchorService AnchorService { get; }

        public UnitOfWork(TelBotApplicationDbContext dbContext,
             ILogger<UnitOfWork> logger,
            IBotCommandService botCommandService,
            IVenueCommandService venueCommandServise,
            IMessageLoggerService messageLoggerService,
            ITextFilterService textFilterService,
            IAnchorService anchorService)
        {
            _dbContext = dbContext;
            BotCommandService = botCommandService;
            _logger = logger;
            VenueCommandServise = venueCommandServise;
            MessageLoggerService = messageLoggerService;
            TextFilterService = textFilterService;
            AnchorService = anchorService;
        }
        public async Task<int> Complete()
        {
            return await _dbContext.SaveChangesAsync().ConfigureAwait(false);
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
