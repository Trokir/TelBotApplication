using MemberMessageClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TelBotApplication.DAL;
using TelBotApplication.DAL.Interfaces;
using TelBotApplication.DAL.Services;

Console.Title ="Client";

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.AddConsole();
    })
    .ConfigureServices(services =>
    {
        services.AddHostedService<MemberMessageHubClient>()
        .AddTransient<IVenueCommandService, VenueCommandService>()
                .AddTransient<IAdminService, AdminService>()
                .AddTransient<IGroupService, GroupService>()
                 .AddTransient<IMessageLoggerService, MessageLoggerService>()
                  .AddTransient<IBotCommandService, BotCommandService>()
                .AddTransient<IGroupService, GroupService>()
                .AddTransient<IUnitOfWork, UnitOfWork>()
        .AddScoped<TelBotApplicationDbContext>();
        services.AddDbContext<TelBotApplicationDbContext>(opt =>
        opt.UseSqlite(@"Data source=E:/Projects/TelBotApplication/TelBotApplication.DAL/telbot.db"));
        SQLitePCL.Batteries.Init();
    })
    .Build();

host.Run();
