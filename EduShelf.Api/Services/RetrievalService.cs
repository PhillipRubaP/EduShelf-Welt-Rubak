using EduShelf.Api.Data;
using EduShelf.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using System.IO;
using Microsoft.Extensions.AI;

namespace EduShelf.Api.Services
{
    public class RetrievalService
    {
        private readonly ApiDbContext _context;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
        private readonly ILogger<RetrievalService> _logger;

        public RetrievalService(ApiDbContext context, IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, ILogger<RetrievalService> logger)
        {
            _context = context;
            _embeddingGenerator = embeddingGenerator;
            _logger = logger;
        }

        public async Task<List<DocumentChunk>> GetRelevantChunksAsync(string userInput, int userId, Intent intent)
        {
            _logger.LogInformation("Retrieving chunks for Intent: {IntentType}, Document: {DocumentName}", intent.Type, intent.DocumentName);
            Console.WriteLine($"[RetrievalService] User {userId} asking about '{intent.DocumentName}' (Intent: {intent.Type})");

            if (!string.IsNullOrEmpty(intent.DocumentName))
            {
                var documentNameWithoutExtension = Path.GetFileNameWithoutExtension(intent.DocumentName);
                
                var chunks = await _context.DocumentChunks
                    .Include(dc => dc.Document)
                    .Where(dc => dc.Document.UserId == userId && 
                           (EF.Functions.ILike(dc.Document.Title, intent.DocumentName) || 
                            EF.Functions.ILike(dc.Document.Title, documentNameWithoutExtension)))
                    .ToListAsync();

                if (chunks.Any())
                {
                    Console.WriteLine($"[RetrievalService] Found {chunks.Count} chunks by name match ('{intent.DocumentName}')");
                    Console.WriteLine($"[RetrievalService] Content Preview: {chunks.First().Content.Substring(0, Math.Min(100, chunks.First().Content.Length))}...");
                    return chunks;
                }
                
                // Fallback: If no documents found by name, try semantic search
                Console.WriteLine($"[RetrievalService] No chunks found by name ('{intent.DocumentName}'). Falling back to semantic search.");
            }
            
            // Fallthrough to embedding search
            var promptEmbedding = await _embeddingGenerator.GenerateAsync(userInput);
            var k = GetDynamicK(userInput);
            return await _context.DocumentChunks
                .Include(dc => dc.Document)
                .Where(dc => dc.Document.UserId == userId)
                .OrderBy(dc => dc.Embedding.L2Distance(new Vector(promptEmbedding.Vector)))
                .Take(k)
                .ToListAsync();
        }


        private int GetDynamicK(string userInput)
        {
            var wordCount = userInput.Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;

            if (wordCount < 10) return 5;
            if (wordCount <= 30) return 10;
            return 20;
        }
    }
}