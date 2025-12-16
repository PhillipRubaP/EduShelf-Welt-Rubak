using EduShelf.Api.Data;
using EduShelf.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
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
        private readonly IImageProcessingService _imageProcessingService;
        private readonly string _uploadPath;

        public IndexingService(IServiceScopeFactory scopeFactory, ILogger<IndexingService> logger, IImageProcessingService imageProcessingService, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _imageProcessingService = imageProcessingService;
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

                var content = await ExtractTextFromFileAsync(fullPath);
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
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
                var kernel = scope.ServiceProvider.GetRequiredService<Kernel>();
                var embeddingGenerator = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

                // Clear old chunks before adding new ones
                var oldChunks = await context.DocumentChunks.Where(dc => dc.DocumentId == documentId).ToListAsync();
                if (oldChunks.Any())
                {
                    context.DocumentChunks.RemoveRange(oldChunks);
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Removed {OldChunkCount} old chunks for document ID {DocumentId}", oldChunks.Count, documentId);
                }

                foreach (var chunk in chunks)
                {
                    var embedding = await embeddingGenerator.GenerateEmbeddingAsync(chunk);
                    var documentChunk = new DocumentChunk
                    {
                        DocumentId = documentId,
                        Content = chunk,
                        Embedding = new Pgvector.Vector(embedding),
                        Page = 0 // Placeholder for now
                    };
                    context.DocumentChunks.Add(documentChunk);
                }

                await context.SaveChangesAsync();
            }
            _logger.LogInformation("Successfully indexed {ChunkCount} chunks for document ID {DocumentId}", chunks.Count, documentId);
        }

        private async Task ProcessInBatches(int documentId, List<string> chunks, int batchSize)
        {
            var totalChunks = 0;
            for (int i = 0; i < chunks.Count; i += batchSize)
            {
                var batch = chunks.Skip(i).Take(batchSize).ToList();
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
                    var kernel = scope.ServiceProvider.GetRequiredService<Kernel>();
                    var embeddingGenerator = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

                    // It's better to clear all old chunks once at the start,
                    // but if we must do it in batches, this logic would need to be more complex.
                    // For simplicity, we assume the main `IndexDocumentAsync` handles clearing.

                    foreach (var chunk in batch)
                    {
                        var embedding = await embeddingGenerator.GenerateEmbeddingAsync(chunk);
                        var documentChunk = new DocumentChunk
                        {
                            DocumentId = documentId,
                            Content = chunk,
                            Embedding = new Pgvector.Vector(embedding),
                            Page = 0 // Placeholder
                        };
                        context.DocumentChunks.Add(documentChunk);
                    }

                    await context.SaveChangesAsync();
                }
                totalChunks += batch.Count;
                _logger.LogInformation("Indexed batch of {BatchCount} chunks for document ID {DocumentId}", batch.Count, documentId);
            }
            _logger.LogInformation("Successfully indexed a total of {TotalChunks} chunks for document ID {DocumentId}", totalChunks, documentId);
        }

        private async Task<string> ExtractTextFromFileAsync(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            switch (extension)
            {
                case ".pdf":
                    return await ExtractTextFromPdfAsync(filePath);
                case ".docx":
                    return ExtractTextFromDocx(filePath);
                case ".txt":
                    return await File.ReadAllTextAsync(filePath);
                default:
                    _logger.LogWarning("Unsupported file type for text extraction: {Extension}", extension);
                    return string.Empty;
            }
        }

        private async Task<string> ExtractTextFromPdfAsync(string filePath)
        {
            var stringBuilder = new StringBuilder();
            using (var pdf = PdfDocument.Open(filePath))
            {
                foreach (var page in pdf.GetPages())
                {
                    stringBuilder.AppendLine($"--- Page {page.Number} ---");
                    stringBuilder.AppendLine(page.Text);

                    foreach (var image in page.GetImages())
                    {
                        try 
                        {
                            var imageData = image.RawBytes.ToArray();
                            if (imageData != null && imageData.Length > 0)
                            {
                                var ocrText = await _imageProcessingService.ProcessImageAsync(imageData, "Extract all text from this image.");
                                var description = await _imageProcessingService.ProcessImageAsync(imageData, "Describe this image in detail.");

                                stringBuilder.AppendLine("--- Image Content ---");
                                if (!string.IsNullOrWhiteSpace(ocrText))
                                {
                                    stringBuilder.AppendLine("Extracted Text:");
                                    stringBuilder.AppendLine(ocrText);
                                }
                                if (!string.IsNullOrWhiteSpace(description))
                                {
                                    stringBuilder.AppendLine("Image Description:");
                                    stringBuilder.AppendLine(description);
                                }
                                stringBuilder.AppendLine("--- End Image Content ---");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning("Failed to process image in PDF page {PageNumber}. Error: {ErrorMessage}", page.Number, ex.Message);
                        }
                    }
                }
            }
            return stringBuilder.ToString();
        }

        private string ExtractTextFromDocx(string filePath)
        {
            using var doc = WordprocessingDocument.Open(filePath, false);
            return doc.MainDocumentPart?.Document.Body?.InnerText ?? string.Empty;
        }
    }
}