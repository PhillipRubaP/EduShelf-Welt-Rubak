using EduShelf.Api.Exceptions;
using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduShelf.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ApiExplorerSettings(GroupName = "Quizzes")]
    public class QuizzesController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public QuizzesController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        // GET: api/Quizzes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuizDto>>> GetQuizzes()
        {
            var quizzes = await _quizService.GetQuizzesAsync();
            return Ok(quizzes);
        }

        // GET: api/Quizzes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<QuizDto>> GetQuiz(int id)
        {
            var quiz = await _quizService.GetQuizAsync(id);
            return Ok(quiz);
        }

        // POST: api/Quizzes
        [HttpPost]
        public async Task<ActionResult<QuizDto>> PostQuiz(QuizCreateDto quizDto)
        {
            var createdQuiz = await _quizService.CreateQuizAsync(quizDto);
            return CreatedAtAction(nameof(GetQuiz), new { id = createdQuiz.Id }, createdQuiz);
        }

        // PUT: api/Quizzes/5
        [HttpPut("{id}")]
        public async Task<ActionResult<QuizDto>> PutQuiz(int id, QuizUpdateDto quizUpdateDto)
        {
            var updatedQuiz = await _quizService.UpdateQuizAsync(id, quizUpdateDto);
            return Ok(updatedQuiz);
        }

        // PATCH: api/Quizzes/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchQuiz(int id, QuizUpdateDto quizUpdate)
        {
            await _quizService.PatchQuizAsync(id, quizUpdate);
            return NoContent();
        }

        // DELETE: api/Quizzes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            await _quizService.DeleteQuizAsync(id);
            return NoContent();
        }
    }
}