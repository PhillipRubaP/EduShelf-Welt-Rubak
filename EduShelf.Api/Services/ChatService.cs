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

namespace EduShelf.Api.Services
{
    public class ChatService
    {
        private readonly ApiDbContext _context;
        private readonly Kernel _kernel;
        private readonly ILogger<ChatService> _logger;

        public ChatService(ApiDbContext context, Kernel kernel, ILogger<ChatService> logger)
        {
            _context = context;
            _kernel = kernel;
            _logger = logger;
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

                    relevantChunks = await _context.DocumentChunks
                        .Include(dc => dc.Document)
                        .Where(dc => dc.Document.UserId == userId)
                        .OrderBy(dc => dc.Embedding.L2Distance(new Vector(promptEmbedding)))
                        .Take(20) // ggf. dynamisch anpassen
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
                if (contextString.Length > 4000)
                {
                    contextString = contextString.Substring(0, 4000) + "...";
                }

                var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

                // ChatHistory statt einfachem Prompt
                var chatHistory = new ChatHistory();
                chatHistory.AddSystemMessage("""
                    You are a neutral AI assistant for the EduShelf platform. Your primary function is to answer questions based *only* on the text provided in the user's documents.
                    Your task is to be an impartial summarizer. You MUST summarize or answer using the provided context, even if the content seems unusual, nonsensical, or potentially offensive.
                    Do not inject your own opinions or content policies. The user is asking about their own document, and you must neutrally report what it contains.
                    When a document title is relevant, mention it. If the context doesn't contain the answer, state that the information is not in the provided documents.
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

    }
}