using Microsoft.Data.SqlClient;
using ImportadorCNPJ.Helper;
namespace ImportadorCNPJ.Helper
{
    public class DatabaseHelper
    {
        // Verifica se um banco de dados existe
        public static bool CheckDatabaseExists(string databaseName, SqlConnection connection)
        {
            string query = "SELECT database_id FROM sys.databases WHERE Name = @DatabaseName";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@DatabaseName", databaseName);
            return command.ExecuteScalar() != null;
        }

        // Cria o banco de dados caso ele não exista
        public static void CreateDatabaseIfNotExists(string databaseName, SqlConnection connection)
        {
            if (!CheckDatabaseExists(databaseName, connection))
            {
                string createDbQuery = $"CREATE DATABASE [{databaseName}]";

                using var command = new SqlCommand(createDbQuery, connection);
                command.ExecuteNonQuery();

                Console.WriteLine($"Banco de dados '{databaseName}' criado com sucesso.");
            }
            else
            {
                Console.WriteLine($"O banco de dados '{databaseName}' já existe.");
            }
        }

        // Verifica se uma tabela existe no banco de dados
        public static bool CheckTableExists(string tableName, SqlConnection connection)
        {
            string query = @"
                SELECT 1
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = @TableName";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@TableName", tableName);
            return command.ExecuteScalar() != null;
        }

        // Cria as tabelas do arquivo CreateTables.sql caso elas não existam
        public static void CreateTablesIfNotExists(SqlConnection connection)
        {
            string[] createTableCommands = SchemaHelper.ExtractSchemas("./Schemas/CreateTables.sql");

            if (createTableCommands.Length == 0)
            {
            Console.WriteLine("Nenhum comando SQL encontrado ou erro ao carregar o arquivo.");
            return;
            }

            string[] tableNames = SchemaHelper.ExtractTableNames("./Schemas/CreateTables.sql");

            foreach (string tableName in tableNames)
            {
            if (!CheckTableExists(tableName, connection))
            {
                string commandText = SchemaHelper.GetCreateTableCommand(tableName, "./Schemas/CreateTables.sql");
                using var command = new SqlCommand(commandText, connection);
                command.ExecuteNonQuery();
                Console.WriteLine($"Tabela '{tableName}' criada com sucesso.");
            }
            else
            {
                Console.WriteLine($"A tabela '{tableName}' já existe no banco de dados '{connection.Database}'.");
            }
            }
        }

    }
}
