using EduShelf.Api.Data;
using EduShelf.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Pgvector.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.AI;

namespace EduShelf.Api.Services
{
    public class RetrievalService
    {
        private readonly ApiDbContext _context;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingService;
        private readonly int _maxContextLength;

        public RetrievalService(ApiDbContext context, IEmbeddingGenerator<string, Embedding<float>> embeddingService, IConfiguration configuration)
        {
            _context = context;
            _embeddingService = embeddingService;
            _maxContextLength = configuration.GetValue<int>("AIService:ContextLengthLimit", 4096);
        }

        public async Task<(string, List<string>)> GetContextAndSourcesAsync(string query, int userId)
        {
            var queryEmbedding = await _embeddingService.GenerateAsync(query);

            var chunks = await _context.DocumentChunks
                .Where(dc => dc.Document.UserId == userId)
                .OrderBy(dc => dc.Embedding.L2Distance(queryEmbedding))
                .Take(10)
                .Include(dc => dc.Document)
                .ToListAsync();

            var sources = chunks.Select(c => c.Document.Title).Distinct().ToList();
            var context = CombineAndOrderChunks(chunks);

            return (context, sources);
        }

        private string CombineAndOrderChunks(List<DocumentChunk> chunks)
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
            if (context.Length <= _maxContextLength)
            {
                return context;
            }

            var truncated = context.Substring(0, _maxContextLength);
            var lastSentenceEnd = Math.Max(truncated.LastIndexOf('.'), Math.Max(truncated.LastIndexOf('?'), truncated.LastIndexOf('!')));

            if (lastSentenceEnd > -1)
            {
                return truncated.Substring(0, lastSentenceEnd + 1);
            }

            return truncated;
        }
    }
}