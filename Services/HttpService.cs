namespace ImportadorCNPJ.Services
{
    public class HttpService
    {
        private readonly HttpClient _httpClient;

        public HttpService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);
        }

        public async Task<string> GetHtmlAsync(string url)
        {
            Console.WriteLine("Requisitando corpo HTML...");
            try
            {
                return await _httpClient.GetStringAsync(url);
            }
            catch (HttpRequestException e)
            {
                return $"Erro ao fazer a requisição: {e.Message}";
            }
        }
    }
}
