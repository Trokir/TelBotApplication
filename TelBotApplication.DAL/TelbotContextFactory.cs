using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace TelBotApplication.DAL
{
    public class TelbotContextFactory : IDesignTimeDbContextFactory<TelBotApplicationDbContext>
    {

        public TelBotApplicationDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(@"E:\Projects\TelBotApplication\TelBotApplication"))
                .AddJsonFile("appsettings.Development.json")
                .Build();
            var optionsBuilder = new DbContextOptionsBuilder<TelBotApplicationDbContext>();
            var connection = @"Data source=D:/Projects/TelBotApplication/TelBotApplication.DAL/telbot.db";
            _ = optionsBuilder.UseSqlite(connection);
            return new TelBotApplicationDbContext(optionsBuilder.Options);
        }

    }

}

