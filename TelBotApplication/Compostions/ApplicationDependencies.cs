﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TelBotApplication.Clients;
using TelBotApplication.DAL.Interfaces;
using TelBotApplication.DAL.Services;
using TelBotApplication.Domain.Interfaces;
using TelBotApplication.Domain.ML;
using TelBotApplication.Filters;

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
                .AddScoped<IFludFilter, FludFilter>()
            .AddHostedService(provider => provider.GetService<BotClientService>());


            return services;
        }
    }
}
