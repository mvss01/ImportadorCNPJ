namespace ImportadorCNPJ.Services
{
    public class FileProcessor(string fileName, int batchSize = 500000)
    {
        private readonly string? _filePath = $"./Archives/{Path.GetFileNameWithoutExtension(fileName)}";
        private readonly int _batchSize = batchSize;

        public async Task ProcessFileInBatchesAsync()
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

            Console.WriteLine($"\nProcessando arquivo {fileName}.csv...");
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

            Console.WriteLine("\nProcessamento finalizado!");
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
