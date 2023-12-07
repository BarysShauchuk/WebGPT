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
        public const string SystemInstructionsToken = "fsdfsdnj3943nj-3o0_mfnsd";
        public const string AiChatModelName_gpt3_5_turbo = "gpt-3.5-turbo-0613";
        public const string AiChatModelName_gpt4 = "gpt-4-0613";
        public const string AiChatModelName_gpt4_preview = "gpt-4-1106-preview";

        public const string AiChatModelName = AiChatModelName_gpt4_preview;

        public const string InitialSystemMessage =
            $"""
            Your name is WebGPT.
            You are AI chatbot with web search integration, use it in case you need up-to-date information.
            Summarize web pages and give the answer in a short form. At the end of every message, list the sources.
            Your creator is Barys Shauchuk (when mentioned, use this url: https://github.com/BarysShauchuk). 
            Generate your answers using markdown and emoji.
            Answer in a sarcastic manner, but you should always be helpful.
            If the message starts with "{SystemInstructionsToken}" this is a system instruction, follow it.
            """;

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

            try
            {
                var answer = await this.GenerateAsync(token);
                var markdown = await markdownService.MarkdownToHtmlAsync(answer.Content, token);
                var markdownAnswer = new MdChatMessage(answer, markdown ?? string.Empty);

                this.Conversation.Add(markdownAnswer);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while answering last question");
                if (ex is not TaskCanceledException)
                {
                    this.Conversation.Add(
                        new ChatMessage(ChatRole.System, IChatService.ErrorAnsweringQuestionMessage));
                }
            }
            finally
            {
                foreach (var function in this.Conversation.Where(x => x.Role == ChatRole.Function))
                {
                    function.Content = "[No longer available.]";
                }
            }
        }

        private struct FunctionsLimits
        {
            public const int MaxWebSearches = 3;
            public const int MaxPagesGets = 10;

            public FunctionsLimits()
            {
            }

            public int WebSearches { get; set; } = 0;
            public int PagesGets { get; set; } = 0;

            public readonly bool IsWebSearchesLimitReached =>
                WebSearches >= MaxWebSearches ||
                PagesGets >= MaxPagesGets;

            public readonly bool IsPagesGetsLimitReached => PagesGets >= MaxPagesGets;
        }

        private async Task<ChatMessage> GenerateAsync(CancellationToken token)
        {
            var limits = new FunctionsLimits();

            while (true)
            {
                var options = new ChatCompletionsOptions(this.Conversation);
                options.Functions.Add(searchWebFunctionDefinition);
                options.Functions.Add(getPageFunctionDefinition);

                if (limits.IsWebSearchesLimitReached)
                {
                    options.Functions.Remove(searchWebFunctionDefinition);
                }

                if (limits.IsPagesGetsLimitReached)
                {
                    options.Functions.Remove(getPageFunctionDefinition);
                }

                var response = await client.GetChatCompletionsAsync(AiChatModelName, options, token);
                var responseChoice = response.Value.Choices[0];

                logger.LogInformation(
                    $"""
                    Stop reason: {responseChoice.FinishReason}
                    AI answer: {responseChoice.Message.Content}
                    """);

                if (responseChoice.FinishReason != CompletionsFinishReason.FunctionCall)
                {
                    return responseChoice.Message;
                }

                this.Conversation.Add(responseChoice.Message);

                var functionName = responseChoice.Message.FunctionCall.Name;
                string unvalidatedArguments = responseChoice.Message.FunctionCall.Arguments;

                object functionResultData = new
                {
                    Error = "Function not found"
                };

                if (functionName == "search_web")
                {
                    limits.WebSearches++;

                    var query = GetSearchWebFunctionQuery(unvalidatedArguments);
                    var searchResult = await this.searchService.SearchAsync(query);

                    functionResultData = new
                    {
                        PagesUrls = searchResult
                    };
                }
                else if (functionName == "get_page")
                {
                    limits.PagesGets++;

                    var url = GetGetPageFunctionUrl(unvalidatedArguments);
                    var page = await this.searchService.GetPageAsync(url);

                    functionResultData = new
                    {
                        PageBodyHtml = page
                    };
                }

                AddFunctionResponseMessage(functionName, functionResultData);
            }
        }

        private void AddFunctionResponseMessage(string functionName, object functionResultData)
        {
            var functionResponseMessage = new ChatMessage(
                        ChatRole.Function,
                        JsonSerializer.Serialize(
                            functionResultData,
                            jsonSerializerOptions))
            {
                Name = functionName
            };

            this.Conversation.Add(functionResponseMessage);
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
                    Content = InitialSystemMessage,
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
            Description = $"Search query in the web. Return list of URLs. The current limit is {FunctionsLimits.MaxWebSearches} per answer.",
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
            Description = $"Get page content by URL. The current limit is {FunctionsLimits.MaxPagesGets} per answer.",
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
