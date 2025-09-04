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

        public IndexingService(IServiceScopeFactory scopeFactory, ILogger<IndexingService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task IndexDocumentAsync(int documentId, string filePath)
        {
            try
            {
                _logger.LogInformation("Starting indexing for document ID {DocumentId} at path {FilePath}", documentId, filePath);

                var content = ExtractTextFromFile(filePath);
                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogWarning("No content extracted from document {DocumentId}", documentId);
                    return;
                }

#pragma warning disable SKEXP0050 // Experimental
                List<string> lines = TextChunker.SplitPlainTextLines(content, 256);
                var chunks = TextChunker.SplitPlainTextParagraphs(lines, 1024);
#pragma warning restore SKEXP0050 // Experimental
                
                _logger.LogInformation("Document split into {ChunkCount} chunks.", chunks.Count);

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while indexing document {DocumentId}", documentId);
            }
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