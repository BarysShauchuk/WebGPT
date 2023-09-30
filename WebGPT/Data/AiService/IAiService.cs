namespace WebGPT.Data.AiService
{
    public interface IAiService
    {
        public Task<string> GenerateMarkdownResponseAsync(string question, CancellationToken cancellationToken);
    }
}
