using System;
using System.Collections.Generic;
using System.Text;
using EasyMigrator.Abstractions;

namespace TestMigrations
{
    public class DataGenerator1 : IDataGenerator
    {
        public string Name => "TestGen1";
        public List<string> DataGenerationScripts => new List<string>()
        {
            "TestMigrations.Scripts.Migration1.migration1-data.mysql",
            "TestMigrations.Scripts.Migration2.migration2-data.mysql",
            "TestMigrations.Scripts.Migration3.migration3-data.mysql"
        };
    }
}
