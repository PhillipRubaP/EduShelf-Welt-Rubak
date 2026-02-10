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

        public async Task<PagedResult<FlashcardDto>> GetFlashcardsAsync(int userId, bool isAdmin, int page, int pageSize)
        {
            var query = _context.Flashcards.AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(f => f.UserId == userId);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .Include(f => f.FlashcardTags)
                .ThenInclude(ft => ft.Tag)
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => MapToDto(f))
                .ToListAsync();

            return new PagedResult<FlashcardDto>(items, totalCount, page, pageSize);
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

        public async Task<PagedResult<FlashcardDto>> GetFlashcardsByTagAsync(int tagId, int userId, bool isAdmin, int page, int pageSize)
        {
            var query = _context.Flashcards.AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(f => f.UserId == userId);
            }

            query = query.Where(f => f.FlashcardTags.Any(ft => ft.TagId == tagId));

            var totalCount = await query.CountAsync();
            var items = await query
                .Include(f => f.FlashcardTags)
                .ThenInclude(ft => ft.Tag)
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => MapToDto(f))
                .ToListAsync();

            return new PagedResult<FlashcardDto>(items, totalCount, page, pageSize);
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
            // 1. Retrieve Document Content or Use Context
            string documentContent = "";
            string documentTitle = "Generated Flashcards";

            if (!string.IsNullOrWhiteSpace(request.Context))
            {
                documentContent = request.Context;
                documentTitle = "Context Generated";
            }
            else if (request.DocumentIds != null && request.DocumentIds.Any())
            {
                var docs = new List<string>();
                foreach (var docId in request.DocumentIds)
                {
                    try
                    {
                        var content = await _documentService.GetDocumentContentAsync(docId, userId, Roles.Student);
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            docs.Add(content);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to retrieve content for document {DocumentId}", docId);
                        // Continue ensuring we get at least some content
                    }
                }
                documentContent = string.Join("\n\n__NEXT_DOCUMENT__\n\n", docs);
                documentTitle = $"Multi-Doc ({request.DocumentIds.Count})";
            }
            else if (request.DocumentId.HasValue)
            {
                 try 
                {
                    var doc = await _documentService.GetDocumentAsync(request.DocumentId.Value, userId, Roles.Student);
                    documentTitle = doc.Title;
                    
                    documentContent = await _documentService.GetDocumentContentAsync(request.DocumentId.Value, userId, Roles.Student);
                } 
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving document content for flashcard generation.");
                    throw; 
                }
            }
            else
            {
                throw new BadRequestException("No context or document source provided.");
            }

            if (string.IsNullOrWhiteSpace(documentContent))
            {
                 throw new BadRequestException("The source content is empty.");
            }

            // Limit content length
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
            chatHistory.AddUserMessage($"Text to analyze:\n\n{documentContent}\n\n---\n\nIMPORTANT: Based on the text above, generate the JSON flashcards now. Output strictly valid JSON. Do not include any conversational text, markdown, or explanations. Start with [.");

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
                generatedFlashcards = JsonSerializer.Deserialize<List<GeneratedFlashcardJson>>(cleanedJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
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
            
            // Get or Create Tag for the document
            // If using Context, we might want a generic tag or a specific one provided in request? 
            // For now, if Context is used, we use "Generated".
            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == documentTitle);
            if (tag == null)
            {
                tag = new Tag { Name = documentTitle };
                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();
            }

            var flashcardsToReturn = new List<FlashcardDto>();

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
                // We need to save changes to get ID, or we can just map what we have and ID will be 0 until save?
                // Returning with ID is better.
            }

            await _context.SaveChangesAsync();

            // Re-fetch or manually map. Manual map uses entities that are now tracked and have IDs.
            // We need to access the entities we just added. 
            // A simple way is to iterate the Local tracker again for these specific additions.
            // Or better, just collect the entities in a list before adding to context.
            
            return _context.Flashcards.Local
                .Where(f => f.UserId == userId && f.FlashcardTags.Any(ft => ft.TagId == tag.Id))
                .OrderByDescending(f => f.Id)
                .Take(generatedFlashcards.Count)
                .Select(MapToDto)
                .ToList();
        }

        private static string CleanJson(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "[]";

            var start = raw.IndexOf('[');
            var end = raw.LastIndexOf(']');

            if (start >= 0 && end > start)
            {
                return raw.Substring(start, end - start + 1);
            }

            return raw.Trim();
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
