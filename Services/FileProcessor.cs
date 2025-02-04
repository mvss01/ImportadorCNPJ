namespace ImportadorCNPJ.Services
{
    public class FileProcessor(string fileName, int batchSize = 500000)
    {
        private readonly string _fileName = fileName;
        private readonly string _filePath = $"./Archives/{Path.GetFileNameWithoutExtension(fileName)}";
        private readonly int _batchSize = batchSize;

        public async Task ExecuteFullProcessAsync()
        {
            try
            {
                Console.WriteLine($"\nIniciando processamento do arquivo: {_fileName}");

                // 1. Fazer o download do arquivo
                await DownloadFileAsync();

                // 2. Descompactar o arquivo
                await DecompressFileAsync();

                // 3. Processar os dados do arquivo em lotes
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
            string baseUrl = $"https://arquivos.receitafederal.gov.br/dados/cnpj/dados_abertos_cnpj/2025-01/";
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
                Console.WriteLine("Arquivo não encontrado.");
                return;
            }

            using var fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var streamReader = new StreamReader(fileStream);

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

        private static async Task ProcessBatchAsync(List<string> batch)
        {
            foreach (var line in batch)
            {
                ParseCsvLine(line);
            }

            await Task.CompletedTask;
        }

        private static string[] ParseCsvLine(string line)
        {
            var values = new List<string>();
            var currentValue = string.Empty;
            var insideQuotes = false;

            foreach (var character in line)
            {
                if (character == '"')
                {
                    insideQuotes = !insideQuotes;
                }
                else if (character == ';' && !insideQuotes)
                {
                    values.Add(currentValue.Trim());
                    currentValue = string.Empty;
                }
                else
                {
                    currentValue += character;
                }
            }

            values.Add(currentValue.Trim());

			return [.. values];
		}

		private void DisplayProgress(int batchNumber, int totalLines)
		{
			var totalBatches = (int)Math.Ceiling((double)totalLines / _batchSize);
			var progressPercentage = (double)batchNumber / totalBatches * 100;
			Console.Write($"\rProcessando arquivo em lotes [{batchNumber}/{totalBatches}] - Progresso: {progressPercentage:F2}%");
		}
	}
}
