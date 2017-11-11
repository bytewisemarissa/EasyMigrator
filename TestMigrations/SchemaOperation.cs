using System;
using System.Collections.Generic;
using System.Text;
using EasyMigrator.Abstractions;

namespace TestMigrations
{
    public class SchemaOperation : ISchemaOperation
    {
        public List<string> CreateResourseList => new List<string>()
        {
            "TestMigrations.Scripts.Migration1.migration1-schema.mysql",
            "TestMigrations.Scripts.Migration2.migration2-schema.mysql",
            "TestMigrations.Scripts.Migration3.migration3-schema.mysql"
        };

        public List<string> DestroyResourceList => new List<string>()
        {
            "TestMigrations.Scripts.Migration3.migration3-down.mysql",
            "TestMigrations.Scripts.Migration2.migration2-down.mysql",
            "TestMigrations.Scripts.Migration1.migration1-down.mysql"
        };
    }
}
