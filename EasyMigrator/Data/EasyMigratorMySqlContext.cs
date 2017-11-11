using System;
using System.Collections.Generic;
using System.Text;
using EasyMigrator.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyMigrator.Data
{
    public class EasyMigratorMySqlContext : DbContext
    {
        public EasyMigratorMySqlContext(DbContextOptions<EasyMigratorMySqlContext> options) : base(options) { }

        public DbSet<EasyMigrationLog> EasyMigrationLog { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<EasyMigrationLog>().HasKey(eml => eml.MigrationId);

            base.OnModelCreating(builder);
        }
    }
}
