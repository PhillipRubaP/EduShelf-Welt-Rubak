using EduShelf.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Embeddings;
using Pgvector.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

namespace EduShelf.Api.Services
{
    public class RAGService : IRAGService
    {
        private readonly ApiDbContext _context;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingService;
        private const int MaxContextLength = 4096;

        public RAGService(ApiDbContext context, IEmbeddingGenerator<string, Embedding<float>> embeddingService)
        {
            _context = context;
            _embeddingService = embeddingService;
        }

        public async Task<(string, List<string>)> GetContextAndSourcesAsync(string query, int userId)
        {
            var queryEmbedding = await _embeddingService.GenerateAsync(query);

            var chunks = await _context.DocumentChunks
                .Where(dc => dc.Document.UserId == userId)
                .OrderBy(dc => dc.Embedding.L2Distance(queryEmbedding))
                .Take(10)
                .ToListAsync();

            var sources = chunks.Select(c => c.Document.Title).Distinct().ToList();
            var context = CombineAndOrderChunks(chunks);

            return (context, sources);
        }

        private string CombineAndOrderChunks(List<Models.Entities.DocumentChunk> chunks)
        {
            var orderedChunks = chunks.OrderBy(c => c.DocumentId).ThenBy(c => c.Page);
            var contextBuilder = new StringBuilder();
            foreach (var chunk in orderedChunks)
            {
                contextBuilder.AppendLine(chunk.Content);
            }
            return TruncateContext(contextBuilder.ToString());
        }

        private string TruncateContext(string context)
        {
            if (context.Length <= MaxContextLength)
            {
                return context;
            }

            var truncated = context.Substring(0, MaxContextLength);
            var lastSentenceEnd = Math.Max(truncated.LastIndexOf('.'), Math.Max(truncated.LastIndexOf('?'), truncated.LastIndexOf('!')));

            if (lastSentenceEnd > -1)
            {
                return truncated.Substring(0, lastSentenceEnd + 1);
            }

            return truncated;
        }
    }
}