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
            _connection.On<string>(Strings.Events.MessageEdited, EditLog);
            _connection.On<string>(Strings.Events.PhotoSent, SendPhotoLog);
            _connection.On<string>(Strings.Events.DocumentSent, SendDocumentlLog);
            _connection.On<string>(Strings.Events.StickerSent, SendStickerLog);
            _connection.On<string>(Strings.Events.VideoSent, SendVideoLog);
        }

        public async Task SendVideoLog(string message)
        {
            await WriteLog(message, TypeOfMessageLog.Video);
        }
        public async Task SendStickerLog(string message)
        {
            await WriteLog(message, TypeOfMessageLog.Sticker);
        }

        public async Task SendPhotoLog(string message)
        {
            await WriteLog(message, TypeOfMessageLog.Photo);
        }
        public async Task EditLog(string message)
        {
            await WriteLog(message, TypeOfMessageLog.Edited);
        }

        public async Task SendLog(string message)
        {
            await WriteLog(message, TypeOfMessageLog.Added);
        }

        public async Task SendDocumentlLog(string message)
        {
            await WriteLog(message, TypeOfMessageLog.Document);
        }

        private async Task WriteLog(string message, TypeOfMessageLog typeOfMessage)
        {
            var arr = message.Split(':');
            if (arr is string[] array)
            {
                if (array.Any() && array[4] != null && array[4].Length > 10)
                {
                    
                    await _dbContext.MessageLoggerService.AddAsync(new MessageLogger
                    {
                        FullName = array[2],
                        UserName = array[3] ?? "no userName",
                        Message = array[4],
                        TypeOfMessageLog = typeOfMessage,
                        ChatId = long.Parse(array[0]),
                        AddedDate = DateTime.Now,
                        ChatTitle = array[5],
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
