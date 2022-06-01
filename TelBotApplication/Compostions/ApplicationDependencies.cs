using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TelBotApplication.Clients;
using TelBotApplication.DAL.Services;
using TelBotApplication.Domain.Abstraction;
using TelBotApplication.Filters;

namespace TelBotApplication.Compostions
{
    public static class ApplicationDependencies
    {
        public static IServiceCollection AddIntegrationDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            _ = services.AddTransient<IBotCommandService, BotCommandService>()
                .AddSingleton<ISpamConfiguration, SpamConfiguration>()
                .AddSingleton<BotClientService>()
                .AddScoped< IFludFilter,FludFilter >()
            .AddHostedService(provider => provider.GetService<BotClientService>());

            return services;
        }
    }
}
