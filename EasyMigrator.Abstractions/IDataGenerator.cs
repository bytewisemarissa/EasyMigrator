using System;
using System.Collections.Generic;
using System.Text;

namespace EasyMigrator.Abstractions
{
    public interface IDataGenerator
    {
        string Name { get; }
        List<string> DataGenerationScript { get; }
    }
}
