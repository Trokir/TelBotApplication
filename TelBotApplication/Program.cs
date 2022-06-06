using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using TelBotApplication.Compostions;
using TelBotApplication.Domain.Mapping;

var builder = WebApplication.CreateBuilder(args);


ConfigureConfiguration(builder);
ConfigureServices(builder.Services);

var app = builder.Build();
ConfigureApp(app);

app.Run();

void ConfigureConfiguration(WebApplicationBuilder builder)
{
    builder.Services.AddDependencies(builder.Configuration);
}
void ConfigureServices(IServiceCollection services)
{
    _ = services.AddLogging(config =>
    {
        _ = config.AddDebug();
        _ = config.AddConsole();
        //etc
    });

    MapperConfiguration mapperConfig = new MapperConfiguration(mc =>
    {
        mc.AddProfile(new AutoMapperProfile());
    });
    IMapper mapper = mapperConfig.CreateMapper();
    _ = services.AddSingleton(mapper);


    _ = services.AddControllers();
    _ = services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "TelBotApplication", Version = "v1" });
    });
}
void ConfigureApp(WebApplication app)
{

    _ = app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TelBotApplication v1");
    });

    _ = app.UseHttpsRedirection();
    _ = app.UseStaticFiles();
    _ = app.UseRouting();
    _ = app.UseAuthorization();

    _ = app.UseEndpoints(endpoints =>
    {
        _ = endpoints.MapControllers();
    });
}
    //public class Program
    //{
    //    public static void Main(string[] args)
    //    //{
    //    //    IHost host = CreateHostBuilder(args).UseDefaultServiceProvider(options =>
    //    //    options.ValidateScopes = false).Build();
    //    //    using (IServiceScope scope = host.Services.CreateScope())
    //    //    {
    //    //        ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    //    //        logger.LogWarning("Bot started");
    //    //    }


    //    //    host.Run();
    //    var builder = WebApplication.CreateBuilder(args);

    //}

    //public static IHostBuilder CreateHostBuilder(string[] args) =>
    //        Host.CreateDefaultBuilder(args)
    //        .UseNServiceBus(hostBuilderContext =>
    //        {
    //            var endpointConfiguration = new EndpointConfiguration("TelbotApplcation.Clients");
    //            endpointConfiguration.UseTransport<LearningTransport>();
    //            endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);
    //            return endpointConfiguration;
    //        })
    //        .ConfigureAppConfiguration((context, config) =>
    //        {
    //            _ = config.SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
    //            _ = config.AddJsonFile("appsettings.json", optional: true, false);
    //            _ = config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, false);
    //            _ = config.AddEnvironmentVariables();

    //        })
    //        .ConfigureWebHostDefaults(webBuilder =>
    //            {
    //                _ = webBuilder.UseStartup<Startup>();
    //            });
    //     private static async Task OnCriticalError(ICriticalErrorContext context)
    //{
    //    var fatalMessage = $"The following critical error was " +
    //                       $"encountered: {Environment.NewLine}{context.Error}{Environment.NewLine}Process is shutting down. " +
    //                       $"StackTrace: {Environment.NewLine}{context.Exception.StackTrace}";
                    
    //    EventLog.WriteEntry(".NET Runtime", fatalMessage, EventLogEntryType.Error);

    //    try
    //    {
    //        await context.Stop().ConfigureAwait(false);
    //    }
    //    finally
    //    {
    //        Environment.FailFast(fatalMessage, context.Exception);
    //    }
    //}
            
 
