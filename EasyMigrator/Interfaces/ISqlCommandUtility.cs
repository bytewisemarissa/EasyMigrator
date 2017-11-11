using System.Collections.Generic;
using System.Reflection;
using MySql.Data.MySqlClient;

namespace EasyMigrator.Interfaces
{
    public interface ISqlCommandUtility
    {
        bool TestIfLogTableExsists();
        void CreateLogTable();

        void RunEmbeddedResourceList(
            List<string> resourcesToRun,
            Assembly targetAssembly);
    }
}