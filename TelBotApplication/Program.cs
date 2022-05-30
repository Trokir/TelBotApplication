using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Reflection;

namespace TelBotApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).UseDefaultServiceProvider(options =>
            options.ValidateScopes = false).Build();
            using (IServiceScope scope = host.Services.CreateScope())
            {
                ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("Bot started");
            }


            host.Run();

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).

            ConfigureAppConfiguration((context, config) =>
            {
                _ = config.SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                _ = config.AddJsonFile("appsettings.json", optional: true, false);
                _ = config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, false);
                _ = config.AddEnvironmentVariables();

            }).ConfigureWebHostDefaults(webBuilder =>
                {
                    _ = webBuilder.UseStartup<Startup>();
                })

            ;
    }
}
