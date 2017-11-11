using System;
using System.Collections.Generic;
using System.Text;
using EasyMigrator.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestMigrations.Data;

namespace TestMigrations
{
    public class DIConfiguration : IDependencyInjectionConfiguration
    {
        public void ConfigureContainer(IConfigurationRoot configuration, IServiceCollection collection)
        {
            collection.AddDbContext<TestMigrationsDataContext>(options =>
                options.UseMySql(
                        configuration.GetConnectionString("TargetDatabase"))
                    .UseLoggerFactory(null));
        }
    }
}
