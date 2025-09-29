using EduShelf.Api.Data;
using EduShelf.Api.Models.Entities;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Text;
using UglyToad.PdfPig;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;

namespace EduShelf.Api.Services
{
    public class IndexingService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<IndexingService> _logger;
        private readonly string _uploadPath;

        public IndexingService(IServiceScopeFactory scopeFactory, ILogger<IndexingService> logger, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _uploadPath = configuration["FileStorage:UploadPath"] ?? "Uploads";
        }

        public async Task IndexDocumentAsync(int documentId, string fileName, int? batchSize = null)
        {
            try
            {
                var fullPath = Path.Combine(_uploadPath, fileName);
                _logger.LogInformation("Starting indexing for document ID {DocumentId} at path {FilePath}", documentId, fullPath);
                if (!File.Exists(fullPath))
                {
                    _logger.LogError("File not found at path {FilePath}", fullPath);
                    return;
                }

                var content = ExtractTextFromFile(fullPath);
                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogWarning("No content extracted from document {DocumentId}", documentId);
                    return;
                }

#pragma warning disable SKEXP0055 // Experimental
                var chunks = ChunkContent(content);
#pragma warning restore SKEXP0055 // Experimental

                _logger.LogInformation("Document split into {ChunkCount} chunks.", chunks.Count);

                if (batchSize.HasValue)
                {
                    await ProcessInBatches(documentId, chunks, batchSize.Value);
                }
                else
                {
                    await ProcessAll(documentId, chunks);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while indexing document {DocumentId}", documentId);
            }
        }

        private List<string> ChunkContent(string content)
        {
            var sentences = TextChunker.SplitBySentence(content);
            var chunks = new List<string>();
            var currentChunk = new StringBuilder();

            foreach (var sentence in sentences)
            {
                if (currentChunk.Length + sentence.Length > 1024 && currentChunk.Length > 0)
                {
                    chunks.Add(currentChunk.ToString().Trim());
                    currentChunk.Clear();
                }
                currentChunk.Append(sentence);
            }

            if (currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString().Trim());
            }

            return chunks;
        }

        private async Task ProcessAll(int documentId, List<string> chunks)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            var kernel = scope.ServiceProvider.GetRequiredService<Kernel>();
            var embeddingGenerator = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
            var document = await context.Documents.FindAsync(documentId);
            var title = document?.Title ?? "Unknown Document";

            foreach (var chunk in chunks)
            {
                var contentWithTitle = $"Document: {title}\n{chunk}";
                var embedding = await embeddingGenerator.GenerateEmbeddingAsync(contentWithTitle);
                var documentChunk = new DocumentChunk
                {
                    DocumentId = documentId,
                    Content = contentWithTitle,
                    Embedding = new Pgvector.Vector(embedding)
                };
                context.DocumentChunks.Add(documentChunk);
            }

            await context.SaveChangesAsync();
            _logger.LogInformation("Successfully indexed {ChunkCount} chunks for document ID {DocumentId}", chunks.Count, documentId);
        }

        private async Task ProcessInBatches(int documentId, List<string> chunks, int batchSize)
        {
            var totalChunks = 0;
            for (int i = 0; i < chunks.Count; i += batchSize)
            {
                var batch = chunks.Skip(i).Take(batchSize).ToList();
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
                var kernel = scope.ServiceProvider.GetRequiredService<Kernel>();
                var embeddingGenerator = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
                var document = await context.Documents.FindAsync(documentId);
                var title = document?.Title ?? "Unknown Document";

                foreach (var chunk in batch)
                {
                    var contentWithTitle = $"Document: {title}\n{chunk}";
                    var embedding = await embeddingGenerator.GenerateEmbeddingAsync(contentWithTitle);
                    var documentChunk = new DocumentChunk
                    {
                        DocumentId = documentId,
                        Content = contentWithTitle,
                        Embedding = new Pgvector.Vector(embedding)
                    };
                    context.DocumentChunks.Add(documentChunk);
                }

                await context.SaveChangesAsync();
                totalChunks += batch.Count;
                _logger.LogInformation("Indexed batch of {BatchCount} chunks for document ID {DocumentId}", batch.Count, documentId);
            }
            _logger.LogInformation("Successfully indexed a total of {TotalChunks} chunks for document ID {DocumentId}", totalChunks, documentId);
        }

        private string ExtractTextFromFile(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            switch (extension)
            {
                case ".pdf":
                    return ExtractTextFromPdf(filePath);
                case ".docx":
                    return ExtractTextFromDocx(filePath);
                case ".txt":
                    return File.ReadAllText(filePath);
                default:
                    _logger.LogWarning("Unsupported file type for text extraction: {Extension}", extension);
                    return string.Empty;
            }
        }

        private string ExtractTextFromPdf(string filePath)
        {
            using var pdf = PdfDocument.Open(filePath);
            var text = new StringBuilder();
            foreach (var page in pdf.GetPages())
            {
                text.Append(page.Text);
            }
            return text.ToString();
        }

        private string ExtractTextFromDocx(string filePath)
        {
            using var doc = WordprocessingDocument.Open(filePath, false);
            return doc.MainDocumentPart?.Document.Body?.InnerText ?? string.Empty;
        }
    }
}