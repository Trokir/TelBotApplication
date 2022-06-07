using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using TelBotApplication.Clients.Hubs;
using TelBotApplication.Compostions;
using TelBotApplication.Domain.Mapping;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();

builder.Services.AddDependencies(builder.Configuration);
ConfigureServices(builder.Services);
var app = builder.Build();
ConfigureApp(app);


app.Run();


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
    app.UseCors(config => config.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
    _ = app.UseEndpoints(endpoints =>
    {
        endpoints.MapHub<MemberHub>("/hubs/member");
        _ = endpoints.MapControllers();
    });
}


