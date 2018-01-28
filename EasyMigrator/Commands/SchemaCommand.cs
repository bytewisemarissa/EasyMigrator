using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using EasyMigrator.Abstractions;
using EasyMigrator.Interfaces;
using EasyMigrator.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace EasyMigrator.Commands
{
    public class SchemaCommand : ISchemaCommand
    {
        private Assembly _targetAssembly;
        private readonly ILogger<MigrationCommand> _logger;
        private readonly ISqlCommandUtility _sqlCommandUtility;
        private readonly IAssemblyUtility _assemblyUtility;
        
        public SchemaCommand(
            ILogger<MigrationCommand> logger, 
            ISqlCommandUtility sqlCommandUtility,
            IAssemblyUtility assemblyUtility)
        {
            _logger = logger;
            _sqlCommandUtility = sqlCommandUtility;
            _assemblyUtility = assemblyUtility;
        }

        public void SetTargetAssembly(Assembly targetAssembly)
        {
            _targetAssembly = targetAssembly;
        }

        public void PerformCreateOperation()
        {
            var schemaOperation = _assemblyUtility.GetSingleTypeFromAssembly<ISchemaOperation>(_targetAssembly);

            _sqlCommandUtility.RunEmbeddedResourceList(schemaOperation.CreateResourseList, _targetAssembly);
        }

        public void PerformDestroyOperation()
        {
            var schemaOperation = _assemblyUtility.GetSingleTypeFromAssembly<ISchemaOperation>(_targetAssembly);

            _sqlCommandUtility.RunEmbeddedResourceList(schemaOperation.DestroyResourceList, _targetAssembly);
        }

        public void PerformRebuildOperation()
        {
            var schemaOperation = _assemblyUtility.GetSingleTypeFromAssembly<ISchemaOperation>(_targetAssembly);

            _sqlCommandUtility.RunEmbeddedResourceList(schemaOperation.DestroyResourceList, _targetAssembly);
            _sqlCommandUtility.RunEmbeddedResourceList(schemaOperation.CreateResourseList, _targetAssembly);
        }
    }
}
