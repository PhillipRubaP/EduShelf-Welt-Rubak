using EduShelf.Api.Data;
using EduShelf.Api.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

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
        public async Task<ActionResult<IEnumerable<Quiz>>> GetQuizzes()
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

            return await query
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .ToListAsync();
        }

        // GET: api/Quizzes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Quiz>> GetQuiz(int id)
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

            return quiz;
        }

        // POST: api/Quizzes
        [HttpPost]
        public async Task<ActionResult<Quiz>> PostQuiz(Quiz quiz)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid user identifier.");
            }

            var newQuiz = new Quiz
            {
                Title = quiz.Title,
                UserId = userId,
                Questions = quiz.Questions
            };

            _context.Quizzes.Add(newQuiz);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetQuiz", new { id = newQuiz.Id }, newQuiz);
        }

        // PUT: api/Quizzes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuiz(int id, Quiz quiz)
        {
            if (id != quiz.Id)
            {
                return BadRequest();
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid user identifier.");
            }
            var isAdmin = User.IsInRole("Admin");
            var q = await _context.Quizzes.FindAsync(id);

            if (q == null)
            {
                return NotFound();
            }

            if (!isAdmin && q.UserId != userId)
            {
                return Forbid();
            }

            _context.Entry(quiz).State = EntityState.Modified;

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
                var questionIdsToKeep = quizUpdate.Questions.Select(q => q.Id).ToList();
                var questionsToRemove = quiz.Questions.Where(q => !questionIdsToKeep.Contains(q.Id)).ToList();
                _context.Questions.RemoveRange(questionsToRemove);

                foreach (var questionDto in quizUpdate.Questions)
                {
                    var existingQuestion = quiz.Questions.FirstOrDefault(q => q.Id == questionDto.Id);
                    if (existingQuestion != null)
                    {
                        existingQuestion.Text = questionDto.Text;

                        if (questionDto.Answers != null)
                        {
                            var answerIdsToKeep = questionDto.Answers.Select(a => a.Id).ToList();
                            var answersToRemove = existingQuestion.Answers.Where(a => !answerIdsToKeep.Contains(a.Id)).ToList();
                            _context.Answers.RemoveRange(answersToRemove);

                            foreach (var answerDto in questionDto.Answers)
                            {
                                var existingAnswer = existingQuestion.Answers.FirstOrDefault(a => a.Id == answerDto.Id);
                                if (existingAnswer != null)
                                {
                                    existingAnswer.Text = answerDto.Text;
                                    existingAnswer.IsCorrect = answerDto.IsCorrect;
                                }
                                else
                                {
                                    existingQuestion.Answers.Add(new Answer { Text = answerDto.Text, IsCorrect = answerDto.IsCorrect });
                                }
                            }
                        }
                    }
                    else
                    {
                        var newQuestion = new Question
                        {
                            Text = questionDto.Text,
                            Answers = questionDto.Answers?.Select(a => new Answer { Text = a.Text, IsCorrect = a.IsCorrect }).ToList() ?? new List<Answer>()
                        };
                        quiz.Questions.Add(newQuestion);
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

    public class AnswerUpdateDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class QuestionUpdateDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public ICollection<AnswerUpdateDto> Answers { get; set; }
    }

    public class QuizUpdateDto
    {
        public string Title { get; set; }
        public ICollection<QuestionUpdateDto> Questions { get; set; }
    }
}