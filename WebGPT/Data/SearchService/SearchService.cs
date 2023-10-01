using System.Text.RegularExpressions;

namespace WebGPT.Data.SearchService
{
    public class SearchService : ISearchService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<SearchService> logger;

        public SearchService(IHttpClientFactory httpClientFactory, ILogger<SearchService> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
        }

        public async Task<string> GetPageAsync(string? url)
        {
            logger.LogInformation($"Search: get page with URL {url}");

            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }

            using HttpClient client = httpClientFactory.CreateClient();
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string pageContent = await response.Content.ReadAsStringAsync();

                string bodyContent = ExtractBodyContent(pageContent);
                return bodyContent;
            }
            catch (HttpRequestException exception)
            {
                logger.LogError(exception, $"Search: get page with URL {url} failed");
                return string.Empty;
            }
        }

        private string ExtractBodyContent(string pageContent)
        {
            string bodyPattern = @"<\s*body[^>]*>(.*?)<\s*/\s*body\s*>";
            Match match = Regex.Match(
                pageContent, 
                bodyPattern, 
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return string.Empty;
        }

        public Task<List<string>> SearchAsync(string? query)
        {
            // Page body only
            logger.LogInformation($"Search: \"{query}\"");
            return Task.FromResult(new List<string>());
        }
    }
}
