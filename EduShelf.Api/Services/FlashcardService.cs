using EduShelf.Api.Data;
using EduShelf.Api.Exceptions;
using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EduShelf.Api.Constants;

namespace EduShelf.Api.Services
{
    public class FlashcardService : IFlashcardService
    {
        private readonly ApiDbContext _context;
        private readonly Kernel _kernel;
        private readonly IDocumentService _documentService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FlashcardService> _logger;

        public FlashcardService(
            ApiDbContext context,
            Kernel kernel,
            IDocumentService documentService,
            IConfiguration configuration,
            ILogger<FlashcardService> logger)
        {
            _context = context;
            _kernel = kernel;
            _documentService = documentService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IEnumerable<FlashcardDto>> GetFlashcardsAsync(int userId, bool isAdmin)
        {
            var query = _context.Flashcards.AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(f => f.UserId == userId);
            }

            return await query
                .Include(f => f.FlashcardTags)
                .ThenInclude(ft => ft.Tag)
                .Select(f => MapToDto(f))
                .ToListAsync();
        }

        public async Task<FlashcardDto> GetFlashcardAsync(int id, int userId, bool isAdmin)
        {
            var flashcard = await _context.Flashcards
                .Include(f => f.FlashcardTags)
                .ThenInclude(ft => ft.Tag)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (flashcard == null)
            {
                throw new NotFoundException("Flashcard not found.");
            }

            if (!isAdmin && flashcard.UserId != userId)
            {
                throw new ForbidException("You are not authorized to access this flashcard.");
            }

            return MapToDto(flashcard);
        }

        public async Task<IEnumerable<FlashcardDto>> GetFlashcardsByTagAsync(int tagId, int userId, bool isAdmin)
        {
            var query = _context.Flashcards.AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(f => f.UserId == userId);
            }

            return await query
                .Where(f => f.FlashcardTags.Any(ft => ft.TagId == tagId))
                .Include(f => f.FlashcardTags)
                .ThenInclude(ft => ft.Tag)
                .Select(f => MapToDto(f))
                .ToListAsync();
        }

        public async Task<FlashcardDto> CreateFlashcardAsync(FlashcardCreateDto flashcardDto, int userId)
        {
            var flashcard = new Flashcard
            {
                UserId = userId,
                Question = flashcardDto.Question,
                Answer = flashcardDto.Answer
            };

            if (flashcardDto.Tags != null)
            {
                foreach (var tagName in flashcardDto.Tags)
                {
                    var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                    if (tag == null)
                    {
                        tag = new Tag { Name = tagName };
                        _context.Tags.Add(tag);
                    }
                    flashcard.FlashcardTags.Add(new FlashcardTag { Tag = tag });
                }
            }

            _context.Flashcards.Add(flashcard);
            await _context.SaveChangesAsync();

            return MapToDto(flashcard);
        }

        // Note: For Update, we might want to return nullable or handle differently, but here keeping consistent with Controller for now logic-wise
        // The Interface defined Task<IActionResult> which was a mistake, let's fix that to be cleaner.
        // Returning updated DTO is standard.
        public async Task<FlashcardDto> UpdateFlashcardAsync(int id, FlashcardCreateDto flashcardDto, int userId, bool isAdmin)
        {
            var flashcard = await _context.Flashcards
                .Include(f => f.FlashcardTags)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (flashcard == null)
            {
                throw new NotFoundException("Flashcard not found.");
            }

            if (!isAdmin && flashcard.UserId != userId)
            {
                throw new ForbidException("You are not authorized to update this flashcard.");
            }

            flashcard.Question = flashcardDto.Question;
            flashcard.Answer = flashcardDto.Answer;

            flashcard.FlashcardTags.Clear();
            if (flashcardDto.Tags != null)
            {
                foreach (var tagName in flashcardDto.Tags)
                {
                    var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                    if (tag == null)
                    {
                        tag = new Tag { Name = tagName };
                        _context.Tags.Add(tag);
                    }
                    flashcard.FlashcardTags.Add(new FlashcardTag { Tag = tag });
                }
            }

            await _context.SaveChangesAsync();
            return MapToDto(flashcard);
        }

