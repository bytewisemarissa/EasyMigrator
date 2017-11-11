using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace TestMigrations.Data
{
    public class TestMigrationsDataContext : DbContext
    {
        public TestMigrationsDataContext(DbContextOptions<TestMigrationsDataContext> options) : base(options) { }

        public DbSet<TestTableOne> TestTableOne { get; set; }
        public DbSet<TestTableTwo> TestTableTwo { get; set; }
        public DbSet<TestTableThree> TestTableThree { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<TestTableOne>().HasKey(kvp => kvp.Id);
            builder.Entity<TestTableTwo>().HasKey(kvp => kvp.Id);
            builder.Entity<TestTableThree>().HasKey(kvp => kvp.Id);

            base.OnModelCreating(builder);
        }
    }
}
