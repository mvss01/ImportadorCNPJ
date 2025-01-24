using System;
using System.Net.Http;
using System.Threading.Tasks;
using ImportadorCNPJ.Services;

class Program
{
    static async Task Main(string[] args)
    {
        var httpService = new HttpService();

        string url = "https://arquivos.receitafederal.gov.br/dados/cnpj/dados_abertos_cnpj/2025-01/";

        string html = await httpService.GetHtmlAsync(url);

        Console.WriteLine("Resposta HTML:");
        Console.WriteLine(html);

    }
}
