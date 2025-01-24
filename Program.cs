using ImportadorCNPJ.Services;

class Program
{
    static async Task Main(string[] args)
    {
        var httpService = new HttpService();

        string year = DateTime.Now.Year.ToString();
        string month = DateTime.Now.Month.ToString("D2");
        string baseUrl = $"https://arquivos.receitafederal.gov.br/dados/cnpj/dados_abertos_cnpj/{year}-{month}/";

        string html = await httpService.GetHtmlAsync(baseUrl);

        var fileNames = HtmlFileNameExtractor.ExtractFileNames(html);

        foreach (var fileName in fileNames)
        {
            Console.WriteLine(fileName);
        }

    }
}
