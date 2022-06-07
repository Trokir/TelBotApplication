using MemberMessageClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
Console.Title ="Client";

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.AddConsole();
    })
    .ConfigureServices(services =>
    {
        services.AddHostedService<MemberMessageHubClient>();
    })
    .Build();

host.Run();
