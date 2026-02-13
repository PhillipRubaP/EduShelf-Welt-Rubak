using EduShelf.Api.Data;
using EduShelf.Api.Exceptions;
using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using EduShelf.Api.Constants;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json; // Added for parsing logic

namespace EduShelf.Api.Services
{
    public class QuizService : IQuizService
    {
        private readonly ApiDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly IConfiguration _configuration;
        private readonly IDocumentService _documentService;
        private readonly ILogger<QuizService> _logger;

        public QuizService(
            ApiDbContext context, 
            IHttpContextAccessor httpContextAccessor, 
            Kernel kernel, 
            IConfiguration configuration,
            IDocumentService documentService,
            ILogger<QuizService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            _configuration = configuration;
            _documentService = documentService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated or user ID is invalid.");
            }
            return userId;
        }

        private bool IsCurrentUserAdmin()
        {
            return _httpContextAccessor.HttpContext?.User.IsInRole(Roles.Admin) ?? false;
        }

        public async Task<PagedResult<QuizDto>> GetQuizzesAsync(int page, int pageSize)
        {
            var userId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();

            var query = _context.Quizzes.AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(q => q.UserId == userId);
            }

            var totalCount = await query.CountAsync();
            var quizzes = await query
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .OrderByDescending(q => q.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = quizzes.Select(MapToDto).ToList();
            return new PagedResult<QuizDto>(dtos, totalCount, page, pageSize);
        }

        public async Task<QuizDto> GetQuizAsync(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
            {
                throw new NotFoundException($"Quiz with ID {id} not found.");
            }

            var userId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();

            if (!isAdmin && quiz.UserId != userId)
            {
                throw new ForbidException("You are not authorized to access this quiz.");
            }

            return MapToDto(quiz);
        }

        public async Task<QuizDto> CreateQuizAsync(QuizCreateDto quizDto)
        {
            var userId = GetCurrentUserId();

            var newQuiz = new Quiz
            {
                Title = quizDto.Title,
                UserId = userId,
                Questions = quizDto.Questions?.Select(q => new Question
                {
                    Text = q.Text,
                    Answers = q.Answers?.Select(a => new Answer
                    {
                        Text = a.Text,
                        IsCorrect = a.IsCorrect
                    }).ToList() ?? new List<Answer>()
                }).ToList() ?? new List<Question>()
            };

            _context.Quizzes.Add(newQuiz);
            await _context.SaveChangesAsync();

            return MapToDto(newQuiz);
        }

        public async Task<QuizDto> UpdateQuizAsync(int id, QuizUpdateDto quizUpdateDto)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
            {
                throw new NotFoundException($"Quiz with ID {id} not found.");
            }

            var userId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();

            if (!isAdmin && quiz.UserId != userId)
            {
                throw new ForbidException("You are not authorized to update this quiz.");
            }

            quiz.Title = quizUpdateDto.Title;

            // --- Question Management ---
            var questionIdsFromDto = new HashSet<int>(quizUpdateDto.Questions.Where(q => q.Id != 0).Select(q => q.Id));
            var questionsToRemove = quiz.Questions.Where(q => !questionIdsFromDto.Contains(q.Id)).ToList();
            _context.Questions.RemoveRange(questionsToRemove);

            foreach (var questionDto in quizUpdateDto.Questions)
            {
                Question? question;
                if (questionDto.Id != 0) // Existing Question
                {
                    question = quiz.Questions.FirstOrDefault(q => q.Id == questionDto.Id);
                    if (question == null) continue; // Should practically not happen if ID matches context of quiz, or could warn/error
                    question.Text = questionDto.Text;
                }
                else // New Question
                {
                    question = new Question { Text = questionDto.Text, Answers = new List<Answer>() };
                    quiz.Questions.Add(question);
                }

                // --- Answer Management ---
                var answerIdsFromDto = new HashSet<int>(questionDto.Answers.Where(a => a.Id != 0).Select(a => a.Id));
                var answersToRemove = question.Answers.Where(a => !answerIdsFromDto.Contains(a.Id)).ToList();
                _context.Answers.RemoveRange(answersToRemove);

                foreach (var answerDto in questionDto.Answers)
                {
                    if (answerDto.Id != 0) // Existing Answer
                    {
                        var answer = question.Answers.FirstOrDefault(a => a.Id == answerDto.Id);
                        if (answer == null) continue;
                        answer.Text = answerDto.Text;
                        answer.IsCorrect = answerDto.IsCorrect;
                    }
                    else // New Answer
                    {
                        question.Answers.Add(new Answer { Text = answerDto.Text, IsCorrect = answerDto.IsCorrect });
                    }
                }
            }

