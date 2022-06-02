using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TelBotApplication.Clients;
using TelBotApplication.DAL;
using TelBotApplication.DAL.Services;
using TelBotApplication.Domain.Abstraction;
using TelBotApplication.Domain.Chats;
using TelBotApplication.Filters;

namespace TelBotApplication.Compostions
{
    public static class ApplicationDependencies
    {
        public static IServiceCollection AddIntegrationDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            _ = services.AddTransient<IBotCommandService, BotCommandService>()
                .AddTransient<IVenueCommandServise, VenueCommandServise>()
                .AddTransient<IUnitOfWork, UnitOfWork>()
                .AddSingleton<ISpamConfiguration, SpamConfiguration>()
                .AddSingleton<BotClientService>()
                .AddScoped<IFludFilter, FludFilter>()
                .AddTransient<IMemberExecutor, MemberExecutor>()
            .AddHostedService(provider => provider.GetService<BotClientService>());
             

            return services;
        }
    }
}
