using System.Collections.Generic;
using System.Reflection;

namespace EasyMigrator.Interfaces
{
    public interface IAssemblyUtility
    {
        List<T> GetTypeFromAssembly<T>(Assembly targetAssembly);
        T GetSingleTypeFromAssembly<T>(Assembly assembly);
    }
}