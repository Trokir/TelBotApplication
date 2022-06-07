using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TelBotApplication.Clients;
using TelBotApplication.Clients.Hubs;
using TelBotApplication.DAL.Interfaces;

namespace MemberMessageClient
{
    public class MemberMessageHubClient : INewMember, IHostedService
    {
        private readonly ILogger<MemberMessageHubClient> _logger;
        private readonly IUnitOfWork _dbContext;
        private readonly HubConnection _connection;
        public MemberMessageHubClient(ILogger<MemberMessageHubClient> logger,
            IUnitOfWork dbContext)
        {
            _dbContext = dbContext;
            _logger = logger;
            _connection = new HubConnectionBuilder()
                .WithUrl(Strings.HubUrl)
                .Build();

            _connection.On<string>(Strings.Events.MessageSent, SendLog);

        }


        public Task SendLog(string message)
        {
            _logger.LogInformation("{CurrentMessage}", message);
           ///* _dbConte*/xt.Me
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
