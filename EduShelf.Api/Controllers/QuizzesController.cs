using EduShelf.Api.Exceptions;
using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;

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

        // POST: api/Quizzes/generate
        [HttpPost("generate")]
        public async Task<ActionResult<QuizDto>> GenerateQuiz([FromBody] GenerateQuizRequest request)
        {
            // GetUserId not implemented in Controller like in FlashcardController? 
            // Ah, I see GetUserId private method is missing in this controller, but User.FindFirstValue is standard.
            // Let's implement helper or use inline.
            var userIdString = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var generatedQuiz = await _quizService.GenerateQuizAsync(request, userId);
            return CreatedAtAction(nameof(GetQuiz), new { id = generatedQuiz.Id }, generatedQuiz);
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