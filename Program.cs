using ImportadorCNPJ.Infra;
using ImportadorCNPJ.Services;

class Program
{
    static async Task Main(string[] args)
    {
        Database db = new();
        using (var connection = db.GetConnection())
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1 + 1";
            var result = await command.ExecuteScalarAsync();
            Console.WriteLine($"Result of 1 + 1 is: {result}");
        }

        var httpService = new HttpService();

        string year = DateTime.Now.Year.ToString();
        string month = DateTime.Now.Month.ToString("D2");
        string baseUrl = $"https://arquivos.receitafederal.gov.br/dados/cnpj/dados_abertos_cnpj/{year}-{month}/";

        string html = await httpService.GetHtmlAsync(baseUrl);

        var fileNames = HtmlFileNameExtractor.ExtractFileNames(html);

        await ArchiveDownloader.DownloadFileAsync(baseUrl, fileNames[36]);
//36
        await FileDecompressor.DecompressAsync(fileNames[36]);

        var processarDados = new FileProcessor(fileNames[36]);

        await processarDados.ProcessFileInBatchesAsync();

    }
}
