using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EasyMigrator.Interfaces
{
    public interface IMigrationCommand
    {
        void SetTargetAssembly(Assembly targetAssembly);
        void PerformUpOperation();
        void PerformDownOperation();
        void PerformCurrentOperation();
        void PerformSerialOperation(int serial);
        void PerformLogTableCreation();
    }
}
