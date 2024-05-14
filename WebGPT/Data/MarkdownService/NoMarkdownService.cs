
namespace WebGPT.Data.MarkdownService
{
    public class NoMarkdownService : IMarkdownService
    {
        public Task<string?> MarkdownToHtmlAsync(string text, CancellationToken cancellationToken)
        {
            return Task.FromResult<string?>(text);
        }
    }
}
