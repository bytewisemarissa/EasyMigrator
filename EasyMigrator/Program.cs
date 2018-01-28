using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using EasyMigrator.Abstractions;
using EasyMigrator.Commands;
using EasyMigrator.Configuration;
using EasyMigrator.Data;
using EasyMigrator.Interfaces;
using EasyMigrator.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EasyMigrator
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandLineApplication application = new CommandLineApplication()
            {
                Name = "easymigrator",
                Description = "A .NET Core datbase migration utility targeted for MySQL.",
                FullName = ConsoleUtility.ComposeFullName()
            };

            CommandOption configurationOption = application.Option(
                "-c|--config",
                "Sets the config file to use. (Defaults to 'configuration.json'.)",
                CommandOptionType.SingleValue);

            CommandOption logLevelOption = application.Option(
                "-l|--log",
                "Sets the logging level. (trace, debug, information, warning, error, critical, none. Defaults to 'information'.)",
                CommandOptionType.SingleValue);

            CommandOption targetAssemblyOption = application.Option(
                "-t|--target",
                "Sets the assembly to use for migrations.",
                CommandOptionType.SingleValue);

            CommandOption noWarnOption = application.Option(
                "-f|--nowarn",
                "Skip backup warning, I know what I'm doing.",
                CommandOptionType.NoValue);

            CommandOption connectionStringOption = application.Option(
                "-cs|--connectionstring",
                "Sets the name of the connection string for the operation to use.",
                CommandOptionType.SingleValue);

            application.HelpOption("-?|-h|--help");

            application.Command("migrate", migrateCommand =>
            {
                migrateCommand.Description = "Perform a migration.";

                CommandOption upOption = migrateCommand.Option(
                    "-u|--up",
                    "Perform an upwards migration.",
                    CommandOptionType.NoValue);

                CommandOption downOption = migrateCommand.Option(
                    "-d|--down",
                    "Perform an downwards migration.",
                    CommandOptionType.NoValue);

                CommandOption currentOption = migrateCommand.Option(
                    "-c|--current",
                    "Perform upwards migrations until the database is current.",
                    CommandOptionType.NoValue);

                CommandOption serialOption = migrateCommand.Option(
                    "-s|--serial",
                    "Perform migrations, up or down, until the database is at the target serial.",
                    CommandOptionType.SingleValue);

                CommandOption logTableOption = migrateCommand.Option(
                    "-l|--logtable",
                    "Create the log table if it does not exsist.",
                    CommandOptionType.NoValue);

                migrateCommand.HelpOption("-?|-h|--help");

                migrateCommand.OnExecute(() =>
                {
                    if (!connectionStringOption.HasValue())
                    {
                        Console.WriteLine("You must choose a connection string name.");
                        return 2;
                    }

                    ApplicationOptionManager.ConnectionStringName = connectionStringOption.Value();

                    if (!targetAssemblyOption.HasValue())
                    {
                        Console.WriteLine("You must choose an assembly for migrations.");
                        return 2;
                    }

                    Assembly assembly;
                    try
                    { 
                        var assemblyPath = $"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\{targetAssemblyOption.Value()}";
                        assembly = Assembly.LoadFile(assemblyPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("There was a problem loading the target migration assembly.");
                        Console.WriteLine(ex.Message);
                        return 2;
                    }

                    if (!upOption.HasValue() &&
                        !downOption.HasValue() &&
                        !currentOption.HasValue() &&
                        !serialOption.HasValue() &&
                        !logTableOption.HasValue())
                    {
                        migrateCommand.ShowHelp();
                        return 2;
                    }

                    if (!noWarnOption.HasValue())
                    {
                        ConsoleUtility.PerformWarning();
                    }

                    SetupApplication(
                        configurationOption.HasValue() ? configurationOption.Value() : "configuration.json",
                        logLevelOption.HasValue() ? logLevelOption.Value() : "warning",
                        assembly,
                        connectionStringOption.Value());
                    
                    IMigrationCommand migrationCommand = ServiceProviderManager.ServiceProvider.GetService<IMigrationCommand>();
                    migrationCommand.SetTargetAssembly(assembly);

                    if (logTableOption.HasValue())
                    {
                        migrationCommand.PerformLogTableCreation();
                        return 0;
                    }

                    if (upOption.HasValue() ^
                        downOption.HasValue() ^
                        currentOption.HasValue() ^
                        serialOption.HasValue())
                    {
                        if (upOption.HasValue())
                        {
                            migrationCommand.PerformUpOperation();
                        }

                        if (downOption.HasValue())
                        {
                            migrationCommand.PerformDownOperation();
                        }

                        if (currentOption.HasValue())
                        {   
                            migrationCommand.PerformCurrentOperation();
                        }

                        if (serialOption.HasValue())
                        {
                            string unparsedSerial = serialOption.Value();

                            int serial;
                            try
                            {
                                serial = Convert.ToInt32(unparsedSerial);
                            }
                            catch
                            {
                                Console.WriteLine("There was a problem parsing the serial value provided.");
                                return 2;
                            }

                            migrationCommand.PerformSerialOperation(serial);
                        }

                        return 0;
                    }

                    Console.WriteLine("The up, down, current, and serial option are mutually exclusive.");
                    return 2;
                });
            });

            application.Command("schema", schemaCommand =>
            {
                CommandOption createOption = schemaCommand.Option(
                    "-c|--create",
                    "Attempts to create the schema from scratch.",
                    CommandOptionType.NoValue);

                CommandOption destroyOption = schemaCommand.Option(
                    "-d|--destroy",
                    "Attempts to delete the schema.",
                    CommandOptionType.NoValue);

                CommandOption rebuildOption = schemaCommand.Option(
                    "-r|--rebuild",
                    "Destroys and rebuilds the schema.",
                    CommandOptionType.NoValue);
                
                schemaCommand.HelpOption("-?|-h|--help");

                schemaCommand.OnExecute(() =>
                {
                    if (!connectionStringOption.HasValue())
                    {
                        Console.WriteLine("You must choose a connection string name.");
                        return 2;
                    }

                    ApplicationOptionManager.ConnectionStringName = connectionStringOption.Value();

                    if (!targetAssemblyOption.HasValue())
                    {
                        Console.WriteLine("You must choose an assembly for migrations.");
                        return 2;
                    }

                    Assembly assembly;
                    try
                    {
                        var assemblyPath = $"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\{targetAssemblyOption.Value()}";
                        assembly = Assembly.LoadFile(assemblyPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("There was a problem loading the target migration assembly.");
                        Console.WriteLine(ex.Message);
                        return 2;
                    }

                    if (!createOption.HasValue() &&
                        !destroyOption.HasValue() &&
                        !rebuildOption.HasValue() )
                    {
                        schemaCommand.ShowHelp();
                        return 2;
                    }

                    if (!noWarnOption.HasValue())
                    {
                        ConsoleUtility.PerformWarning();
                    }

                    SetupApplication(
                        configurationOption.HasValue() ? configurationOption.Value() : "configuration.json",
                        logLevelOption.HasValue() ? logLevelOption.Value() : "warning",
                        assembly,
                        connectionStringOption.Value());

                    ISchemaCommand command = ServiceProviderManager.ServiceProvider.GetService<ISchemaCommand>();
                    command.SetTargetAssembly(assembly);

                    if (createOption.HasValue())
                    {
                        command.PerformCreateOperation();
                    }

                    if (destroyOption.HasValue())
                    {
                        command.PerformDestroyOperation();
                    }

                    if (rebuildOption.HasValue())
                    {
                        command.PerformRebuildOperation();
                    }

                    return 0;
                });
            });

            application.Command("datagen", datagenCommand =>
            {
                CommandOption nameOption = datagenCommand.Option(
                    "-n|--name",
                    "Attempts to run the data generation action of this name.",
                    CommandOptionType.SingleValue);

                CommandOption listOption = datagenCommand.Option(
                    "-l|--list",
                    "Lists all datagen actions in an assembly.",
                    CommandOptionType.NoValue);

                datagenCommand.HelpOption("-?|-h|--help");

                datagenCommand.OnExecute(() =>
                {
                    if (!connectionStringOption.HasValue())
                    {
                        Console.WriteLine("You must choose a connection string name.");
                        return 2;
                    }

                    ApplicationOptionManager.ConnectionStringName = connectionStringOption.Value();

                    if (!targetAssemblyOption.HasValue())
                    {
                        Console.WriteLine("You must choose an assembly for migrations.");
                        return 2;
                    }

                    Assembly assembly;
                    try
                    {
                        var assemblyPath = $"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\{targetAssemblyOption.Value()}";
                        assembly = Assembly.LoadFile(assemblyPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("There was a problem loading the target migration assembly.");
                        Console.WriteLine(ex.Message);
                        return 2;
                    }

                    if (!nameOption.HasValue() && !listOption.HasValue())
                    {
                        datagenCommand.ShowHelp();
                        return 2;
                    }

                    if (!noWarnOption.HasValue())
                    {
                        ConsoleUtility.PerformWarning();
                    }

                    SetupApplication(
                        configurationOption.HasValue() ? configurationOption.Value() : "configuration.json",
                        logLevelOption.HasValue() ? logLevelOption.Value() : "warning",
                        assembly,
                        connectionStringOption.Value());

                    IDataGeneratorCommand command = ServiceProviderManager.ServiceProvider.GetService<IDataGeneratorCommand>();
                    command.SetTargetAssembly(assembly);

                    if (listOption.HasValue())
                    {
                        command.DisplayDataGenNames();
                        return 0;
                    }

                    command.PerformDataGenerationOperation(nameOption.Value());

                    return 0;
                });
            });

            application.OnExecute(() =>
            {
                application.ShowHelp();
                return 2;
            });

            try
            {
                application.Execute(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void SetupApplication(string configurationName, string logLevel, Assembly targetAssembly, string connectionStringName)
        {
            IConfigurationRoot configuration = SetupConfiguration(configurationName);

            LogLevel selectedLogLevel;
            switch (logLevel.ToLower())
            {
                case "trace":
                    selectedLogLevel = LogLevel.Trace;
                    break;
                case "debug":
                    selectedLogLevel = LogLevel.Debug;
                    break;
                case "information":
                    selectedLogLevel = LogLevel.Information;
                    break;
                case "warning":
                    selectedLogLevel = LogLevel.Warning;
                    break;
                case "error":
                    selectedLogLevel = LogLevel.Error;
                    break;
                case "critical":
                    selectedLogLevel = LogLevel.Critical;
                    break;
                case "none":
                    selectedLogLevel = LogLevel.None;
                    break;
                default:
                    Console.WriteLine("The log level specified could not be found. Using 'trace'.");
                    selectedLogLevel = LogLevel.Trace;
                    break;
            }

            IServiceProvider diContainer = SetupDependencyInjection(configuration, selectedLogLevel, targetAssembly, connectionStringName);

            ServiceProviderManager.ServiceProvider = diContainer;
        }

        private static IConfigurationRoot SetupConfiguration(string configurationFileName = "configuration.json")
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(configurationFileName);

            return builder.Build();
        }
        
        private static IServiceProvider SetupDependencyInjection(
            IConfigurationRoot configuration, 
            LogLevel selectedLogLevel, 
            Assembly targetAssembly,
            string connectionStringName)
        {
            ServiceCollection collection = new ServiceCollection();

            collection.Configure<ApplicationConfiguration>(configuration);
            collection.Configure<ApplicationSettings>(configuration.GetSection("ApplicationSettings"));
            collection.Configure<ConnectionStrings>(configuration.GetSection("ConnectionStrings"));

            collection.AddLogging((builder =>
            {
                builder.SetMinimumLevel(selectedLogLevel);
                builder.AddConsole();
            }));
            
            collection.AddDbContext<EasyMigratorMySqlContext>(options =>
                options.UseMySql(
                    configuration.GetConnectionString(connectionStringName))
                        .UseLoggerFactory(null));

            collection.AddTransient<IDatabaseConnectionFactory, DatabaseConnectionFactory>();
            collection.AddTransient<ISqlCommandUtility, SqlCommandUtility>();
            collection.AddTransient<IAssemblyUtility, AssemblyUtility>();

            collection.AddTransient<IMigrationCommand, MigrationCommand>();
            collection.AddTransient<ISchemaCommand, SchemaCommand>();
            collection.AddTransient<IDataGeneratorCommand, DataGeneratorCommand>();
            collection.AddTransient<IConfigurationRoot>(provider => configuration);

            var libraryDiConfig = GetAssemblyDiConfiguration(targetAssembly);
            foreach (var config in libraryDiConfig)
            {
                config.ConfigureContainer(configuration, collection);
            }

            return collection.BuildServiceProvider();
        }

        private static List<IDependencyInjectionConfiguration> GetAssemblyDiConfiguration(Assembly targetAssembly)
        {
            var searchType = typeof(IDependencyInjectionConfiguration);

            var types = targetAssembly.ExportedTypes.Where(t => searchType.IsAssignableFrom(t)).ToList();
            
            List<IDependencyInjectionConfiguration> returnValue = new List<IDependencyInjectionConfiguration>();
            foreach (var migrationType in types)
            {
                returnValue.Add(Activator.CreateInstance(migrationType) as IDependencyInjectionConfiguration);
            }

            return returnValue.ToList();
        }
    }
}
