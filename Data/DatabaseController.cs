using Microsoft.Data.SqlClient;


namespace ImportadorCNPJ.Data
{
    public class DatabaseController(SqlConnection connection)
    {
        private readonly SqlConnection _connection = connection;

        public async Task InsertBatchAsync(string tableName, string[] columns, List<string[]> batch)
        {
            if (batch.Count == 0) return;

            try
            {
                // Monta o comando SQL para inserção em massa
                var columnNames = string.Join(", ", columns);
                var valuePlaceholders = string.Join(", ", columns.Select((_, index) => $"@col{index}"));

                var query = $"INSERT INTO {tableName} ({columnNames}) VALUES ({valuePlaceholders})";

                foreach (var row in batch)
                {
                    if (row.Length != columns.Length)
                    {
                        Console.WriteLine($"Erro: Número de valores ({row.Length}) não corresponde ao número de colunas ({columns.Length}) na tabela {tableName}.");
                        continue; // Ignora linhas inválidas
                    }

                    using var command = new SqlCommand(query, _connection);

                    for (int i = 0; i < columns.Length; i++)
                    {
                        command.Parameters.AddWithValue($"@col{i}", row[i] != null ? row[i] : DBNull.Value);
                    }

                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao inserir dados na tabela {tableName}: {ex.Message}");
            }
        }

    }
}
