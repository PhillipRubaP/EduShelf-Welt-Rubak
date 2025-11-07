using EduShelf.Api.Data;
using EduShelf.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace EduShelf.Api.Services
{
    public class RAGService : IRAGService
    {
        private readonly ApiDbContext _context;
        private readonly IntentDetectionService _intentDetectionService;
        private readonly PdfImageExtractionService _pdfImageExtractionService;
        private readonly ImageProcessingService _imageProcessingService;
        private readonly RetrievalService _retrievalService;
        private readonly PromptGenerationService _promptGenerationService;
        private readonly Kernel _kernel;
        private readonly ILogger<RAGService> _logger;

        public RAGService(
            ApiDbContext context,
            IntentDetectionService intentDetectionService,
            PdfImageExtractionService pdfImageExtractionService,
            ImageProcessingService imageProcessingService,
            RetrievalService retrievalService,
            PromptGenerationService promptGenerationService,
            Kernel kernel,
            ILogger<RAGService> logger)
        {
            _context = context;
            _intentDetectionService = intentDetectionService;
            _pdfImageExtractionService = pdfImageExtractionService;
            _imageProcessingService = imageProcessingService;
            _retrievalService = retrievalService;
            _promptGenerationService = promptGenerationService;
            _kernel = kernel;
            _logger = logger;
        }

        public async Task<string> GetResponseAsync(string query, int userId)
        {
            var intent = await _intentDetectionService.GetIntentAsync(query);

            if (intent.Type == "DescribeImage" && !string.IsNullOrEmpty(intent.DocumentName))
            {
                return await HandleDescribeImageIntent(intent, userId);
            }

            var (context, sources) = await _retrievalService.GetContextAndSourcesAsync(query, userId);
            var prompt = _promptGenerationService.GeneratePrompt(query, context);
            
            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            var result = await chatCompletionService.GetChatMessageContentAsync(prompt);

            return result.Content ?? "I'm sorry, I couldn't generate a response.";
        }

        private async Task<string> HandleDescribeImageIntent(Intent intent, int userId)
        {
            _logger.LogInformation("Handling DescribeImage intent for document: {DocumentName}", intent.DocumentName);

            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.UserId == userId && d.Title == intent.DocumentName);

            if (document == null)
            {
                return $"I could not find the document named '{intent.DocumentName}'.";
            }

            var filePath = Path.Combine("Uploads", document.FilePath);
            if (!File.Exists(filePath))
            {
                return $"I'm sorry, the file for document '{intent.DocumentName}' is missing.";
            }

            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var images = _pdfImageExtractionService.ExtractImages(fileStream);

            if (!images.Any())
            {
                return $"I did not find any images in the document '{intent.DocumentName}'.";
            }

            var firstImage = images.FirstOrDefault();
            if (firstImage == null)
            {
                return "I found images, but could not process them.";
            }

            using var imageStream = new MemoryStream(firstImage);
            var description = await _imageProcessingService.ProcessImageAsync(imageStream, "Describe this image in detail.");

            return description ?? "I was able to extract an image, but I could not generate a description for it.";
        }
    }
}