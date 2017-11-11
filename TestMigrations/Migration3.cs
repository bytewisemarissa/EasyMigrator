using System;
using System.Collections.Generic;
using EasyMigrator.Abstractions;

namespace TestMigrations
{
    public class Migration3 : IDatabaseMigration
    {
        public int SerialNumber => 3;
        public List<string> ScriptResourseListUp => new List<string>()
        {
            "TestMigrations.Scripts.Migration3.migration3-schema.mysql",
            "TestMigrations.Scripts.Migration3.migration3-data.mysql"
        };

        public List<string> ScriptResourceListDown => new List<string>()
        {
            "TestMigrations.Scripts.Migration3.migration3-down.mysql"
        };
    }
}
