using System;
using System.Collections.Generic;

namespace EasyMigrator.Abstractions
{
    public interface IDatabaseMigration
    {
        int SerialNumber { get; }

        List<string> ScriptResourseListUp { get; }
        List<string> ScriptResourceListDown { get; }
    }
}
