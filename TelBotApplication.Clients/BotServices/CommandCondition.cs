using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TelBotApplication.DAL.Interfaces;
using TelBotApplication.Domain.Models;

namespace TelBotApplication.Clients.BotServices
{
    public class CommandCondition : ICommandCondition
    {
        private readonly IServiceProvider _factory;
        private readonly ILogger<Filter> _logger;
        public CommandCondition(IServiceProvider factory, ILogger<Filter> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        public async Task<IEnumerable<VenueCommand>> GetAllLocations()
        {
            using var scope = _factory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            return await context.VenueCommandServise.GetAllAsync();

        }

        public async Task<IEnumerable<BotCaller>> GetAllBotCommands()
        {
            using var scope = _factory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            return await context.BotCommandService.GetAllAsync();
        }
    }
}
