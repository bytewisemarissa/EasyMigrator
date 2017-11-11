using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Transactions;
using EasyMigrator.Abstractions;
using EasyMigrator.Configuration;
using EasyMigrator.Data;
using EasyMigrator.Data.Models;
using EasyMigrator.Interfaces;
using EasyMigrator.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;

namespace EasyMigrator.Commands
{
    public class MigrationCommand : IMigrationCommand
    {
        private readonly ILogger<MigrationCommand> _logger;
        
        private readonly ISqlCommandUtility _sqlCommandUtility;
        private readonly IAssemblyUtility _assemblyUtility;
        private readonly IEasyMigrationLogDataService _dataService;
        
        private Assembly _targetAssembly;

        public MigrationCommand(
            ILogger<MigrationCommand> logger,
            ISqlCommandUtility sqlCommandUtility,
            IAssemblyUtility assemblyUtility,
            IEasyMigrationLogDataService dataService)
        {
            _logger = logger;
            _sqlCommandUtility = sqlCommandUtility;
            _assemblyUtility = assemblyUtility;
            _dataService = dataService;

            _logger.LogTrace("MigrationCommand has been instantiated.");
        }

        public void SetTargetAssembly(Assembly targetAssembly)
        {
            _targetAssembly = targetAssembly;
        }

        public void PerformUpOperation()
        {
            TestForLogTable();

            List<IDatabaseMigration> migrations =
                _assemblyUtility.GetTypeFromAssembly<IDatabaseMigration>(_targetAssembly)
                .OrderBy(m => m.SerialNumber)
                .ToList();
            List<EasyMigrationLog> migrationLogs = _dataService.GetMigrationLogs(_targetAssembly.ManifestModule.Name);

            if (migrationLogs.Count > 0)
            {
                if (migrationLogs.Last().Serial == migrations.Last().SerialNumber)
                {
                    _logger.LogInformation("The database is already on the latest migration.");
                    return;
                }

                if (migrationLogs.Last().Serial > migrations.Last().SerialNumber)
                {
                    _logger.LogInformation(
                        "The database is on a migration serial not contained in the migrations assembly. Are you using the latest build?");
                    return;
                }
            }

            var targetSerial = migrationLogs.Count > 0 ? migrationLogs.Last().Serial + 1 : 1;

            var targetMigration = migrations.Single(m => m.SerialNumber == targetSerial);

            _logger.LogDebug($"The target migration serial is {targetSerial}.");
            
            Stopwatch timer = Stopwatch.StartNew();

            _sqlCommandUtility.RunEmbeddedResourceList(targetMigration.ScriptResourseListUp, _targetAssembly);
            
            timer.Stop();

            _dataService.UpdateMigrationLog(targetSerial, _targetAssembly.ManifestModule.Name);

            _logger.LogInformation($"Up migration operation completed. Took {timer.Elapsed.ToString()}.");
        }

        public void PerformDownOperation()
        {
            TestForLogTable();

            List <IDatabaseMigration> migrations =
                _assemblyUtility.GetTypeFromAssembly<IDatabaseMigration>(_targetAssembly)
                    .OrderBy(m => m.SerialNumber)
                    .ToList();
            List<EasyMigrationLog> migrationLogs = _dataService.GetMigrationLogs(_targetAssembly.ManifestModule.Name);

            if (migrationLogs.Count > 0)
            {
                if (migrationLogs.Last().Serial == 0)
                {
                    _logger.LogInformation("The database is already on the earliest migration.");
                    return;
                }
            }

            var targetSerial = migrationLogs.Last().Serial;

            var targetMigration = migrations.Single(m => m.SerialNumber == targetSerial);

            _logger.LogDebug($"The target migration serial is {targetSerial}.");

            Stopwatch timer = Stopwatch.StartNew();

            _sqlCommandUtility.RunEmbeddedResourceList(targetMigration.ScriptResourceListDown, _targetAssembly);

            timer.Stop();

            _dataService.UpdateMigrationLog(targetSerial - 1, _targetAssembly.ManifestModule.Name);

            _logger.LogInformation($"Down migration operation completed. Took {timer.Elapsed.ToString()}.");
        }
        
