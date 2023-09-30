using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebGPT.Data.AiService;
using WebGPT.Data.MarkdownService;
using WebGPT.Data.Models;

namespace WebGPT.Data.ChatService
{
    public class ChatService : IChatService
    {
        private readonly IAiService aiService;
        private readonly IMarkdownService markdownService;

        public ChatService(IAiService aiService, IMarkdownService markdownService)
        {
            this.aiService = aiService;
            this.markdownService = markdownService;
        }

        public List<QuestionAnswer> Conversation { get; set; } = new();

        public CancellationTokenSource? AnswerQuestionCancellationTokenSource { get; set; }

        public async Task AnswerQuestionAsync(QuestionAnswer qa)
        {
            if (AnswerQuestionCancellationTokenSource is null)
            {
                return;
            }

            var token = AnswerQuestionCancellationTokenSource.Token;
            var markdown = await aiService.GenerateMarkdownResponseAsync(qa.Question, token);
            qa.Answer = await markdownService.MarkdownToHtmlAsync(markdown, token);
        }

        public Task ClearConversationAsync()
        {
            AnswerQuestionCancellationTokenSource?.Cancel();
            AnswerQuestionCancellationTokenSource = null;

            Conversation.Clear();
            return Task.CompletedTask;
        }
    }
}
