using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelBotApplication.DAL;
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
            using IServiceScope scope = _factory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            return await context.VenueCommandServise.GetAllAsync();
          
        }

        public async Task<IEnumerable<BotCaller>> GetAllBotCommands()
        {
            using IServiceScope scope = _factory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            return await context.BotCommandService.GetAllAsync();
        }
    }
}