            await _context.SaveChangesAsync();
            return MapToDto(quiz);
        }

        public async Task PatchQuizAsync(int id, QuizUpdateDto quizUpdate)
        {
             var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
            {
                throw new NotFoundException($"Quiz with ID {id} not found.");
            }

            var userId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();

            if (!isAdmin && quiz.UserId != userId)
            {
               throw new ForbidException("You are not authorized to update this quiz.");
            }

            if (!string.IsNullOrEmpty(quizUpdate.Title))
            {
                quiz.Title = quizUpdate.Title;
            }

            if (quizUpdate.Questions != null)
            {
                foreach (var questionDto in quizUpdate.Questions)
                {
                    var question = quiz.Questions.FirstOrDefault(q => q.Id == questionDto.Id);
                    if (question != null)
                    {
                        if (questionDto.Text != null)
                        {
                            question.Text = questionDto.Text;
                        }

                        if (questionDto.Answers != null)
                        {
                            foreach (var answerDto in questionDto.Answers)
                            {
                                var answer = question.Answers.FirstOrDefault(a => a.Id == answerDto.Id);
                                if (answer != null)
                                {
                                    if (answerDto.Text != null)
                                    {
                                        answer.Text = answerDto.Text;
                                    }
                                    answer.IsCorrect = answerDto.IsCorrect;
                                }
                            }
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteQuizAsync(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
            {
                throw new NotFoundException($"Quiz with ID {id} not found.");
            }

            var userId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();

            if (!isAdmin && quiz.UserId != userId)
            {
               throw new ForbidException("You are not authorized to delete this quiz.");
            }

            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();
        }

        public async Task<QuizDto> GenerateQuizAsync(GenerateQuizRequest request, int userId)
        {
            // 1. Retrieve Document Content
            string documentContent = "";
            string documentTitle = "Generated Quiz";

            if (!string.IsNullOrWhiteSpace(request.Context))
            {
                documentContent = request.Context;
                documentTitle = "Context Generated Quiz";
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
                    catch (Exception)
                    {
                         // Log but continue
                    }
                }
                documentContent = string.Join("\n\n__NEXT_DOCUMENT__\n\n", docs);
                documentTitle = $"Multi-Doc Quiz ({request.DocumentIds.Count})";
            }
            else if (request.DocumentId.HasValue)
            {
                 try 
                {
                    var doc = await _documentService.GetDocumentAsync(request.DocumentId.Value, userId, Roles.Student);
                    documentTitle = doc.Title + " Quiz";
                    documentContent = await _documentService.GetDocumentContentAsync(request.DocumentId.Value, userId, Roles.Student);
                } 
                catch (Exception)
                {
                     // Handle or log
                     throw new ArgumentException("Could not retrieve document content for generation.");
                }
            }
            else
            {
                 throw new BadRequestException("No context or document source provided.");
            }

            if (string.IsNullOrWhiteSpace(documentContent))
            {
                throw new ArgumentException("Document content is empty or unreadable.");
            }

            // Limit content size roughly to fit context window if needed, but assuming model handles or truncates 4096 tokens.
            if (documentContent.Length > 15000)
            {
                documentContent = documentContent.Substring(0, 15000); // Rough truncation
            }

            // 2. Prepare AI Prompt
            var promptTemplate = _configuration.GetValue<string>("AIService:Prompts:Quiz");
            if (string.IsNullOrEmpty(promptTemplate))
            {
                 throw new InvalidOperationException("Quiz prompt is not configured.");
            }

            // 3. Call AI
            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(promptTemplate.Replace("{Count}", request.Count.ToString()));
            chatHistory.AddUserMessage($"[Text Analysis]:\n{documentContent}\n\n---\n\nIMPORTANT: Based on the text above, generate the JSON quiz now. Output strictly valid JSON. Do not include any conversational text, markdown, or explanations. Start with {{.");

            var result = await _chatCompletionService.GetChatMessageContentAsync(chatHistory);
            var rawJson = result.Content ?? "{}";
            _logger.LogInformation("AI Generated Quiz JSON Raw: {Json}", rawJson);

            // 4. Parse JSON
            var cleanedJson = EduShelf.Api.Helpers.JsonHelper.ExtractJson(rawJson);
            GeneratedQuizJson generatedQuiz;
            try
            {
                generatedQuiz = ParseQuizResponse(cleanedJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse generated quiz JSON. Cleaned JSON: {CleanedJson}", cleanedJson);
                throw new InvalidOperationException("AI failed to generate valid JSON for the quiz.", ex);
            }
            
            if (generatedQuiz == null) throw new InvalidOperationException("Generated quiz is null.");

            // 5. Save Quiz
            // Use the title from AI or fallback
            if (string.IsNullOrWhiteSpace(generatedQuiz.Title) || generatedQuiz.Title == "Geography Quiz")
            {
                generatedQuiz.Title = documentTitle;
            }

            var newQuiz = new Quiz
            {
                Title = generatedQuiz.Title,
                UserId = userId,
                Questions = generatedQuiz.Questions.Select(q => new Question
                {
                    Text = q.Text,
                    Answers = q.Answers.Select(a => new Answer
                    {
                        Text = a.Text,
                        IsCorrect = a.IsCorrect
                    }).ToList()
                }).ToList()
            };

            _context.Quizzes.Add(newQuiz);
            await _context.SaveChangesAsync();

            return MapToDto(newQuiz);
        }

        private GeneratedQuizJson ParseQuizResponse(string cleanedJson)
        {
            try
            {
                var jsonNode = System.Text.Json.Nodes.JsonNode.Parse(cleanedJson);
                if (jsonNode == null) return new GeneratedQuizJson();

                // Handle 'quiz' Wrapper
                if (jsonNode["quiz"] != null)
                {
                    jsonNode = jsonNode["quiz"];
                }
                
                // Attempt standard deserialization first
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                GeneratedQuizJson? result = null;
                try 
                {
                    result = jsonNode.Deserialize<GeneratedQuizJson>(options);
                }
                catch { }

                // If standard worked and has questions with text, return it
                if (result != null && result.Questions != null && result.Questions.Any() && !string.IsNullOrEmpty(result.Questions[0].Text))
                {
                    return result;
                }

                // Fallback: Manual Parsing for "All Lowercase / Alternative Schema"
                var manualQuiz = new GeneratedQuizJson();
                manualQuiz.Title = jsonNode?["title"]?.ToString() ?? result?.Title ?? "Generated Quiz";
                
                var questionsNode = jsonNode["questions"] ?? jsonNode["Questions"];
                if (questionsNode is System.Text.Json.Nodes.JsonArray qArray)
                {
                    manualQuiz.Questions = new List<GeneratedQuestionJson>();
                    foreach (var qItem in qArray)
                    {
                        var qText = qItem?["text"]?.ToString() ?? qItem?["question"]?.ToString();
                        var answerStr = qItem?["answer"]?.ToString(); // Correct answer if string
                        
                        var newQ = new GeneratedQuestionJson { Text = qText ?? "Unknown Question" };
                        
                        var optsNode = qItem?["answers"] ?? qItem?["options"];
                        if (optsNode is System.Text.Json.Nodes.JsonArray optsArray)
                        {
                            newQ.Answers = new List<GeneratedAnswerJson>();
                            foreach (var opt in optsArray)
                            {
                                string? optText = null;
                                bool isCorrect = false;

                                if (opt is System.Text.Json.Nodes.JsonValue) // ["Option A", "Option B"]
                                {
                                    optText = opt.ToString();
                                    if (!string.IsNullOrEmpty(answerStr) && optText.Trim().Equals(answerStr.Trim(), StringComparison.OrdinalIgnoreCase))
                                    {
                                        isCorrect = true;
                                    }
                                }
                                else if (opt is System.Text.Json.Nodes.JsonObject) // [{"text": "...", "isCorrect": ...}]
                                {
                                    optText = opt["text"]?.ToString() ?? opt["Text"]?.ToString();
                                    // Try to find boolean
                                    var isCorrectNode = opt["isCorrect"] ?? opt["IsCorrect"];
                                    if (isCorrectNode != null)
                                    {
                                        bool.TryParse(isCorrectNode.ToString(), out isCorrect);
                                    }
                                }

                                if (optText != null)
                                {
                                    newQ.Answers.Add(new GeneratedAnswerJson { Text = optText, IsCorrect = isCorrect });
                                }
                            }
                        }
                        manualQuiz.Questions.Add(newQ);
                    }
                }

                return manualQuiz;
            }
            catch (Exception ex)
            {
                // Log via throw
                throw new InvalidOperationException("Failed to manually parse quiz JSON structure.", ex);
            }
        }

        private static QuizDto MapToDto(Quiz quiz)
        {
            return new QuizDto
            {
                Id = quiz.Id,
                Title = quiz.Title,
                CreatedAt = quiz.CreatedAt,
                Questions = quiz.Questions.Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    Answers = q.Answers.Select(a => new AnswerDto
                    {
                        Id = a.Id,
                        Text = a.Text,
                        IsCorrect = a.IsCorrect
                    }).ToList()
                }).ToList()
            };
        }
    }
}
