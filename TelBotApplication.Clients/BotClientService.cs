using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace TelBotApplication.Clients
{
    public class BotClientService : BackgroundService
    {
        public IServiceProvider Services { get; }

        public BotClientService(IServiceProvider services)

        {
            Services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService =
                    scope.ServiceProvider
                        .GetRequiredService<IScopedProcessingService>();

                await scopedProcessingService.StartChatPolling(stoppingToken);
            }
        }

    }
}
