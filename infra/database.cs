using Microsoft.Data.SqlClient;

namespace ImportadorCNPJ.Infra
{
    public class Database
    {
        private readonly string _connectionString;

        public Database()
        {
            _connectionString = "Server=localhost,1433;Database=master;User Id=sa;Password=Senh@123;TrustServerCertificate=True;";
        }

        public SqlConnection GetConnection()
        {
            try
            {
                var connection = new SqlConnection(_connectionString);
                connection.Open();
                Console.WriteLine("Conexão com o SQL Server estabelecida com sucesso!");
                return connection;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao conectar ao SQL Server: {ex.Message}");
                throw;
            }
        }
    }
}
