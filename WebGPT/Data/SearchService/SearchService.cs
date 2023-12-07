using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using static System.Net.Mime.MediaTypeNames;
using WebGPT.Configuration;
using WebGPT.Extensions;
using Microsoft.Extensions.Options;
using System.Text;
using Rext = WebGPT.Extensions.RegexExtensions;
using System;

namespace WebGPT.Data.SearchService
{
    public partial class SearchService : ISearchService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ApiSettings apiSettings;
        private readonly ILogger<SearchService> logger;

        public SearchService(
            IHttpClientFactory httpClientFactory,
            IOptionsMonitor<ApiSettings> options,
            ILogger<SearchService> logger)
        {
            this.httpClientFactory = httpClientFactory;
            apiSettings = options.Get("Brave");
            this.logger = logger;
        }

        public async Task<string> GetPageAsync(string? url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }

            using HttpClient client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            try
            {
                using HttpResponseMessage response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError($"Search: get page with URL {url} failed");
                    return string.Empty;
                }

                string pageContent = await response.Content.ReadAsStringAsync();
                string bodyContent = ExtractContent(pageContent);

                logger.LogInformation(
                    $"Search: get page with URL {url}. Chars in response: {bodyContent.Length}");

                return bodyContent;
            }
            catch (Exception ex)
            {
                logger.LogError($"Search: get page with URL {url} failed: {ex.Message}");
                return "[Page currently unavailable.]";
            }
        }

        private string ExtractContent(string pageContent)
        {
            string body = Rext.BodyRegex().Match(pageContent).Value;

            var text = body
                .ReplaceRegex(Rext.ScriptRegex())
                .ReplaceRegex(Rext.StyleRegex())
                .ReplaceRegex(Rext.HtmlTagsRegex())
                .ReplaceRegex(Rext.WhiteSpaceRegex(), " ");

            return text;
        }

        public async Task<List<string>> SearchAsync(string? query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return new List<string>();
            }

            logger.LogInformation($"Search: \"{query}\"");

            var webQuery = HttpUtility.UrlEncode(query);

            using var httpClient = httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(
                new HttpMethod("GET"), $"{apiSettings.BaseUrl}/web/search?q={webQuery}");

            request.Headers.TryAddWithoutValidation("X-Subscription-Token", apiSettings.ApiKey);

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return new List<string>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<WebSearchResponse>(json);
            var pages = result?.web?.results?
                .Select(r => r.profile?.url ?? string.Empty)
                .ToList() ?? new List<string>();

            return pages;
        }

        private class WebSearchResponse
        {
            public Search? web { get; set; }
        }

        private class Search
        {
            public SearchResult[]? results { get; set; }
        }

        private class SearchResult
        {
            public Profile? profile { get; set; }
        }

        private class Profile
        {
            public string? url { get; set; }
        }
    }
}
