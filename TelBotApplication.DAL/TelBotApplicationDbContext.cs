﻿using Microsoft.EntityFrameworkCore;
using TelBotApplication.Domain.Models;

namespace TelBotApplication.DAL
{
    public class TelBotApplicationDbContext : DbContext
    {

        public TelBotApplicationDbContext(DbContextOptions<TelBotApplicationDbContext> options) : base(options) { }

        public DbSet<BotCaller> BotCallers { get; set; }
        public DbSet<VenueCommand> VenueCommands { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<MessageLogger> MessageLoggers { get; set; }
        public DbSet<MessageModel> MessageModels { get; set; }

    
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
        }
    }
}
