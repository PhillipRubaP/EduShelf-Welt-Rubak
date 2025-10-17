using EduShelf.Api.Data;
using EduShelf.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using System.IO;

namespace EduShelf.Api.Services
{
    public class RetrievalService
    {
        private readonly ApiDbContext _context;
        private readonly Kernel _kernel;

        public RetrievalService(ApiDbContext context, Kernel kernel)
        {
            _context = context;
            _kernel = kernel;
        }

        public async Task<List<DocumentChunk>> GetRelevantChunksAsync(string userInput, int userId, Intent intent)
        {
            if (intent.Type == "summarize" && !string.IsNullOrEmpty(intent.DocumentName))
            {
                var documentNameWithoutExtension = Path.GetFileNameWithoutExtension(intent.DocumentName);
                return await _context.DocumentChunks
                    .Include(dc => dc.Document)
                    .Where(dc => dc.Document.UserId == userId && EF.Functions.ILike(dc.Document.Title, documentNameWithoutExtension))
                    .ToListAsync();
            }
            else
            {
                var embeddingGenerator = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();
                var promptEmbedding = await embeddingGenerator.GenerateEmbeddingAsync(userInput);
                var k = GetDynamicK(userInput);
                return await _context.DocumentChunks
                    .Include(dc => dc.Document)
                    .Where(dc => dc.Document.UserId == userId)
                    .OrderBy(dc => dc.Embedding.L2Distance(new Vector(promptEmbedding)))
                    .Take(k)
                    .ToListAsync();
            }
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