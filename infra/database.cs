using Microsoft.Data.SqlClient;

namespace ImportadorCNPJ.Infra
{
    public class Database : IDisposable
    {
        private readonly string _connectionString;
        private readonly Lazy<SqlConnection> _connection;
        private bool _disposed = false;

        public Database(string databaseName = "master") // Banco padrão é 'master'
        {
            _connectionString = $"Server=localhost,1433;Database={databaseName};User Id=sa;Password=Senh@123;TrustServerCertificate=True;";
            _connection = new Lazy<SqlConnection>(() =>
            {
                var conn = new SqlConnection(_connectionString);
                conn.Open();
                Console.WriteLine($"Conexão com o SQL Server estabelecida com sucesso no banco '{databaseName}'!");
                return conn;
            });
        }

        public SqlConnection GetConnection()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Database), "A conexão já foi encerrada.");

            return _connection.Value;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_connection.IsValueCreated)
                {
                    _connection.Value.Close();
                    _connection.Value.Dispose();
                    Console.WriteLine("Conexão com o SQL Server encerrada corretamente.");
                }
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}
