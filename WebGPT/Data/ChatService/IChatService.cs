using Azure.AI.OpenAI;
using System.Collections.ObjectModel;

namespace WebGPT.Data.ChatService
{
    public interface IChatService
    {
        public const string QuestionCanceledMessage = "Question was canceled.";

        public const string ErrorAnsweringQuestionMessage = "An error occurred while answering the question.";

        public ObservableCollection<ChatMessage> Conversation { get; set; }

        public CancellationTokenSource? AnswerQuestionCancellationTokenSource { get; set; }

        public Task AnswerLastQuestionAsync();

        public void ClearConversation();

        public string? GetSearchWebFunctionQuery(string json);
    }
}
