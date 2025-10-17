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
            chatHistory.AddSystemMessage(_configuration.GetValue<string>("AIService:Prompts:System"));

            foreach (var message in chatSession.ChatMessages.OrderBy(m => m.CreatedAt))
            {
                chatHistory.AddUserMessage(message.Message);
                if (!string.IsNullOrEmpty(message.Response))
                {
                    chatHistory.AddAssistantMessage(message.Response);
                }
            }

            chatHistory.AddSystemMessage($"Context:\n{contextString}");
            chatHistory.AddUserMessage(userInput);

            return chatHistory;
        }

        private string TruncateToTokenLimit(string text, int tokenLimit)
        {
            if (text.Length <= tokenLimit) return text;

            var sentences = text.Split(new[] { ". ", "! ", "? " }, StringSplitOptions.RemoveEmptyEntries);
            var truncatedText = new StringBuilder();
            var currentLength = 0;

            foreach (var sentence in sentences)
            {
                if (currentLength + sentence.Length + 2 > tokenLimit)
                {
                    break;
                }
                truncatedText.Append(sentence).Append(". ");
                currentLength += sentence.Length + 2;
            }

            return truncatedText.ToString().TrimEnd() + "...";
        }
    }
}