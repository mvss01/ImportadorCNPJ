using System.Text;
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
            string tableName = Path.GetFileNameWithoutExtension(_fileName).Split('_')[0].ToUpper();
            string[] columns = DetermineTableColumns(tableName);
            var parsedBatch = batch.Select(ParseCsvLine).ToList();

            using var db = new Database("CNPJ");
            using var connection = db.GetConnection();
            var databaseController = new DatabaseController(connection);

            await databaseController.InsertBatchAsync(tableName, columns, parsedBatch);
        }

        private static readonly Dictionary<string, string[]> _tableSchemas = new();

        private static void LoadTableSchemas()
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
                        .Where(line => line.Contains('(') || line.Contains('[') || line.Contains('"'))
                        .Select(ExtractColumnName)
                        .Where(column => !string.IsNullOrEmpty(column))
                        .ToArray();

                    _tableSchemas[tableName] = columns;
                }
            }
        }

        private static string ExtractTableName(string tableNameLine)
        {
            return tableNameLine.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0].Trim('(', ')').ToUpper();
        }

        private static string ExtractColumnName(string columnDefinition)
        {
            columnDefinition = columnDefinition.Trim();

            if (columnDefinition.Contains('[') && columnDefinition.Contains(']'))
            {
                return columnDefinition[(columnDefinition.IndexOf('[') + 1)..columnDefinition.IndexOf(']')];
            }
            if (columnDefinition.Contains('"'))
            {
                return columnDefinition[(columnDefinition.IndexOf('"') + 1)..columnDefinition.LastIndexOf('"')];
            }
            var parts = columnDefinition.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts[0] : string.Empty;
        }

        private static string[] DetermineTableColumns(string tableName)
        {
            // Remover números ao final do nome da tabela
            string normalizedTableName = NormalizeTableName(tableName);

            if (_tableSchemas.Count == 0)
                LoadTableSchemas();

            return _tableSchemas.TryGetValue(normalizedTableName, out var columns)
                ? columns
                : throw new InvalidOperationException($"Tabela não reconhecida: {normalizedTableName}");
        }

        private static string NormalizeTableName(string tableName)
        {
            // Usar Regex para remover números no final do nome da tabela
            return MyRegex().Replace(tableName, "");
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
    }
}
