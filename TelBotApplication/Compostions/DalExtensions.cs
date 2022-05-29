using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TelBotApplication.DAL;
using TelBotApplication.DAL.Services;

namespace TelBotApplication.Compostions
{
    public static class DalExtensions
    {
        public static IServiceCollection AddDalDependensies(this IServiceCollection services, string connectionString)
        {
            services.AddScoped<TelBotApplicationDbContext>();
            services.AddDbContext<TelBotApplicationDbContext>(opt => opt.UseSqlite(connectionString));
            SQLitePCL.Batteries.Init();
            return services;
        }
    }
}
