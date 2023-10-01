using Azure;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Http;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebGPT.Data.MarkdownService;
using WebGPT.Data.Models;
using WebGPT.Data.SearchService;

namespace WebGPT.Data.ChatService
{
    public class ChatService : IChatService
    {
        public const string AiChatModelName = "gpt-3.5-turbo";

        private readonly OpenAIClient client;
        private readonly ISearchService searchService;
        private readonly IMarkdownService markdownService;
        private readonly ILogger<ChatService> logger;

        public ChatService(
            OpenAIClient client,
            ISearchService searchService,
            IMarkdownService markdownService, 
            ILogger<ChatService> logger)
        {
            this.client = client;
            this.searchService = searchService;
            this.markdownService = markdownService;
            this.logger = logger;

            this.ClearConversation();
        }

        public ObservableCollection<ChatMessage> Conversation { get; set; } = new();

        public CancellationTokenSource? AnswerQuestionCancellationTokenSource { get; set; }

        public async Task AnswerLastQuestionAsync()
        {
            if (AnswerQuestionCancellationTokenSource is null)
            {
                return;
            }

            if (Conversation.LastOrDefault()?.Role != ChatRole.User)
            {
                return;
            }

            var token = this.AnswerQuestionCancellationTokenSource.Token;

            var answer = await this.GenerateAsync(token);
            var markdown = await markdownService.MarkdownToHtmlAsync(answer.Content, token);
            var markdownAnswer = new MdChatMessage(answer, markdown ?? string.Empty);

            this.Conversation.Add(markdownAnswer);
        }

        private async Task<ChatMessage> GenerateAsync(CancellationToken token)
        {
            var options = new ChatCompletionsOptions(this.Conversation);
            options.Functions.Add(searchWebFunctionDefinition);
            options.Functions.Add(getPageFunctionDefinition);

            var response = await client.GetChatCompletionsAsync(AiChatModelName, options, token);
            var responseChoice = response.Value.Choices[0];

            logger.LogInformation(
                $"""
                Stop reason: {responseChoice.FinishReason}
                AI answer: {responseChoice.Message.Content}
                """);

            if (responseChoice.FinishReason == CompletionsFinishReason.FunctionCall)
            {
                this.Conversation.Add(responseChoice.Message);

                var functionName = responseChoice.Message.FunctionCall.Name;

                if (functionName == "search_web")
                {
                    // Validate and process the JSON arguments for the function call
                    string unvalidatedArguments = responseChoice.Message.FunctionCall.Arguments;

                    var query = GetSearchWebFunctionQuery(unvalidatedArguments);
                    var searchResult = await this.searchService.SearchAsync(query);

                    object functionResultData = new
                    {
                        PagesUrls = searchResult
                    };

                    var functionResponseMessage = new ChatMessage(
                        ChatRole.Function,
                        JsonSerializer.Serialize(
                            functionResultData,
                            jsonSerializerOptions))
                    {
                        Name = responseChoice.Message.FunctionCall.Name
                    };

                    this.Conversation.Add(functionResponseMessage);
                }
                else if (functionName == "get_page")
                {
                    // Validate and process the JSON arguments for the function call
                    string unvalidatedArguments = responseChoice.Message.FunctionCall.Arguments;
                    var url = GetGetPageFunctionUrl(unvalidatedArguments);
                    var page = await this.searchService.GetPageAsync(url);
                    object functionResultData = new
                    {
                        PageBodyHtml = page
                    };

                    var functionResponseMessage = new ChatMessage(
                        ChatRole.Function,
                        JsonSerializer.Serialize(
                            functionResultData,
                            jsonSerializerOptions))
                    {
                        Name = responseChoice.Message.FunctionCall.Name
                    };

                    this.Conversation.Add(functionResponseMessage);
                }

                return await this.GenerateAsync(token);
            }

            return responseChoice.Message;
        }

        public void ClearConversation()
        {
            AnswerQuestionCancellationTokenSource?.Cancel();
            AnswerQuestionCancellationTokenSource = null;

            this.Conversation = new()
            {
                new ChatMessage
                {
                    Role = ChatRole.System,
                    Content = "Your name is WebGPT. You are AI chatbot with Google Search integration. Your creator is Barys Shauchuk (when mentioned, use this url: https://github.com/BarysShauchuk). Generate your answers using markdown and emoji."
                }
            };

            
        }

        public string? GetSearchWebFunctionQuery(string json)
        {
            return JsonSerializer
                .Deserialize<SearchWebFunctionArguments>(
                    json, 
                    jsonSerializerOptions)?
                .Query;
        }

        public string? GetGetPageFunctionUrl(string json)
        {
            return JsonSerializer
                .Deserialize<GetPageFunctionArguments>(
                    json, 
                    jsonSerializerOptions)?
                .Url;
        }

        private static readonly JsonSerializerOptions jsonSerializerOptions 
            = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        private class SearchWebFunctionArguments
        {
            public string Query { get; set; } = string.Empty;
        }
        private class GetPageFunctionArguments
        {
            public string Url { get; set; } = string.Empty;
        }

        private FunctionDefinition searchWebFunctionDefinition = new FunctionDefinition
        {
            Name = "search_web",
            Description = "Search query in the web. Return list of URLs.",
            Parameters = BinaryData.FromObjectAsJson(
                    new
                    {
                        Type = "object",
                        Properties = new
                        {
                            Query = new
                            {
                                Type = "string",
                            }
                        },
                        Required = new[] { "query" },
                    },
                    jsonSerializerOptions),
        };

        private FunctionDefinition getPageFunctionDefinition = new FunctionDefinition
        {
            Name = "get_page",
            Description = "Get page content by URL.",
            Parameters = BinaryData.FromObjectAsJson(
                    new
                    {
                        Type = "object",
                        Properties = new
                        {
                            Url = new
                            {
                                Type = "string",
                            }
                        },
                        Required = new[] { "url" },
                    },
                    jsonSerializerOptions),
        };
    }
}
