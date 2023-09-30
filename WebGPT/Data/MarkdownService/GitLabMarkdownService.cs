using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebGPT.Configuration;

namespace WebGPT.Data.MarkdownService
{
    public class GitLabMarkdownService : IMarkdownService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ApiSettings apiSettings;

        public GitLabMarkdownService(
            IHttpClientFactory httpClientFactory, 
            IOptionsMonitor<ApiSettings> options)
        {
            this.httpClientFactory = httpClientFactory;
            this.apiSettings = options.Get("GitLab");
        }

        public async Task<string?> MarkdownToHtmlAsync(string text, CancellationToken cancellationToken)
        {
            using var httpClient = this.httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(
                new HttpMethod("POST"), $"{apiSettings.BaseUrl}/markdown");

            request.Headers.TryAddWithoutValidation("PRIVATE-TOKEN", apiSettings.ApiKey);

            request.Content = new StringContent(
                JsonSerializer.Serialize(new MarkdownRequest(text)));

            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<MarkdownResponse>(json)?.Html;
            }

            return null;
        }

        private class MarkdownRequest
        {
            public MarkdownRequest(string text)
            {
                Text = text;
            }

            [JsonPropertyName("text")]
            public string Text { get; set; }

            [JsonPropertyName("gfm")]
            public bool Gfm { get; set; } = false;
        }

        private class MarkdownResponse
        {
            [JsonPropertyName("html")]
            public string? Html { get; set; }
        }
    }
}
