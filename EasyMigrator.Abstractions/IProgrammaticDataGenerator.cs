using System;
using System.Collections.Generic;
using System.Text;

namespace EasyMigrator.Abstractions
{
    public interface IProgrammaticDataGenerator
    {
        string Name { get; }

        void GenerationRoutine();
    }
}
