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

            var botConfig = new EnvironmentBotConfiguration();
            configuration.GetSection("BotConfig").Bind(botConfig);
            _ = services.AddSingleton(Options.Create(botConfig));
            return services.AddDalDependensies(@"Data source=D:/Projects/TelBotApplication/TelBotApplication.DAL/telbot.db").AddIntegrationDependencies(configuration);

        }
    }
}
