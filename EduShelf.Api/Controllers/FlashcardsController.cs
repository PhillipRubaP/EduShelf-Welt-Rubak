using EduShelf.Api.Data;
using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduShelf.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlashcardsController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public FlashcardsController(ApiDbContext context)
        {
            _context = context;
        }

        // GET: api/Flashcards
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FlashcardDto>>> GetFlashcards()
        {
            return await _context.Flashcards
                .Include(f => f.FlashcardTags)
                .ThenInclude(ft => ft.Tag)
                .Select(f => new FlashcardDto
                {
                    Id = f.Id,
                    UserId = f.UserId,
                    Question = f.Question,
                    Answer = f.Answer,
                    CreatedAt = f.CreatedAt,
                    Tags = f.FlashcardTags.Select(ft => ft.Tag.Name).ToList()
                })
                .ToListAsync();
        }

        // GET: api/Flashcards/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FlashcardDto>> GetFlashcard(int id)
        {
            var flashcard = await _context.Flashcards
                .Include(f => f.FlashcardTags)
                .ThenInclude(ft => ft.Tag)
                .Select(f => new FlashcardDto
                {
                    Id = f.Id,
                    UserId = f.UserId,
                    Question = f.Question,
                    Answer = f.Answer,
                    CreatedAt = f.CreatedAt,
                    Tags = f.FlashcardTags.Select(ft => ft.Tag.Name).ToList()
                })
                .FirstOrDefaultAsync(f => f.Id == id);

            if (flashcard == null)
            {
                return NotFound();
            }

            return flashcard;
        }

        // POST: api/Flashcards
        [HttpPost]
        public async Task<ActionResult<FlashcardDto>> PostFlashcard(FlashcardCreateDto flashcardDto)
        {
            var flashcard = new Flashcard
            {
                UserId = flashcardDto.UserId,
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

            var flashcardResultDto = new FlashcardDto
            {
                Id = flashcard.Id,
                UserId = flashcard.UserId,
                Question = flashcard.Question,
                Answer = flashcard.Answer,
                CreatedAt = flashcard.CreatedAt,
                Tags = flashcard.FlashcardTags.Select(ft => ft.Tag.Name).ToList()
            };

            return CreatedAtAction("GetFlashcard", new { id = flashcard.Id }, flashcardResultDto);
        }

        // PUT: api/Flashcards/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFlashcard(int id, FlashcardCreateDto flashcardDto)
        {
            var flashcard = await _context.Flashcards
                .Include(f => f.FlashcardTags)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (flashcard == null)
            {
                return NotFound();
            }

            flashcard.UserId = flashcardDto.UserId;
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

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FlashcardExists(id))
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

        // DELETE: api/Flashcards/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFlashcard(int id)
        {
            var flashcard = await _context.Flashcards.FindAsync(id);
            if (flashcard == null)
            {
                return NotFound();
            }

            _context.Flashcards.Remove(flashcard);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FlashcardExists(int id)
        {
            return _context.Flashcards.Any(e => e.Id == id);
        }
    }
}