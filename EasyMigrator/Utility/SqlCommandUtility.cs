using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using EasyMigrator.Commands;
using EasyMigrator.Interfaces;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace EasyMigrator.Utility
{
    public class SqlCommandUtility : ISqlCommandUtility
    {
        private readonly ILogger<SqlCommandUtility> _logger;
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public SqlCommandUtility(ILogger<SqlCommandUtility> logger, IDatabaseConnectionFactory factory)
        {
            _logger = logger;
            _connectionFactory = factory;
        }

        public bool TestIfLogTableExsists()
        {
            using (var databaseConnection = _connectionFactory.GetDatabaseConnection())
            {
                databaseConnection.Open();
                
                _logger.LogDebug("Testing for presense of 'easymigrationlog' table.");

                MySqlCommand tableTestCommand = new MySqlCommand(
                    $"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '{databaseConnection.Database}' AND table_name = 'easymigrationlog' LIMIT 1;",
                    databaseConnection);

                var sqlResult = tableTestCommand.ExecuteScalar();

                var result = Convert.ToBoolean(sqlResult);

                _logger.LogDebug($"Test complete the result was, {result}.");

                return result;
            }
        }

        public void CreateLogTable()
        {
            _logger.LogDebug("Starting log table creation.");

            var assembly = Assembly.GetEntryAssembly();

            var resourceStream =
                assembly.GetManifestResourceStream("EasyMigrator.Data.Script.easymigrationlog.mysql");

            using (var reader = new StreamReader(resourceStream))
            {
                var commandString = reader.ReadToEnd();

                _logger.LogDebug("Fetched log creation script from embedded resource.");

                using (var databaseConnection = _connectionFactory.GetDatabaseConnection())
                {
                    databaseConnection.Open();

                    MySqlCommand logTableCreation = new MySqlCommand(commandString, databaseConnection);
                    logTableCreation.ExecuteNonQuery();

                    _logger.LogDebug("Log table created.");
                }
            }
        }

        public void RunEmbeddedResourceList(
            List<string> resourcesToRun,
            Assembly targetAssembly)
        {
            using (var databaseConnection = _connectionFactory.GetDatabaseConnection())
            {
                databaseConnection.Open();

                MySqlCommand command = databaseConnection.CreateCommand();

                using (var transaction = databaseConnection.BeginTransaction())
                {
                    try
                    {
                        command.Connection = databaseConnection;
                        command.Transaction = transaction;

                        command.CommandText = "SET autocommit = 0;";
                        command.ExecuteNonQuery();

                        foreach (var script in resourcesToRun)
                        {
                            try
                            {
                                _logger.LogDebug($"Running resource stream {script}.");

                                var resourceStream = targetAssembly.GetManifestResourceStream(script);

                                if (resourceStream == null)
                                {
                                    throw new NullReferenceException(
                                        $"Could not find the specified file '{script}' in a resource stream of the migration dll. " +
                                        $"Did you include it as an embedded resource?");
                                }

                                using (var reader = new StreamReader(resourceStream))
                                {
                                    var commandString = reader.ReadToEnd();

                                    command.CommandText = commandString;
                                    command.ExecuteNonQuery();
                                }
                            }
                            catch
                            {
                                _logger.LogCritical($"There was a problem running the script '{script}'.");
                                throw;
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        _logger.LogCritical(
                            "Database may be in an inconsistant state! " +
                            "Any DDL statements can not be rolled back!");
                        throw;
                    }
                    finally
                    {
                        command.CommandText = "SET autocommit = 1;";
                        command.Transaction = null;
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
