using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using EasyMigrator.Abstractions;
using EasyMigrator.Interfaces;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;

namespace EasyMigrator.Commands
{
    public class DataGeneratorCommand : IDataGeneratorCommand
    {
        private Assembly _targetAssembly;
        private readonly ILogger _logger;
        private readonly IAssemblyUtility _assemblyUtility;
        private readonly ISqlCommandUtility _commandUtility;

        public DataGeneratorCommand(
            ILogger<MigrationCommand> logger,
            IAssemblyUtility assemblyUtility,
            ISqlCommandUtility commandUtility)
        {
            _logger = logger;
            _assemblyUtility = assemblyUtility;
            _commandUtility = commandUtility;
        }

        public void SetTargetAssembly(Assembly targetAssembly)
        {
            _targetAssembly = targetAssembly;
        }

        public void PerformDataGenerationOperation(string generationName)
        {
            var scriptDataGenActions = _assemblyUtility.GetTypeFromAssembly<IDataGenerator>(_targetAssembly);
            var programaticDataGenActions =
                _assemblyUtility.GetTypeFromAssembly<IProgrammaticDataGenerator>(_targetAssembly);

            SanityCheckNaming(scriptDataGenActions, programaticDataGenActions);

            var scriptSeach = scriptDataGenActions.SingleOrDefault(a => String.CompareOrdinal(a.Name.ToLower(), generationName.ToLower()) == 0);
            if (scriptSeach == default(IDataGenerator))
            {
                var programmaticSeach = programaticDataGenActions.SingleOrDefault(a => String.CompareOrdinal(a.Name.ToLower(),generationName.ToLower()) == 0);
                if (programmaticSeach == default(IProgrammaticDataGenerator))
                {
                    throw new ApplicationException($"No datagen action found with the name '{generationName}'.");
                }

                programmaticSeach.GenerationRoutine();
            }
            else
            {
                _commandUtility.RunEmbeddedResourceList(scriptSeach.DataGenerationScript, _targetAssembly);
            }
        }

        private void SanityCheckNaming(
            List<IDataGenerator> scriptDataGenActions, 
            List<IProgrammaticDataGenerator> programaticDataGenActions
            )
        {
            if (scriptDataGenActions.Count != scriptDataGenActions.Select(a => a.Name).Distinct().Count())
            {
                throw new ApplicationException("There are multiple implmentations of IDataGenerator with the same name value.");
            }

            if (programaticDataGenActions.Count != programaticDataGenActions.Select(a => a.Name).Distinct().Count())
            {
                throw new ApplicationException("There are multiple implmentations of IProgrammaticDataGenerator with the same name value.");
            }
            
            if (programaticDataGenActions.Select(a => a.Name).Intersect(scriptDataGenActions.Select(a => a.Name)).Any())
            {
                throw new ApplicationException("There are implmentations of IDataGenerator and IProgrammaticDataGenerator with the same name value.");
            }
        }
    }
}
