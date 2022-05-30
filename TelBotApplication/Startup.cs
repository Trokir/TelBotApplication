using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using TelBotApplication.Compostions;
using TelBotApplication.Domain.Mapping;

namespace TelBotApplication
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;

        }



        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
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
            _ = services.AddDependencies(_configuration);

            _ = services.AddControllers();
            _ = services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TelBotApplication", Version = "v1" });
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                _ = app.UseDeveloperExceptionPage();
                _ = app.UseSwagger();
                _ = app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TelBotApplication v1"));
            }

            _ = app.UseHttpsRedirection();
            _ = app.UseStaticFiles();
            _ = app.UseRouting();
            _ = app.UseAuthorization();

            _ = app.UseEndpoints(endpoints =>
            {
                _ = endpoints.MapControllers();
            });
        }
    }
}
