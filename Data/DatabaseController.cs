using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;

namespace ImportadorCNPJ.Data
{
    public class DatabaseController(SqlConnection connection)
    {
        private readonly SqlConnection _connection = connection;

        public async Task InsertBatchAsync(string tableName, string[] columns, List<object[]> batch)
        {
            if (batch == null || batch.Count == 0)
                throw new ArgumentException("O lote de dados está vazio ou nulo.");

            try
            {
                // Monta a query SQL dinamicamente
                var columnNames = string.Join(", ", columns);
                var valuePlaceholders = string.Join(", ", columns.Select((_, index) => $"@col{index}"));

                var query = $"INSERT INTO {tableName} ({columnNames}) VALUES ({valuePlaceholders})";

                foreach (var row in batch)
                {
                    if (row.Length != columns.Length)
                        throw new ArgumentException("O número de valores não corresponde ao número de colunas.");

                    using var command = new SqlCommand(query, _connection);

                    for (int i = 0; i < columns.Length; i++)
                    {
                        var value = row[i];

                        // Tratamento genérico para conversões
                        if (value is string strValue && decimal.TryParse(strValue.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decimalValue))
                        {
                            command.Parameters.AddWithValue($"@col{i}", decimalValue);
                        }
                        else
                        {
                            command.Parameters.AddWithValue($"@col{i}", value ?? DBNull.Value);
                        }
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
