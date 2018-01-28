using System;
using System.Collections.Generic;
using System.Text;
using EasyMigrator.Abstractions;

namespace TestMigrations
{
    public class DataGenerator2 : IDataGenerator
    {
        public string Name => "TestGen2";
        public List<string> DataGenerationScripts => new List<string>()
        {
            "TestMigrations.Scripts.Migration1.migration1-data.mysql"
        };
    }
}
