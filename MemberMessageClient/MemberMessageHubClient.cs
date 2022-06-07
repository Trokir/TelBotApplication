using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TelBotApplication.Clients;
using TelBotApplication.Clients.Hubs;
using TelBotApplication.DAL.Interfaces;
using TelBotApplication.Domain.Enums;
using TelBotApplication.Domain.Models;

namespace MemberMessageClient
{
    public class MemberMessageHubClient : INewMember, IHostedService
    {
        private readonly ILogger<MemberMessageHubClient> _logger;
        private readonly IUnitOfWork _dbContext;
        private readonly HubConnection _connection;
        public MemberMessageHubClient(ILogger<MemberMessageHubClient> logger
            ,
            IUnitOfWork dbContext
            )
        {
            _dbContext = dbContext;
            _logger = logger;
            _connection = new HubConnectionBuilder()
                .WithUrl(Strings.HubUrl)
                .Build();

            _connection.On<string>(Strings.Events.MessageSent, SendLog);

        }


        public async Task SendLog(string message)
        {
           
            var arr = message.Split(':');
            if (arr is string[] array)
            {
                if (array.Any())
                {
                    var group = await _dbContext.GroupService.FindIdAsync(x => x.ChatId == long.Parse(array[0]));
                    if (group == null)
                    {
                        await _dbContext.GroupService.AddAsync(new Group { ChatId = long.Parse(array[0]) });
                    }
                    group = await _dbContext.GroupService.FindIdAsync(x => x.ChatId == long.Parse(array[0]));
                    await _dbContext.MessageLoggerService.AddAsync(new MessageLogger
                    {
                        GroupId = group.Id,
                        FullName = array[2],
                        UserName = array[3],
                        Message = array[4],
                        TypeOfMessageLog = TypeOfMessageLog.Added,
                        ChatId = long.Parse(array[0]),
                        Group =group
                    }); 
                }

            }
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Loop is here to wait until the server is running
            while (true)
            {
                try
                {
                    _logger.LogInformation("Started");
                    await _connection.StartAsync(cancellationToken);

                    break;
                }
                catch
                {
                    _logger.LogInformation("Error");
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
