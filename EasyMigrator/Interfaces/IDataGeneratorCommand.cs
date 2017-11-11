using System.Reflection;

namespace EasyMigrator.Interfaces
{
    public interface IDataGeneratorCommand
    {
        void SetTargetAssembly(Assembly targetAssembly);
        void PerformDataGenerationOperation(string generationName);
    }
}