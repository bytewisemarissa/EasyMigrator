using System;
using System.Collections.Generic;
using System.Text;

namespace EasyMigrator.Abstractions
{
    public interface ISchemaOperation
    {
        List<string> CreateResourseList { get; }
        List<string> DestroyResourceList { get; }
    }
}
