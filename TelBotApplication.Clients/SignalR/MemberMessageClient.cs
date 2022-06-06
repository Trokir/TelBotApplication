using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TelBotApplication.Clients.Hubs;

namespace TelBotApplication.Clients.SignalR
{
    public class MemberMessageClient : INewMember, IHostedService
    {
        private readonly ILogger<MemberMessageClient> _logger;
        private HubConnection _connection;
        public MemberMessageClient(ILogger<MemberMessageClient> logger)
        {
            _logger = logger;

            _connection = new HubConnectionBuilder()
                .WithUrl(Strings.HubUrl)
                .Build();

            _connection.On<string>(Strings.Events.MessageSent, SayHello);
        }
        public Task SayHello(string message)
        {
            _logger.LogInformation("{CurrentMessage}", message);
            Console.WriteLine(message);
            return Task.CompletedTask;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Loop is here to wait until the server is running
            while (true)
            {
                try
                {
                    await _connection.StartAsync(cancellationToken);

                    break;
                }
                catch
                {
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _connection.DisposeAsync();
        }
    }
}