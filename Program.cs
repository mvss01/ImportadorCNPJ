using ImportadorCNPJ.Infra;
using ImportadorCNPJ.Helper;
using ImportadorCNPJ.Services;

class Program
{
    static async Task Main(string[] args)
    {
        string databaseName = "CNPJ";

        // Conexão inicial com o SQL Server
        using (Database db = new())
        {
            using var masterConnection = db.GetConnection(); // Conecta ao banco 'master'

            // 1. Cria o banco de dados 'CNPJ' se ele não existir
            DatabaseHelper.CreateDatabaseIfNotExists(databaseName, masterConnection);
        }

        // 2. Conecta ao banco de dados 'CNPJ'
        using (Database db = new(databaseName)) // Passa o nome do banco 'CNPJ'
        {
            using var cnpjConnection = db.GetConnection();

            // 3. Cria as tabelas no banco de dados 'CNPJ'
            DatabaseHelper.CreateTablesIfNotExists(cnpjConnection);
        }

        // 4. Extrair nome dos arquivos e agrupar pelo nome das tabelas
        List<List<string>> groupedFiles = await HtmlFileNameExtractor.GroupFiles();

        // 5. Iterar sobre os grupos de arquivos e processá-los
        foreach (var fileGroup in groupedFiles)
        {
            foreach (var fileName in fileGroup)
            {
                var fileProcessor = new FileProcessor(fileName);
                await fileProcessor.ExecuteFullProcessAsync();
            }
        }
    }
}
