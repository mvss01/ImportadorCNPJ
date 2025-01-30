using Microsoft.Data.SqlClient;

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
            // Caminho para o arquivo CreateTables.sql
            string filePath = "./Schemas/CreateTables.sql";

            if (!File.Exists(filePath))
            {
                Console.WriteLine("Arquivo CreateTables.sql não encontrado.");
                return;
            }

            // Lê o conteúdo do arquivo SQL
            string sqlScript = File.ReadAllText(filePath);

            // Divide o script em comandos individuais (baseado no ponto e vírgula)
            string[] tableCommands = sqlScript.Split(";", StringSplitOptions.RemoveEmptyEntries);

            foreach (string commandText in tableCommands)
            {
                // Extração do nome da tabela
                string? tableName = ExtractTableName(commandText);

                if (!string.IsNullOrWhiteSpace(tableName))
                {
                    if (!CheckTableExists(tableName, connection))
                    {
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

        // Método auxiliar para extrair o nome da tabela a partir do comando SQL
        private static string? ExtractTableName(string createTableCommand)
        {
            const string createTableKeyword = "CREATE TABLE ";

            int startIndex = createTableCommand.IndexOf(createTableKeyword, StringComparison.OrdinalIgnoreCase);

            if (startIndex >= 0)
            {
                startIndex += createTableKeyword.Length;
                int endIndex = createTableCommand.IndexOf('(', startIndex);

                if (endIndex > startIndex)
                {
                    return createTableCommand[startIndex..endIndex].Trim();
                }
            }

            return null;
        }
    }
}
