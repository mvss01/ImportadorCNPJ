using System.Text.RegularExpressions;

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
    }
}
