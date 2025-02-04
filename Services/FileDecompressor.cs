using System.IO.Compression;

namespace ImportadorCNPJ.Services
{
    public class FileDecompressor
    {
        public static async Task DecompressAsync(string fileName)
        {
            string filePath = $"./Archives/{fileName}";
            string outputPath = $"./Archives";
            Directory.CreateDirectory(outputPath);

            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("O arquivo não existe.");
                    return;
                }

                Console.WriteLine($"Descompactando arquivo {fileName}...");

                await Task.Run(() =>
                {
                    using (ZipArchive archive = ZipFile.OpenRead(filePath))
                    {
                        int totalEntries = archive.Entries.Count;
                        int extractedEntries = 0;

                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            string destinationPath = Path.Combine(outputPath, entry.FullName);
                            if (string.IsNullOrEmpty(entry.Name))
                            {
                                Directory.CreateDirectory(destinationPath);
                            }
                            else
                            {
                                string newFileName = Path.GetFileNameWithoutExtension(fileName);
                                string newDestinationPath = Path.Combine(outputPath, newFileName);
                                entry.ExtractToFile(newDestinationPath, true);
                            }

                            extractedEntries++;
                            double progress = (double)extractedEntries / totalEntries * 100;
                            Console.Write($"\rProgresso: {Math.Floor(progress)}%");
                        }
                    }

                    File.Delete(filePath);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao descompactar o arquivo: {ex.Message}");
            }
        }
    }
}
