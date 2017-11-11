using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using EasyMigrator.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace EasyMigrator.Utility
{
    public class AssemblyUtility : IAssemblyUtility
    {
        public List<T> GetTypeFromAssembly<T>(Assembly targetAssembly)
        {
            var types = targetAssembly.ExportedTypes.Where(t => typeof(T).IsAssignableFrom(t)).ToList();

            if (types.Count == 0)
            {
                throw new ApplicationException($"No classes that implement {typeof(T).Name} found in the target assembly.");
            }

            List<T> returnValue = new List<T>();
            foreach (var migrationType in types)
            {
                returnValue.Add((T)
                    ActivatorUtilities.GetServiceOrCreateInstance(
                        ServiceProviderManager.ServiceProvider,
                        migrationType));
            }

            return returnValue;
        }

        public T GetSingleTypeFromAssembly<T>(Assembly assembly)
        {
            var types = assembly.ExportedTypes.Where(t => typeof(T).IsAssignableFrom(t)).ToList();

            if (types.Count == 0)
            {
                throw new ApplicationException($"No classes that implement {typeof(T).Name} found in the target assembly.");
            }

            if (types.Count > 1)
            {
                throw new ApplicationException($"The target assembly contains more than one classes that implement {typeof(T).Name}.");
            }

            return (T)
                ActivatorUtilities.GetServiceOrCreateInstance(
                    ServiceProviderManager.ServiceProvider,
                    types.First());
        }
    }
}
