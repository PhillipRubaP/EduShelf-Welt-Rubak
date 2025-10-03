using EduShelf.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Embeddings;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;
using EduShelf.Api.Models.Entities;
using Microsoft.Extensions.Configuration;

namespace EduShelf.Api.Services
{
    public class ChatService
    {
        private readonly ApiDbContext _context;
        private readonly Kernel _kernel;
        private readonly ILogger<ChatService> _logger;
        private readonly IConfiguration _configuration;

        public ChatService(ApiDbContext context, Kernel kernel, ILogger<ChatService> logger, IConfiguration configuration)
        {
            _context = context;
            _kernel = kernel;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<string> GetResponseAsync(string userInput, int userId)
        {
            try
            {
                var contextText = new StringBuilder();
                List<DocumentChunk> relevantChunks;

                // Regex to find "file 'filename.ext'"
                var fileMatch = Regex.Match(userInput, @"file\s+'([^']+\.\w+)'", RegexOptions.IgnoreCase);

                if (fileMatch.Success)
                {
                    var fileName = fileMatch.Groups[1].Value;
                    relevantChunks = await _context.DocumentChunks
                        .Include(dc => dc.Document)
                        .Where(dc => dc.Document.UserId == userId && EF.Functions.ILike(dc.Document.Title, fileName))
                        .ToListAsync();
                }
                else
                {
                    var embeddingGenerator = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();
                    var promptEmbedding = await embeddingGenerator.GenerateEmbeddingAsync(userInput);

                    var k = GetDynamicK(userInput);

                    relevantChunks = await _context.DocumentChunks
                        .Include(dc => dc.Document)
                        .Where(dc => dc.Document.UserId == userId)
                        .OrderBy(dc => dc.Embedding.L2Distance(new Vector(promptEmbedding)))
                        .Take(k)
                        .ToListAsync();
                }

                if (!relevantChunks.Any())
                {
                    contextText.AppendLine("No relevant documents found.");
                }
                else
                {
                    foreach (var chunk in relevantChunks)
                    {
                        // Titel hinzufügen für besseren Kontext
                        contextText.AppendLine($"[{chunk.Document.Title}] {chunk.Content}");
                    }
                }

                // Promptgröße begrenzen (z. B. 4000 Zeichen)
                var contextString = contextText.ToString();
                var contextLengthLimit = _configuration.GetValue<int>("AIService:ContextLengthLimit", 4096);

                if (contextString.Length > contextLengthLimit)
                {
                    contextString = TruncateToTokenLimit(contextString, contextLengthLimit);
                }

                var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

                // ChatHistory statt einfachem Prompt
                var chatHistory = new ChatHistory();
                chatHistory.AddSystemMessage("""
                    You are a learning assistant on the EduShelf platform. 
                    Your primary role is to help users study and learn from the documents they upload. 
                    - Always prioritize using the content of the provided documents. 
                    - If the answer is not in the documents, clearly say: "This is not in your documents," and then you may provide general knowledge if it is accurate and helpful. 
                    - When you mix document-based information and general knowledge, always separate them clearly. 
                    - Be neutral, concise, and explanatory — never inject your own opinions or judge the document content. 
                    - If asked to summarize or explain, always mention which document the content came from when possible.

                """);

                chatHistory.AddSystemMessage($"Context:\n{contextString}");
                chatHistory.AddUserMessage(userInput);

                var result = await chatCompletionService.GetChatMessageContentAsync(chatHistory);

                return result.Content ?? "I'm sorry, I couldn't generate a response.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting response from ChatService.");
                return "An error occurred while processing your request.";
            }
        }

        private int GetDynamicK(string userInput)
        {
            var wordCount = userInput.Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;

            if (wordCount < 10) return 5;
            if (wordCount <= 30) return 10;
            return 20;
        }

        private string TruncateToTokenLimit(string text, int tokenLimit)
        {
            // Simple heuristic: 1 token ~= 4 characters
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