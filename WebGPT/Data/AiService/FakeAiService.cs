namespace WebGPT.Data.AiService
{
    public class FakeAiService : IAiService
    {
        public async Task<string> GenerateMarkdownResponseAsync(string question, CancellationToken cancellationToken)
        {
            await Task.Delay(3000, cancellationToken);

            return $"Hello, **World**! The question was: \n{question}";
        }
    }
}
