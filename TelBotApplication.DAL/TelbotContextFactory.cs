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
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(@"E:\Projects\TelBotApplication\TelBotApplication"))
                .AddJsonFile("appsettings.Development.json")
                .Build();
            DbContextOptionsBuilder<TelBotApplicationDbContext> optionsBuilder = new DbContextOptionsBuilder<TelBotApplicationDbContext>();
            string connection = @"Data source=D:/Projects/TelBotApplication/TelBotApplication.DAL/telbot.db";
            _ = optionsBuilder.UseSqlite(connection);
            return new TelBotApplicationDbContext(optionsBuilder.Options);
        }

    }

}

