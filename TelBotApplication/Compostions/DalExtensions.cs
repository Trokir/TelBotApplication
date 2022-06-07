using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TelBotApplication.DAL;
using TelBotApplication.DAL.Interfaces;
using TelBotApplication.DAL.Services;

namespace TelBotApplication.Compostions
{
    public static class DalExtensions
    {
        public static IServiceCollection AddDalDependensies(this IServiceCollection services, string connectionString)
        {
            services
                    .AddTransient<IVenueCommandService, VenueCommandService>()
                .AddTransient<IAdminService, AdminService>()
                .AddTransient<IGroupService, GroupService>()
                 .AddTransient<IMessageLoggerService, MessageLoggerService>()
                  .AddTransient<IBotCommandService, BotCommandService>()
                .AddTransient<IGroupService, GroupService>()
                .AddTransient<ITextFilter, TextFilter>()
                .AddTransient<IUnitOfWork, UnitOfWork>()
            .AddScoped<TelBotApplicationDbContext>()
            .AddDbContext<TelBotApplicationDbContext>(opt =>
            opt.UseSqlite(connectionString));
            SQLitePCL.Batteries.Init();
            return services;
        }
    }
}
