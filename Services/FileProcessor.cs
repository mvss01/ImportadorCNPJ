using System.Text;
using System.Text.RegularExpressions;
using ImportadorCNPJ.Data;
using ImportadorCNPJ.Infra;

namespace ImportadorCNPJ.Services
{
    public partial class FileProcessor(string fileName, int batchSize = 100000)
    {
        private readonly string _fileName = fileName;
        private readonly string _filePath = Path.Combine("./Archives", Path.GetFileNameWithoutExtension(fileName));
        private readonly int _batchSize = batchSize;

        public async Task ExecuteFullProcessAsync()
        {
            try
            {
                Console.WriteLine($"\nIniciando processamento do arquivo: {_fileName}");

                await DownloadFileAsync();
                await DecompressFileAsync();
                await ProcessFileInBatchesAsync();

                Console.WriteLine($"\nProcessamento do arquivo {_fileName} concluído com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar o arquivo {_fileName}: {ex.Message}");
            }
        }

        private async Task DownloadFileAsync()
        {
            string baseUrl = "https://arquivos.receitafederal.gov.br/dados/cnpj/dados_abertos_cnpj/2025-01/";
            await ArchiveDownloader.DownloadFileAsync(baseUrl, _fileName);
        }

        private async Task DecompressFileAsync()
        {
            await FileDecompressor.DecompressAsync(_fileName);
        }

        private async Task ProcessFileInBatchesAsync()
        {
            if (!File.Exists(_filePath))
            {
                Console.WriteLine("Arquivo não encontrado após descompactação.");
                return;
            }

            using var fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var streamReader = new StreamReader(fileStream, Encoding.GetEncoding("Windows-1252"));

            string? line;
            var batch = new List<string>();
            var totalLines = File.ReadLines(_filePath).Count();
            var processedLines = 0;
            var batchNumber = 0;

            Console.WriteLine($"\nProcessando arquivo {_fileName}.csv...");
            DisplayProgress(batchNumber, totalLines);

            while ((line = await streamReader.ReadLineAsync()) != null)
            {
                batch.Add(line);
                processedLines++;

                if (batch.Count >= _batchSize)
                {
                    batchNumber++;
                    await ProcessBatchAsync(batch);
                    batch.Clear();
                    DisplayProgress(batchNumber, totalLines);
                }
            }

            if (batch.Count > 0)
            {
                batchNumber++;
                await ProcessBatchAsync(batch);
                DisplayProgress(batchNumber, totalLines);
            }
        }

        private async Task ProcessBatchAsync(List<string> batch)
        {
            string tableName = MyRegex1().Replace(Path.GetFileNameWithoutExtension(_fileName).Split('_')[0], "").ToUpper();
            string[] columns = DetermineTableColumns(tableName);
            var parsedBatch = batch.Select(ParseCsvLine).Select(line => line.Cast<object>().ToArray()).ToList();

            using var db = new Database("CNPJ");
            using var connection = db.GetConnection();
            var databaseController = new DatabaseController(connection);

            await databaseController.InsertBatchAsync(tableName, columns, parsedBatch);
        }

        private static readonly Dictionary<string, string[]> _tableSchemas = [];

        public static void LoadTableSchemas()
        {
            string schemaFilePath = "./Schemas/CreateTables.sql";
            if (!File.Exists(schemaFilePath))
                throw new FileNotFoundException("Arquivo de schema não encontrado.", schemaFilePath);

            var tableDefinitions = File.ReadAllText(schemaFilePath)
                .Split("CREATE TABLE", StringSplitOptions.RemoveEmptyEntries)
                .Select(def => def.Trim());

            foreach (var definition in tableDefinitions)
            {
                var lines = definition.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 0)
                {
                    var tableName = ExtractTableName(lines[0]);
                    var columns = lines.Skip(1)
                        .Where(line => IsColumnDefinition(line)) // Filtra apenas linhas que definem colunas
                        .Select(ExtractColumnName)
                        .Where(column => !string.IsNullOrEmpty(column))
                        .ToArray();

                    _tableSchemas[tableName] = columns;
                }
            }
        }

        private static bool IsColumnDefinition(string line)
        {
            // Verifica se a linha contém uma definição de coluna válida
            line = line.Trim();
            return !line.StartsWith("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) &&
                !line.StartsWith("PRIMARY KEY", StringComparison.OrdinalIgnoreCase) &&
                !line.StartsWith(")", StringComparison.OrdinalIgnoreCase); // Ignora fechamento de tabela
        }

        private static string ExtractTableName(string tableNameLine)
        {
            // Normaliza o nome da tabela removendo parênteses e convertendo para maiúsculas
            return tableNameLine.Split('(', StringSplitOptions.RemoveEmptyEntries)[0]
                                .Trim()
                                .ToUpper();
        }

        private static string ExtractColumnName(string columnDefinition)
        {
            columnDefinition = columnDefinition.Trim();

            // Remove palavras-chave como PRIMARY KEY, NOT NULL, etc.
            var keywords = new[] { "PRIMARY KEY", "NOT NULL", "NULL", "UNIQUE", "AUTO_INCREMENT" };
            foreach (var keyword in keywords)
            {
                var keywordIndex = columnDefinition.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
                if (keywordIndex > 0)
                {
                    columnDefinition = columnDefinition[..keywordIndex].Trim();
                }
            }

            // Extrai o nome da coluna baseado no formato comum
            var parts = columnDefinition.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts[0] : string.Empty;
        }

        public static string[] DetermineTableColumns(string tableName)
        {
            // Remove números ao final do nome da tabela para normalização
            if (_tableSchemas.Count == 0)
                LoadTableSchemas();

            return _tableSchemas.TryGetValue(tableName, out var columns)
                ? columns
                : throw new InvalidOperationException($"Tabela não reconhecida: {tableName}");
        }

        private static string[] ParseCsvLine(string line)
        {
            var values = new List<string>();
            var currentValue = new System.Text.StringBuilder();
            var insideQuotes = false;

            foreach (var character in line)
            {
                if (character == '"')
                {
                    insideQuotes = !insideQuotes;
                }
                else if (character == ';' && !insideQuotes)
                {
                    values.Add(currentValue.ToString().Trim());
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(character);
                }
            }
            values.Add(currentValue.ToString().Trim());
            return [.. values];
        }

        private void DisplayProgress(int batchNumber, int totalLines)
        {
            var totalBatches = (int)Math.Ceiling((double)totalLines / _batchSize);
            var progressPercentage = (double)batchNumber / totalBatches * 100;
            Console.Write($"\rProcessando arquivo em lotes [{batchNumber}/{totalBatches}] - Progresso: {progressPercentage:F2}%");
        }

        [System.Text.RegularExpressions.GeneratedRegex(@"\d+$")]
        private static partial System.Text.RegularExpressions.Regex MyRegex();
        [GeneratedRegex(@"\d")]
        private static partial Regex MyRegex1();
    }
}
