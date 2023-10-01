using Azure.AI.OpenAI;
using System.Collections.ObjectModel;

namespace WebGPT.Data.ChatService
{
    public interface IChatService
    {
        public ObservableCollection<ChatMessage> Conversation { get; set; }

        public CancellationTokenSource? AnswerQuestionCancellationTokenSource { get; set; }

        public Task AnswerLastQuestionAsync();

        public void ClearConversation();

        public string? GetSearchWebFunctionQuery(string json);
    }
}
