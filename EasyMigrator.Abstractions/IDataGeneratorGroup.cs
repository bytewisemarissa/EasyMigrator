using System;
using System.Collections.Generic;
using System.Text;

namespace EasyMigrator.Abstractions
{
    public interface IDataGeneratorGroup
    {
        string Name { get; }
        List<string> DataGenerationActionNames { get; }
    }
}
