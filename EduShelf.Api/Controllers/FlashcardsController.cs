using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EduShelf.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ApiExplorerSettings(GroupName = "Flashcards")]
    public class FlashcardsController : ControllerBase
    {
        private readonly IFlashcardService _flashcardService;

        public FlashcardsController(IFlashcardService flashcardService)
        {
            _flashcardService = flashcardService;
        }

        // GET: api/Flashcards
        [HttpGet]
        public async Task<ActionResult<PagedResult<FlashcardDto>>> GetFlashcards([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = GetUserId();
            var isAdmin = IsAdmin();
            var flashcards = await _flashcardService.GetFlashcardsAsync(userId, isAdmin, page, pageSize);
            return Ok(flashcards);
        }

        // GET: api/Flashcards/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FlashcardDto>> GetFlashcard(int id)
        {
            var userId = GetUserId();
            var isAdmin = IsAdmin();
            var flashcard = await _flashcardService.GetFlashcardAsync(id, userId, isAdmin);
            return Ok(flashcard);
        }

        // GET: api/Flashcards/tag/5
        [HttpGet("tag/{tagId}")]
        public async Task<ActionResult<PagedResult<FlashcardDto>>> GetFlashcardsByTag(int tagId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = GetUserId();
            var isAdmin = IsAdmin();
            var flashcards = await _flashcardService.GetFlashcardsByTagAsync(tagId, userId, isAdmin, page, pageSize);
            return Ok(flashcards);
        }

        // POST: api/Flashcards
        [HttpPost]
        public async Task<ActionResult<FlashcardDto>> PostFlashcard(FlashcardCreateDto flashcardDto)
        {
            var userId = GetUserId();
            var createdFlashcard = await _flashcardService.CreateFlashcardAsync(flashcardDto, userId);
            return CreatedAtAction(nameof(GetFlashcard), new { id = createdFlashcard.Id }, createdFlashcard);
        }

        // POST: api/Flashcards/generate
        [HttpPost("generate")]
        public async Task<ActionResult<List<FlashcardDto>>> GenerateFlashcards([FromBody] GenerateFlashcardsRequest request)
        {
            var userId = GetUserId();
            var generatedFlashcards = await _flashcardService.GenerateFlashcardsAsync(request, userId);
            return Ok(generatedFlashcards);
        }

        // PUT: api/Flashcards/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFlashcard(int id, FlashcardCreateDto flashcardDto)
        {
            var userId = GetUserId();
            var isAdmin = IsAdmin();
            await _flashcardService.UpdateFlashcardAsync(id, flashcardDto, userId, isAdmin);
            return NoContent();
        }

        // PATCH: api/Flashcards/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchFlashcard(int id, FlashcardUpdateDto flashcardUpdate)
        {
            var userId = GetUserId();
            var isAdmin = IsAdmin();
            await _flashcardService.PatchFlashcardAsync(id, flashcardUpdate, userId, isAdmin);
            return NoContent();
        }

        // DELETE: api/Flashcards/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFlashcard(int id)
        {
            var userId = GetUserId();
            var isAdmin = IsAdmin();
            await _flashcardService.DeleteFlashcardAsync(id, userId, isAdmin);
            return NoContent();
        }

        private int GetUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                throw new System.UnauthorizedAccessException("Invalid user identifier.");
            }
            return userId;
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }
    }


}