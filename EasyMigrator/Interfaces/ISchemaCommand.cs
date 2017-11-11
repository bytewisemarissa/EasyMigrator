using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Bson;

namespace EasyMigrator.Interfaces
{
    public interface ISchemaCommand
    {
        void SetTargetAssembly(Assembly targetAssembly);
        void PerformCreateOperation();
        void PerformDestroyOperation();
        void PerformRebuildOperation();
    }
}
