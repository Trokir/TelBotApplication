using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TelBotApplication.Clients;
using TelBotApplication.DAL.Services;
using TelBotApplication.Domain.Abstraction;


namespace TelBotApplication.Compostions
{
    public static class ApplicationDependencies
    {
        public static IServiceCollection AddIntegrationDependencies(this IServiceCollection services, IConfiguration configuration, EnvironmentConfiguration environment)
        {
            _ = services.AddTransient<IBotCommandService, BotCommandService>();
            _ = services.AddSingleton<BotClientService>();
            _ = services.AddHostedService(provider => provider.GetService<BotClientService>());

            return services;
        }
    }
}