        public void PerformCurrentOperation()
        {
            TestForLogTable();

            List<IDatabaseMigration> migrations =
                _assemblyUtility.GetTypeFromAssembly<IDatabaseMigration>(_targetAssembly)
                    .OrderBy(m => m.SerialNumber)
                    .ToList();
            List<EasyMigrationLog> migrationLogs = _dataService.GetMigrationLogs(_targetAssembly.ManifestModule.Name);

            if (migrationLogs.Count > 0)
            {
                if (migrationLogs.Last().Serial == migrations.Last().SerialNumber)
                {
                    _logger.LogInformation("The database is already on the latest migration.");
                    return;
                }

                if (migrationLogs.Last().Serial > migrations.Last().SerialNumber)
                {
                    _logger.LogInformation(
                        "The database is on a migration serial not contained in the migrations assembly. Are you using the latest build?");
                    return;
                }
            }

            var targetMigration = migrations.Last();

            var targetSerial = targetMigration.SerialNumber;

            var currentSerial = migrationLogs.Count > 0 ? migrationLogs.Last().Serial : 0;

            _logger.LogDebug($"The target migration serial is {targetSerial}.");

            Stopwatch timer = Stopwatch.StartNew();

            while (targetSerial != currentSerial)
            {
                if (targetSerial < currentSerial)
                {
                    PerformDownOperation();
                }
                else
                {
                    PerformUpOperation();
                }

                migrationLogs = _dataService.GetMigrationLogs(_targetAssembly.ManifestModule.Name);
                currentSerial = migrationLogs.Last().Serial;
            }

            timer.Stop();

            _logger.LogInformation($"Current migration operation completed. Took {timer.Elapsed.ToString()}.");
        }

        public void PerformSerialOperation(int serial)
        {
            TestForLogTable();

            List<IDatabaseMigration> migrations =
                _assemblyUtility.GetTypeFromAssembly<IDatabaseMigration>(_targetAssembly)
                    .OrderBy(m => m.SerialNumber)
                    .ToList();

            List<EasyMigrationLog> migrationLogs = _dataService.GetMigrationLogs(_targetAssembly.ManifestModule.Name);

            if (migrationLogs.Count > 0)
            {
                if (migrations.Select(m => m.SerialNumber).All(i => i != serial))
                {
                    if (serial != 0)
                    {
                        _logger.LogCritical(
                            "The migrations assembly does not contain the specified serial.");
                        return;
                    }
                }

                if (migrationLogs.Last().Serial == serial)
                {
                    _logger.LogInformation(
                        "The database is already on the specified serial.");
                    return;
                }
            }

            var targetSerial = serial;

            var currentSerial = migrationLogs.Last().Serial;

            _logger.LogDebug($"The target migration serial is {targetSerial}.");

            Stopwatch timer = Stopwatch.StartNew();

            while (targetSerial != currentSerial)
            {
                if (targetSerial > currentSerial)
                {
                    PerformUpOperation();
                }
                else
                {
                    PerformDownOperation();
                }

                migrationLogs = _dataService.GetMigrationLogs(_targetAssembly.ManifestModule.Name);
                currentSerial = migrationLogs.Last().Serial;
            }

            timer.Stop();

            _logger.LogInformation($"Serial migration operation completed. Took {timer.Elapsed.ToString()}.");
        }

        public void PerformLogTableCreation()
        {
            if (!_sqlCommandUtility.TestIfLogTableExsists())
            {
                _logger.LogInformation("Creating table 'easymigrationlog'.");

                _sqlCommandUtility.CreateLogTable();
            }
            else
            {
                _logger.LogInformation("Log table has already been created.");
            }
        }

        private void TestForLogTable()
        {
            if (!_sqlCommandUtility.TestIfLogTableExsists())
            {
                throw new ApplicationException("Log table has not been created. Please create the log table before using EasyMigrator against this database.");
            }
        }
    }
}
