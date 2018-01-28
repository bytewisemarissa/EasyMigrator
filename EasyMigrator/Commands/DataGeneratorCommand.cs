using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
            List<IDataGenerator> scriptDataGenActions;
            try
            {
                scriptDataGenActions = _assemblyUtility.GetTypeFromAssembly<IDataGenerator>(_targetAssembly);
            }
            catch
            {
                scriptDataGenActions = new List<IDataGenerator>();
            }
            
            List<IProgrammaticDataGenerator> programaticDataGenActions;
            try
            {
                programaticDataGenActions = _assemblyUtility.GetTypeFromAssembly<IProgrammaticDataGenerator>(_targetAssembly);
            }
            catch
            {
                programaticDataGenActions = new List<IProgrammaticDataGenerator>();
            }
            
            List<IDataGeneratorGroup> dataGeneratorGroups;
            try
            {
                dataGeneratorGroups = _assemblyUtility.GetTypeFromAssembly<IDataGeneratorGroup>(_targetAssembly);
            }
            catch
            {
                dataGeneratorGroups = new List<IDataGeneratorGroup>();
            }
            
            SanityCheckNaming(scriptDataGenActions, programaticDataGenActions, dataGeneratorGroups, generationName);
            
            var scriptSeach = scriptDataGenActions.SingleOrDefault(sdg =>
                String.Equals(sdg.Name, generationName, StringComparison.CurrentCultureIgnoreCase));
            var programmaticSeach = programaticDataGenActions.SingleOrDefault(pdg =>
                String.Equals(pdg.Name, generationName, StringComparison.CurrentCultureIgnoreCase));
            var groupSearch = dataGeneratorGroups.SingleOrDefault(dgg =>
                String.Equals(dgg.Name, generationName, StringComparison.CurrentCultureIgnoreCase));
            
            if (scriptSeach != default(IDataGenerator))
            {
                _commandUtility.RunEmbeddedResourceList(scriptSeach.DataGenerationScripts, _targetAssembly);
            }
            
            if (programmaticSeach != default(IProgrammaticDataGenerator))
            {
                _logger.LogInformation($"Starting generation function, \"{programmaticSeach.Name}\".");

                programmaticSeach.GenerationRoutine(_logger);

                _logger.LogInformation($"Finished generation function, \"{programmaticSeach.Name}\".");
            }
            
            if (groupSearch != default(IDataGeneratorGroup))
            {
                foreach (var actionName in groupSearch.DataGenerationActionNames)
                {
                    var scriptSeachGroup = scriptDataGenActions.SingleOrDefault(sdg =>
                        String.Equals(sdg.Name, actionName, StringComparison.CurrentCultureIgnoreCase));
                    
                    var programmaticSeachGroup = programaticDataGenActions.SingleOrDefault(pdg =>
                        String.Equals(pdg.Name, actionName, StringComparison.CurrentCultureIgnoreCase));
                    
                    if (scriptSeachGroup == default(IDataGenerator) &&
                        programmaticSeachGroup == default(IProgrammaticDataGenerator))
                    {
                        throw new ApplicationException(
                            $"The data generator group '{groupSearch.Name}' contains the action name '{actionName}' which can not be found in the targe assembly.");
                    }
                    
                    if (scriptSeachGroup != default(IDataGenerator))
                    {
                        _commandUtility.RunEmbeddedResourceList(scriptSeachGroup.DataGenerationScripts, _targetAssembly);
                    }
                    
                    if (programmaticSeachGroup != default(IProgrammaticDataGenerator))
                    {
                        _logger.LogInformation($"Starting generation function, \"{programmaticSeachGroup.Name}\".");

                        programmaticSeachGroup.GenerationRoutine(_logger);

                        _logger.LogInformation($"Finished generation function, \"{programmaticSeachGroup.Name}\".");
                    }
                }
            }
        }

        public void DisplayDataGenNames()
        {
            var scriptDataGenActions = _assemblyUtility.GetTypeFromAssembly<IDataGenerator>(_targetAssembly);
            var programaticDataGenActions = _assemblyUtility.GetTypeFromAssembly<IProgrammaticDataGenerator>(_targetAssembly);
            var groupActions = _assemblyUtility.GetTypeFromAssembly<IDataGeneratorGroup>(_targetAssembly);

            Console.WriteLine();
            Console.WriteLine("Script Actions:");
            foreach (var scriptAction in scriptDataGenActions)
            {
                Console.WriteLine(scriptAction.Name);
            }

            Console.WriteLine();
            Console.WriteLine("Programmatic Actions:");
            foreach (var programaticAction in programaticDataGenActions)
            {
                Console.WriteLine(programaticAction.Name);
            }

            Console.WriteLine();
            Console.WriteLine("Data Generator Group:");
            foreach (var groupName in groupActions)
            {
                Console.WriteLine(groupName.Name);
            }
        }

        private void SanityCheckNaming(
            List<IDataGenerator> scriptDataGenActions, 
            List<IProgrammaticDataGenerator> programaticDataGenActions,
            List<IDataGeneratorGroup> dataGeneratorGroups,
            string generationName)
        {
            if (scriptDataGenActions.Count != scriptDataGenActions.Select(sdg => sdg.Name).Distinct().Count())
            {
                throw new ApplicationException("There are multiple implmentations of IDataGenerator with the same name value.");
            }

            if (programaticDataGenActions.Count != programaticDataGenActions.Select(pdg => pdg.Name).Distinct().Count())
            {
                throw new ApplicationException("There are multiple implmentations of IProgrammaticDataGenerator with the same name value.");
            }

            if (dataGeneratorGroups.Count != dataGeneratorGroups.Select(dgg => dgg.Name).Distinct().Count())
            {
                throw new ApplicationException("There are multiple implmentations of IDataGeneratorGroup with the same name value.");
            }
            
            if (programaticDataGenActions.Select(pdg => pdg.Name)
                .Intersect(scriptDataGenActions.Select(sdg => sdg.Name))
                .Intersect(dataGeneratorGroups.Select(dgg => dgg.Name)).Any())
            {
                throw new ApplicationException("There are implmentations of IDataGenerator or IProgrammaticDataGenerator or IDataGeneratorGroup with the same name value.");
            }

            if (scriptDataGenActions.All(sdg => sdg.Name != generationName) &&
                programaticDataGenActions.All(pdg => pdg.Name != generationName) &&
                dataGeneratorGroups.All(dgg => dgg.Name != generationName))
            {
                throw new ApplicationException($"No datagen action found with the name '{generationName}'.");
            }
        }
    }
}
