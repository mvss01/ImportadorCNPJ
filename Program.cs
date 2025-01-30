using ImportadorCNPJ.Infra;
using ImportadorCNPJ.Services;
using ImportadorCNPJ.Helper;

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

        // 4. Lógica para baixar e processar os arquivos
        var httpService = new HttpService();

        string year = DateTime.Now.Year.ToString();
        string month = DateTime.Now.Month.ToString("D2");
        string baseUrl = $"https://arquivos.receitafederal.gov.br/dados/cnpj/dados_abertos_cnpj/{year}-{month}/";

        string html = await httpService.GetHtmlAsync(baseUrl);
        Console.WriteLine(html);
        var fileNames = HtmlFileNameExtractor.ExtractFileNames(html);

        await ArchiveDownloader.DownloadFileAsync(baseUrl, fileNames[36]);

        await FileDecompressor.DecompressAsync(fileNames[36]);

        var processarDados = new FileProcessor(fileNames[36]);

        await processarDados.ProcessFileInBatchesAsync();
    }
}
