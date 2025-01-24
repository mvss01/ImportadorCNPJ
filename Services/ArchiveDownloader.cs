namespace ImportadorCNPJ.Services
{
    public class ArchiveDownloader
    {
        public static async Task DownloadFileAsync(string baseUrl, string fileName)
        {
            try
            {
                var filePath = Path.Combine(Environment.CurrentDirectory, "Archives", fileName);
                using HttpClient client = new();
                Console.WriteLine($"Baixando o arquivo {fileName}...");
                client.Timeout = Timeout.InfiniteTimeSpan;

                using var response = await client.GetAsync($"{baseUrl}{fileName}", HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var canReportProgress = totalBytes != -1;

                using var contentStream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                var buffer = new byte[8192];
                long totalRead = 0;
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer)) != 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                    totalRead += bytesRead;

                    if (canReportProgress)
                    {
                        Console.Write($"\rProgresso: {totalRead * 100 / totalBytes}%");
                    }
                }

                Console.WriteLine("\nDownload concluído.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao baixar o arquivo {fileName}: {ex.Message}");
            }
        }
    }
}
