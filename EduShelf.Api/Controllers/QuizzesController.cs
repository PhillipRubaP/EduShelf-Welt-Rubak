using EduShelf.Api.Data;
using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EduShelf.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ApiExplorerSettings(GroupName = "Quizzes")]
    public class QuizzesController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public QuizzesController(ApiDbContext context)
        {
            _context = context;
        }

        // GET: api/Quizzes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuizDto>>> GetQuizzes()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid user identifier.");
            }
            var isAdmin = User.IsInRole("Admin");

            var query = _context.Quizzes.AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(q => q.UserId == userId);
            }

            var quizzes = await query
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .ToListAsync();

            var quizDtos = quizzes.Select(q => new QuizDto
            {
                Id = q.Id,
                Title = q.Title,
                CreatedAt = q.CreatedAt,
                Questions = q.Questions.Select(qu => new QuestionDto
                {
                    Id = qu.Id,
                    Text = qu.Text,
                    Answers = qu.Answers.Select(a => new AnswerDto
                    {
                        Id = a.Id,
                        Text = a.Text,
                        IsCorrect = a.IsCorrect
                    }).ToList()
                }).ToList()
            }).ToList();

            return quizDtos;
        }

        // GET: api/Quizzes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<QuizDto>> GetQuiz(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
            {
                return NotFound();
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid user identifier.");
            }
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && quiz.UserId != userId)
            {
                return Forbid();
            }

            var quizDto = new QuizDto
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

            return quizDto;
        }

        // POST: api/Quizzes
        [HttpPost]
        public async Task<ActionResult<QuizDto>> PostQuiz(QuizCreateDto quizDto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid user identifier.");
            }

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

            var resultDto = new QuizDto
            {
                Id = newQuiz.Id,
                Title = newQuiz.Title,
                CreatedAt = newQuiz.CreatedAt,
                Questions = newQuiz.Questions.Select(q => new QuestionDto
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

            return CreatedAtAction("GetQuiz", new { id = newQuiz.Id }, resultDto);
        }

        // PUT: api/Quizzes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuiz(int id, QuizUpdateDto quizUpdateDto)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
            {
                return NotFound();
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid user identifier.");
            }
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && quiz.UserId != userId)
            {
                return Forbid();
            }

            quiz.Title = quizUpdateDto.Title;

            // Remove questions that are not in the DTO
            var questionsToRemove = quiz.Questions
                .Where(q => !quizUpdateDto.Questions.Any(dto => dto.Id == q.Id))
                .ToList();
            _context.Questions.RemoveRange(questionsToRemove);

            foreach (var questionDto in quizUpdateDto.Questions)
            {
                var question = quiz.Questions.FirstOrDefault(q => q.Id == questionDto.Id);
                if (question == null)
                {
                    // Add new question
                    question = new Question { Text = questionDto.Text, Answers = new List<Answer>() };
                    quiz.Questions.Add(question);
                }
                else
                {
                    question.Text = questionDto.Text;
                }

                // Remove answers that are not in the DTO
                var answersToRemove = question.Answers
                    .Where(a => !questionDto.Answers.Any(dto => dto.Id == a.Id))
                    .ToList();
                _context.Answers.RemoveRange(answersToRemove);

                foreach (var answerDto in questionDto.Answers)
                {
                    var answer = question.Answers.FirstOrDefault(a => a.Id == answerDto.Id);
                    if (answer == null)
                    {
                        // Add new answer
                        question.Answers.Add(new Answer { Text = answerDto.Text, IsCorrect = answerDto.IsCorrect });
                    }
                    else
                    {
                        answer.Text = answerDto.Text;
                        answer.IsCorrect = answerDto.IsCorrect;
                    }
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuizExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // PATCH: api/Quizzes/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchQuiz(int id, QuizUpdateDto quizUpdate)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
            {
                return NotFound();
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid user identifier.");
            }
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && quiz.UserId != userId)
            {
                return Forbid();
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

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuizExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        // DELETE: api/Quizzes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
            {
                return NotFound();
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid user identifier.");
            }
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && quiz.UserId != userId)
            {
                return Forbid();
            }

            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool QuizExists(int id)
        {
            return _context.Quizzes.Any(e => e.Id == id);
        }
    }

}