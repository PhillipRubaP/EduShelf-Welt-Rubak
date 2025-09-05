using EduShelf.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Embeddings;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using System.Text;

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
                var embeddingGenerator = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();
                var promptEmbedding = await embeddingGenerator.GenerateEmbeddingAsync(userInput);

                var relevantChunks = await _context.DocumentChunks
                    .Include(dc => dc.Document)
                    .Where(dc => dc.Document.UserId == userId)
                    .OrderBy(dc => dc.Embedding.L2Distance(new Vector(promptEmbedding)))
                    .Take(20)
                    .ToListAsync();

                var contextText = new StringBuilder();
                foreach (var chunk in relevantChunks)
                {
                    contextText.AppendLine(chunk.Content);
                }

                var prompt = $"""
                You are a helpful AI assistant for the EduShelf platform.
                Answer the user's question based on the following context, which includes document titles.
                When a document title is relevant to the question, mention it in your answer.
                If the context doesn't contain the answer, say that you don't know.

                Context:
                {contextText}

                Question:
                {userInput}
                """;

                var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
                var result = await chatCompletionService.GetChatMessageContentAsync(prompt);

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