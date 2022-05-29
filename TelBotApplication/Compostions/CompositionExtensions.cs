using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TelBotApplication.Domain.Abstraction;

namespace TelBotApplication.Compostions
{
    public static class CompositionExtensions
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            var environmentConfiguration = new EnvironmentConfiguration();
            configuration.GetSection("Environment").Bind(environmentConfiguration);
            var botConfig = new EnvironmentBotConfiguration();
            configuration.GetSection("BotConfig").Bind(botConfig);

            services.AddSingleton(Options.Create(environmentConfiguration));
            services.AddSingleton(Options.Create(botConfig));
            var connection = configuration.GetConnectionString("DefaualtConnection");

            return services.AddDalDependensies(connection).AddIntegrationDependencies(configuration, environmentConfiguration)
                ;
        }
    }
}
