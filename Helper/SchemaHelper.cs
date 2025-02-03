 namespace ImportadorCNPJ.Helper
 {
    public class SchemaHelper
    {
        // Método para extrair os nomes das tabelas a partir do arquivo SQL
        public static string[] ExtractTableNames(string filePath)
        {
            const string createTableKeyword = "CREATE TABLE ";

            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("Arquivo CreateTables.sql não encontrado.");
                    return [];
                }

                string sqlScript = File.ReadAllText(filePath);
                string[] createTableCommands = sqlScript.Split(";", StringSplitOptions.RemoveEmptyEntries);
                List<string> tableNames = [];

                foreach (string command in createTableCommands)
                {
                    int startIndex = command.IndexOf(createTableKeyword, StringComparison.OrdinalIgnoreCase);

                    if (startIndex >= 0)
                    {
                        startIndex += createTableKeyword.Length;
                        int endIndex = command.IndexOf('(', startIndex);

                        if (endIndex > startIndex)
                        {
                            string tableName = command[startIndex..endIndex].Trim();
                            if (!string.IsNullOrEmpty(tableName))
                            {
                                tableNames.Add(tableName);
                            }
                        }
                    }
                }

                return [.. tableNames];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar o arquivo SQL: {ex.Message}");
                return [];
            }
        }

        // Método auxiliar para encontrar e extrair os nomes das tabelas do arquivo SQL
        public static string[] ExtractSchemas(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("Arquivo CreateTables.sql não encontrado.");
                    return [];
                }

                string sqlScript = File.ReadAllText(filePath);
                return sqlScript.Split(";", StringSplitOptions.RemoveEmptyEntries);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar o arquivo SQL: {ex.Message}");
                return [];
            }
        }

        // Método para obter o comando SQL com base no nome da tabela
        public static string GetCreateTableCommand(string tableName, string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("Arquivo CreateTables.sql não encontrado.");
                    return string.Empty;
                }

                string sqlScript = File.ReadAllText(filePath);
                string[] createTableCommands = sqlScript.Split(";", StringSplitOptions.RemoveEmptyEntries);

                foreach (string command in createTableCommands)
                {
                    if (command.TrimStart().StartsWith("CREATE TABLE", StringComparison.OrdinalIgnoreCase))
                    {
                        int startIndex = command.IndexOf("CREATE TABLE", StringComparison.OrdinalIgnoreCase) + "CREATE TABLE".Length;
                        int endIndex = command.IndexOf('(', startIndex);

                        if (endIndex > startIndex)
                        {
                            string extractedTableName = command[startIndex..endIndex].Trim();
                            if (string.Equals(extractedTableName, tableName, StringComparison.OrdinalIgnoreCase))
                            {
                                return command.Trim() + ";";
                            }
                        }
                    }
                }

                Console.WriteLine($"Tabela {tableName} não encontrada no arquivo SQL.");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar o arquivo SQL: {ex.Message}");
                return string.Empty;
            }
        }


    }

 }
