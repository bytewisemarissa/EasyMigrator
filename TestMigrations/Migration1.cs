using System;
using System.Collections.Generic;
using EasyMigrator.Abstractions;

namespace TestMigrations
{
    public class Migration1 : IDatabaseMigration
    {
        public int SerialNumber => 1;
        public List<string> ScriptResourseListUp => new List<string>()
        {
            "TestMigrations.Scripts.Migration1.migration1-schema.mysql",
            "TestMigrations.Scripts.Migration1.migration1-data.mysql"
        };

        public List<string> ScriptResourceListDown => new List<string>()
        {
            "TestMigrations.Scripts.Migration1.migration1-down.mysql"
        };
    }
}
