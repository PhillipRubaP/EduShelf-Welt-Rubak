using EduShelf.Api.Models.Entities;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;

namespace EduShelf.Api.Services
{
    public class PromptGenerationService
    {
        private readonly IConfiguration _configuration;

        public PromptGenerationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ChatHistory BuildChatHistory(ChatSession chatSession, List<DocumentChunk> relevantChunks, string userInput)
        {
            var contextText = new StringBuilder();
            if (relevantChunks.Any())
            {
                foreach (var chunk in relevantChunks)
                {
                    contextText.AppendLine($"[{chunk.Document.Title}] {chunk.Content}");
                }
            }

            var contextString = contextText.ToString();
            var contextLengthLimit = _configuration.GetValue<int>("AIService:ContextLengthLimit", 4096);
            if (contextString.Length > contextLengthLimit)
            {
                contextString = TruncateToTokenLimit(contextString, contextLengthLimit);
            }

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(_configuration.GetValue<string>("AIService:Prompts:System") ?? "You are a helpful assistant.");

            foreach (var message in chatSession.ChatMessages.OrderBy(m => m.CreatedAt))
            {
                var userMessageContent = message.Message;
                if (!string.IsNullOrEmpty(message.ImageDescription))
                {
                    userMessageContent = $"[Image Description: {message.ImageDescription}]\n\n{userMessageContent}";
                }
                chatHistory.AddUserMessage(userMessageContent);
                if (!string.IsNullOrEmpty(message.Response))
                {
                    chatHistory.AddAssistantMessage(message.Response);
                }
            }

            var finalUserMessage = new StringBuilder();
            if (!string.IsNullOrEmpty(contextString))
            {
                Console.WriteLine($"[PromptGeneration] Injecting context of length {contextString.Length} chars into User Message.");
                finalUserMessage.AppendLine($"[Context from Documents]:\n{contextString}\n");
            }
            else 
            {
                Console.WriteLine("[PromptGeneration] No context available to inject.");
            }
            finalUserMessage.AppendLine($"[User Question]: {userInput}");

            chatHistory.AddUserMessage(finalUserMessage.ToString());

            return chatHistory;
        }

        private string TruncateToTokenLimit(string text, int tokenLimit)
        {
            if (text.Length <= tokenLimit) return text;
            return text.Substring(0, tokenLimit) + "...";
        }
    }
}