using WebGPT.Data.Models;

namespace WebGPT.Data.ChatService
{
    public interface IChatService
    {
        public List<QuestionAnswer> Conversation { get; set; }

        public CancellationTokenSource? AnswerQuestionCancellationTokenSource { get; set; }

        public Task AnswerQuestionAsync(QuestionAnswer qa);

        public Task ClearConversationAsync();
    }
}
