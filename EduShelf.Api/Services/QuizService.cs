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

namespace EduShelf.Api.Services
{
    public class QuizService : IQuizService
    {
        private readonly ApiDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public QuizService(ApiDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
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

        public async Task<IEnumerable<QuizDto>> GetQuizzesAsync()
        {
            var userId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();

            var query = _context.Quizzes.AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(q => q.UserId == userId);
            }

            var quizzes = await query
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .ToListAsync();

            return quizzes.Select(MapToDto).ToList();
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
                Question question;
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
