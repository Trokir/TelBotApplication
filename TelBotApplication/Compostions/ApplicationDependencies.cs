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
            services.AddSingleton<UserMonutoringTelegramService>();
            services.AddHostedService(provider => provider.GetService<UserMonutoringTelegramService>());
            services.AddScoped<IBotCommandService, BotCommandService>();
            services.AddTransient<BotClientService>();
            services.AddHostedService(provider => provider.GetService<BotClientService>());

            return services;
        }
    }
}
