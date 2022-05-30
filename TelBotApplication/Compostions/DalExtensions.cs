using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TelBotApplication.DAL;

namespace TelBotApplication.Compostions
{
    public static class DalExtensions
    {
        public static IServiceCollection AddDalDependensies(this IServiceCollection services, string connectionString)
        {
            _ = services.AddScoped<TelBotApplicationDbContext>();
            _ = services.AddDbContext<TelBotApplicationDbContext>(opt => opt.UseSqlite(connectionString));
            SQLitePCL.Batteries.Init();
            return services;
        }
    }
}