        public async Task PatchFlashcardAsync(int id, FlashcardUpdateDto flashcardUpdate, int userId, bool isAdmin)
        {
            var flashcard = await _context.Flashcards
                .Include(f => f.FlashcardTags)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (flashcard == null)
            {
                throw new NotFoundException("Flashcard not found.");
            }

            if (!isAdmin && flashcard.UserId != userId)
            {
                throw new ForbidException("You are not authorized to update this flashcard.");
            }

            if (!string.IsNullOrEmpty(flashcardUpdate.Question))
            {
                flashcard.Question = flashcardUpdate.Question;
            }

            if (!string.IsNullOrEmpty(flashcardUpdate.Answer))
            {
                flashcard.Answer = flashcardUpdate.Answer;
            }

            if (flashcardUpdate.Tags != null)
            {
                flashcard.FlashcardTags.Clear();
                foreach (var tagName in flashcardUpdate.Tags)
                {
                    var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                    if (tag == null)
                    {
                        tag = new Tag { Name = tagName };
                        _context.Tags.Add(tag);
                    }
                    flashcard.FlashcardTags.Add(new FlashcardTag { Tag = tag });
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteFlashcardAsync(int id, int userId, bool isAdmin)
        {
            var flashcard = await _context.Flashcards.FindAsync(id);
            if (flashcard == null)
            {
                throw new NotFoundException("Flashcard not found.");
            }

            if (!isAdmin && flashcard.UserId != userId)
            {
                throw new ForbidException("You are not authorized to delete this flashcard.");
            }

            _context.Flashcards.Remove(flashcard);
            await _context.SaveChangesAsync();
        }

        // --- AI Generation Logic ---

        public async Task<List<FlashcardDto>> GenerateFlashcardsAsync(GenerateFlashcardsRequest request, int userId)
        {
            // 1. Retrieve Document Content
            // We use the admin role internally here to allow the service to fetch content, 
            // but we must check permissions first.
            // Actually DocumentService.GetDocumentContentAsync checks permissions if we pass the role.
            // We'll pass "User" role to enforce permissions.
            string documentContent;
            string documentTitle;
            try 
            {
                var doc = await _documentService.GetDocumentAsync(request.DocumentId, userId, Roles.Student);
                documentTitle = doc.Title;
                
                documentContent = await _documentService.GetDocumentContentAsync(request.DocumentId, userId, Roles.Student);
            } 
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document content for flashcard generation.");
                throw; // Rethrow to let controller handle (e.g. NotFound, Forbid)
            }

            if (string.IsNullOrWhiteSpace(documentContent))
            {
                 throw new BadRequestException("The document has no extractable content.");
            }

            // Limit content length to prevent token overflow (simple truncation for now)
            // A more robust solution would be chunking, but for flashcards, the first N chars usually contain enough context.
            const int maxChars = 12000; 
            if (documentContent.Length > maxChars)
            {
                documentContent = documentContent.Substring(0, maxChars);
            }

            // 2. Prepare AI Prompt
            var promptTemplate = _configuration.GetValue<string>("AIService:Prompts:Flashcard");
            if (string.IsNullOrEmpty(promptTemplate))
            {
                throw new InvalidOperationException("Flashcard prompt is not configured.");
            }

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(promptTemplate.Replace("{Count}", request.Count.ToString()));
            chatHistory.AddUserMessage($"Text to analyze:\n\n{documentContent}");

            // 3. Call AI
            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            var result = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
            var rawContent = result.Content ?? string.Empty;
            _logger.LogInformation("AI Generated Flashcards JSON: {Json}", rawContent);

            // 4. Parse JSON
            var cleanedJson = CleanJson(rawContent);
            List<GeneratedFlashcardJson> generatedFlashcards;
            try
            {
                generatedFlashcards = JsonSerializer.Deserialize<List<GeneratedFlashcardJson>>(cleanedJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse generated flashcards JSON.");
                throw new KernelServiceException("Failed to generate valid flashcards.", ex);
            }

            if (generatedFlashcards == null || !generatedFlashcards.Any())
            {
                 throw new KernelServiceException("AI returned no flashcards.");
            }

            // 5. Save to Database
            var savedFlashcards = new List<FlashcardDto>();
            
            // Get or Create Tag for the document
            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == documentTitle);
            if (tag == null)
            {
                tag = new Tag { Name = documentTitle };
                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();
            }

            foreach (var gen in generatedFlashcards)
            {
                var flashcard = new Flashcard
                {
                    UserId = userId,
                    Question = gen.Question,
                    Answer = gen.Answer
                };
                
                flashcard.FlashcardTags.Add(new FlashcardTag { Tag = tag });
                
                _context.Flashcards.Add(flashcard);
                // We add to the list but ID is 0
            }

            await _context.SaveChangesAsync();

            // After SaveChanges, the entities in _context are updated with IDs.
            // We can retrieve them from the Local tracker or re-query if needed.
            // Since we just added them, they are in Local. We filter by the ones we just added.
            // A safer way is to just map the entities we created in the loop, as their IDs are now populated.
            
            // Re-fetch the entities we just created from the context tracker
            // Ideally we would keep the entity references from the loop.
            // Let's refactor the loop slightly to keep references.
            
            return _context.Flashcards.Local
                .Where(f => f.UserId == userId && f.FlashcardTags.Any(ft => ft.TagId == tag.Id))
                .OrderByDescending(f => f.Id)
                .Take(generatedFlashcards.Count)
                .Select(MapToDto)
                .ToList();
        }

        private static string CleanJson(string raw)
        {
            var cleaned = raw.Trim();
            if (cleaned.StartsWith("```json"))
            {
                cleaned = cleaned.Substring(7);
            }
            if (cleaned.StartsWith("```"))
            {
                cleaned = cleaned.Substring(3);
            }
            if (cleaned.EndsWith("```"))
            {
                cleaned = cleaned.Substring(0, cleaned.Length - 3);
            }
            return cleaned.Trim();
        }

        private static FlashcardDto MapToDto(Flashcard flashcard)
        {
            return new FlashcardDto
            {
                Id = flashcard.Id,
                UserId = flashcard.UserId,
                Question = flashcard.Question,
                Answer = flashcard.Answer,
                CreatedAt = flashcard.CreatedAt,
                Tags = flashcard.FlashcardTags?.Select(ft => ft.Tag.Name).ToList() ?? new List<string>()
            };
        }
    }
}
