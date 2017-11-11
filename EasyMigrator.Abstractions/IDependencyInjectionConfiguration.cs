using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyMigrator.Abstractions
{
    public interface IDependencyInjectionConfiguration
    {
        void ConfigureContainer(IConfigurationRoot configuration, IServiceCollection collection);
    }
}
