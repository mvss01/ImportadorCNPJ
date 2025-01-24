using ImportadorCNPJ.Services;

class Program
{
    static async Task Main(string[] args)
    {
        var httpService = new HttpService();

        string url = "https://arquivos.receitafederal.gov.br/dados/cnpj/dados_abertos_cnpj/2025-01/";

        string html = await httpService.GetHtmlAsync(url);

        var fileNames = HtmlFileNameExtractor.ExtractFileNames(html);

        foreach (var fileName in fileNames)
        {
            Console.WriteLine(fileName);
        }

    }
}
