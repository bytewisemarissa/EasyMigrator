using System;
using System.Collections.Generic;
using System.Text;
using EasyMigrator.Commands;
using EasyMigrator.Configuration;
using EasyMigrator.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;

namespace EasyMigrator.Utility
{
    public class DatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        private ILogger _logger;
        private IOptions<ApplicationConfiguration> _configuration;

        public DatabaseConnectionFactory(
            ILogger<MigrationCommand> logger,
            IOptions<ApplicationConfiguration> configuration)
        {
            _logger = logger;
            _configuration = configuration;

            _logger.LogTrace("DatabaseConnectionFactory has been instantiated.");
        }

        public MySqlConnection GetDatabaseConnection()
        {
            _logger.LogDebug("Creating new sql connection for connection string, TargetDatabase.");

            return new MySqlConnection(_configuration.Value.ConnectionStrings.TargetDatabase);
        }
    }
}
