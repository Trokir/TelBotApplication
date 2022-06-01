using Microsoft.EntityFrameworkCore;
using TelBotApplication.Domain.Models;

namespace TelBotApplication.DAL
{
    public class TelBotApplicationDbContext : DbContext
    {

        public TelBotApplicationDbContext(DbContextOptions<TelBotApplicationDbContext> options) : base(options) { }

        public DbSet<BotCaller> BotCallers { get; set; }
        public DbSet<UserListener> UserListeners { get; set; }
    
        public DbSet<MessageModel> MessageModels { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
        }
    }
}
