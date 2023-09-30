namespace WebGPT.Data.MarkdownService
{
    public interface IMarkdownService
    {
        public Task<string?> MarkdownToHtmlAsync(string text, CancellationToken cancellationToken);
    }
}
