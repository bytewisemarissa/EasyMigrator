using System;
using System.Collections.Generic;
using EasyMigrator.Abstractions;

namespace TestMigrations
{
    public class Migration2 : IDatabaseMigration
    {
        public int SerialNumber => 2;
        public List<string> ScriptResourseListUp => new List<string>()
        {
            "TestMigrations.Scripts.Migration2.migration2-schema.mysql",
            "TestMigrations.Scripts.Migration2.migration2-data.mysql"
        };

        public List<string> ScriptResourceListDown => new List<string>()
        {
            "TestMigrations.Scripts.Migration2.migration2-down.mysql"
        };
    }
}
