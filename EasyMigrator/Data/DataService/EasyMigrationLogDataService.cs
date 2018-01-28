using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using EasyMigrator.Data.Models;
using EasyMigrator.Interfaces;
using Microsoft.Extensions.Logging;

namespace EasyMigrator.Data.DataService
{
    public class EasyMigrationLogDataService : IEasyMigrationLogDataService
    {
        private readonly ILogger _logger;
        private readonly EasyMigratorMySqlContext _databaseContext;


        public EasyMigrationLogDataService(
            ILogger<EasyMigrationLogDataService> logger,
            EasyMigratorMySqlContext databaseContext
            )
        {
            _logger = logger;
            _databaseContext = databaseContext;
        }

        public List<EasyMigrationLog> GetMigrationLogs(string assemblyName)
        {
            return _databaseContext.EasyMigrationLog
                .Where(ml => ml.ImplementationNamespace == assemblyName)
                .OrderBy(ml => ml.MigrationId)
                .ToList();
        }
        
        public void UpdateMigrationLog(int targetSerial, string assemblyName)
        {
            _logger.LogDebug($"Adding migration log.");

            _databaseContext.EasyMigrationLog.Add(new EasyMigrationLog()
            {
                ImplementationNamespace = assemblyName,
                PerformedOn = DateTime.UtcNow,
                Serial = targetSerial
            });
            _databaseContext.SaveChanges();
        }
    }
}
