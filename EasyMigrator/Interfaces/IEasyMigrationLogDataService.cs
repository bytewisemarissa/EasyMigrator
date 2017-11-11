using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EasyMigrator.Data.Models;

namespace EasyMigrator.Interfaces
{
    public interface IEasyMigrationLogDataService
    {
        List<EasyMigrationLog> GetMigrationLogs(string assemblyName);
        void UpdateMigrationLog(int targetSerial, string assemblyName);
    }
}
