using MySql.Data.MySqlClient;

namespace EasyMigrator.Interfaces
{
    public interface IDatabaseConnectionFactory
    {
        MySqlConnection GetDatabaseConnection();
    }
}