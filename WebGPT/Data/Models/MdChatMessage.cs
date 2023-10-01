using Azure.AI.OpenAI;

namespace WebGPT.Data.Models
{
    public class MdChatMessage : ChatMessage
    {
        public MdChatMessage(ChatMessage chatMessage, string markdownContent) 
            : base(chatMessage.Role, chatMessage.Content)
        {
            this.MarkdownContent = markdownContent;
            this.Name = chatMessage.Name;
        }

        public string MarkdownContent { get; set; }
    }
}
