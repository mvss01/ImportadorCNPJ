using System.Text.RegularExpressions;
using ImportadorCNPJ.Helper;

namespace ImportadorCNPJ.Services
{
    public class HtmlFileNameExtractor
    {
        public static List<string> ExtractFileNames(string html)
        {
            Console.WriteLine("Extraindo nomes dos arquivos do HTML...");
            var fileNames = new List<string>();

            if (string.IsNullOrWhiteSpace(html))
                return fileNames;

            string pattern = @"<a\s+[^>]*href\s*=\s*""([^""]+\.(zip))""[^>]*>";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);

            var matches = regex.Matches(html);

            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    fileNames.Add(match.Groups[1].Value);
                }
            }

            return fileNames;
        }

        public static async Task<List<List<string>>> GroupFiles()
        {
            var httpService = new HttpService();

            string year = DateTime.Now.Year.ToString();
            string month = DateTime.Now.Month.ToString("D2");
            // string baseUrl = $"https://arquivos.receitafederal.gov.br/dados/cnpj/dados_abertos_cnpj/{year}-{month}/";
            string baseUrl = $"https://arquivos.receitafederal.gov.br/dados/cnpj/dados_abertos_cnpj/{year}-01/";

            string html = await httpService.GetHtmlAsync(baseUrl);

            var fileNames = ExtractFileNames(html);

            string[] tableNames = SchemaHelper.ExtractTableNames("./Schemas/CreateTables.sql");

            var groupedFilesList = new List<List<string>>();

            foreach (string tableName in tableNames)
            {
                var groupedFiles = fileNames
                    .Where(fileName => fileName.StartsWith(tableName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (groupedFiles.Count == 0)
                {
                    Console.WriteLine($"Nenhum arquivo encontrado para a tabela {tableName}");
                }
                else
                {
                    groupedFilesList.Add(groupedFiles);
                }
            }

            return groupedFilesList;
        }
    }
}
