using Microsoft.Extensions.Options;
using WebGPT.Configuration;

namespace WebGPT.Data.AiService
{
    public class GptService : IAiService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ApiSettings apiSettings;

        public GptService(
            IHttpClientFactory httpClientFactory, 
            IOptionsMonitor<ApiSettings> options)
        {
            this.httpClientFactory = httpClientFactory;
            this.apiSettings = options.Get("OpenAi");
        }

        public Task<string> GenerateMarkdownResponseAsync(string question, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
