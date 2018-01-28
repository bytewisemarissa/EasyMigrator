using System;
using System.Collections.Generic;
using System.Text;
using EasyMigrator.Commands;
using EasyMigrator.Configuration;
using EasyMigrator.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;

namespace EasyMigrator.Utility
{
    public class DatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        private readonly ILogger _logger;
        private readonly IConfigurationRoot _configuration;
        
        public DatabaseConnectionFactory(
            ILogger<MigrationCommand> logger,
            IConfigurationRoot configuration) 
        {
            _logger = logger;
            _configuration = configuration;

            _logger.LogTrace("DatabaseConnectionFactory has been instantiated.");
        }
        

        public MySqlConnection GetDatabaseConnection()
        {
            _logger.LogDebug($"Creating new mysql connection for connection string named, {ApplicationOptionManager.ConnectionStringName}.");

            return new MySqlConnection(_configuration.GetConnectionString(ApplicationOptionManager.ConnectionStringName));
        }
    }
}
