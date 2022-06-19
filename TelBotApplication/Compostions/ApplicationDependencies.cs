﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TelBotApplication.Clients;
using TelBotApplication.Clients.BotServices;
using TelBotApplication.Domain.Chats;
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
                .AddSingleton<ISpamConfiguration, SpamConfiguration>()
                .AddScoped<IFilter, Filter>()
                 .AddScoped<ICommandCondition, CommandCondition>()
                .AddTransient<IMemberExecutor, MemberExecutor>()
                .AddSingleton<BotClientService>()
                .AddHostedService(provider => provider.GetService<BotClientService>());


            return services;
        }
    }
}
