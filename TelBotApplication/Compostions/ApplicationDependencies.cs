using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TelBotApplication.Clients;
using TelBotApplication.Clients.SignalR;
using TelBotApplication.DAL.Interfaces;
using TelBotApplication.DAL.Services;
using TelBotApplication.Domain.Interfaces;
using TelBotApplication.Domain.ML;

namespace TelBotApplication.Compostions
{
    public static class ApplicationDependencies
    {
        public static IServiceCollection AddIntegrationDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            _ = services
                .AddTransient<IScopedProcessingService, ScopedProcessingService>()
                .AddTransient<IBotCommandService, BotCommandService>()
                .AddTransient<ITextFilter, TextFilter>()
                .AddSingleton<ISpamConfiguration, SpamConfiguration>()
                .AddTransient<BotClientService>()
                .AddHostedService(provider => provider.GetService<BotClientService>())
                .AddHostedService<MemberMessageClient>();


            return services;
        }
    }
}
