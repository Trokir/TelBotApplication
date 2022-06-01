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
           
            EnvironmentBotConfiguration botConfig = new EnvironmentBotConfiguration();
            configuration.GetSection("BotConfig").Bind(botConfig);
            _ = services.AddSingleton(Options.Create(botConfig));
            string connection = configuration.GetConnectionString("DefaualtConnection");

            return services.AddDalDependensies(connection).AddIntegrationDependencies(configuration)
                ;
        }
    }
}
