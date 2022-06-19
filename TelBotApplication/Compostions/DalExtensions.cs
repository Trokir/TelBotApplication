using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TelBotApplication.DAL;
using TelBotApplication.DAL.Interfaces;
using TelBotApplication.DAL.Services;
using TelBotApplication.Domain.Models;

namespace TelBotApplication.Compostions
{
    public static class DalExtensions
    {
        public static IServiceCollection AddDalDependensies(this IServiceCollection services, string connectionString)
        {
            services
                    .AddScoped<IVenueCommandService, VenueCommandService>()
                .AddScoped<IAdminService, AdminService>()
                 .AddScoped<IMessageLoggerService, MessageLoggerService>()
                  .AddScoped<IBotCommandService, BotCommandService>()
                .AddScoped<IGroupService, GroupService>()
                 .AddScoped<ITextFilterService, TextFilterService>()
                .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<TelBotApplicationDbContext>()
            .AddDbContext<TelBotApplicationDbContext>(opt =>
            opt.UseSqlite(connectionString));
            SQLitePCL.Batteries.Init();
            return services;
        }
    }
}
