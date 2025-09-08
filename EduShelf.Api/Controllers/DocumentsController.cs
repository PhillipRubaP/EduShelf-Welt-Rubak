using EduShelf.Api.Data;
using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Models.Entities;
using EduShelf.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
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
    public class DocumentsController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly IndexingService _indexingService;
        private readonly string _uploadPath;

        public DocumentsController(ApiDbContext context, IndexingService indexingService, IConfiguration configuration)
        {
            _context = context;
            _indexingService = indexingService;
            _uploadPath = configuration["FileStorage:UploadPath"] ?? "Uploads";
        }

        // GET: api/Documents
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentDto>>> GetDocuments()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized();
            }

            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var documents = await _context.Documents
                .Where(d => d.UserId == userId)
                .Select(d => new DocumentDto
                {
                    Id = d.Id,
                    Title = d.Title,
                    FileType = d.FileType,
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync();

            return Ok(documents);
        }

        // GET: api/Documents/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Document>> GetDocument(int id)
        {
            var document = await _context.Documents.FindAsync(id);

            if (document == null)
            {
                return NotFound();
            }

            return document;
        }

        // POST: api/Documents
        [HttpPost]
        public async Task<ActionResult<Document>> PostDocument([FromForm] IFormFile file, [FromForm] int userId)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), _uploadPath);
            if (!Directory.Exists(uploadsFolderPath))
            {
                Directory.CreateDirectory(uploadsFolderPath);
            }

            var originalFileName = Path.GetFileName(file.FileName);
            var fileExtension = Path.GetExtension(originalFileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".pdf", ".docx", ".txt" };

            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest("Invalid file type. Only .pdf, .docx, and .txt files are allowed.");
            }

            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var document = new Document
            {
                UserId = userId,
                Title = Path.GetFileNameWithoutExtension(originalFileName),
                Path = uniqueFileName,
                FileType = fileExtension.TrimStart('.')
            };


            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            // Fire and forget the indexing process.
            _ = Task.Run(async () =>
            {
                try
                {
                    await _indexingService.IndexDocumentAsync(document.Id, document.Path);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error indexing document {document.Id}: {ex.Message}");
                }
            });

            return CreatedAtAction("GetDocument", new { id = document.Id }, document);
        }

        // PUT: api/Documents/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDocument(int id, Document document)
        {
            if (id != document.Id)
            {
                return BadRequest();
            }

            _context.Entry(document).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DocumentExists(id))
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

        // DELETE: api/Documents/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            return NoContent();
        }
// GET: api/Documents/5/tags
        [HttpGet("{documentId}/tags")]
        public async Task<ActionResult<IEnumerable<Tag>>> GetTagsForDocument(int documentId)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null)
            {
                return NotFound("Document not found.");
            }

            var tags = await _context.DocumentTags
                .Where(dt => dt.DocumentId == documentId)
                .Select(dt => dt.Tag)
                .ToListAsync();

            return Ok(tags);
        }
        
        // POST: api/Documents/5/tags
        [HttpPost("{documentId}/tags")]
        public async Task<IActionResult> AddTagToDocument(int documentId, [FromBody] int tagId)
        {
            var documentExists = await _context.Documents.AnyAsync(d => d.Id == documentId);
            if (!documentExists)
            {
                return NotFound("Document not found.");
            }

            var tagExists = await _context.Tags.AnyAsync(t => t.Id == tagId);
            if (!tagExists)
            {
                return NotFound("Tag not found.");
            }

            var documentTagExists = await _context.DocumentTags.AnyAsync(dt => dt.DocumentId == documentId && dt.TagId == tagId);
            if (documentTagExists)
            {
                return Conflict("This tag is already associated with the document.");
            }

            var documentTag = new DocumentTag
            {
                DocumentId = documentId,
                TagId = tagId
            };

            _context.DocumentTags.Add(documentTag);
            await _context.SaveChangesAsync();

            return Ok();
        }

    

        // DELETE: api/Documents/5/tags/2
        [HttpDelete("{documentId}/tags/{tagId}")]
        public async Task<IActionResult> RemoveTagFromDocument(int documentId, int tagId)
        {
            var documentTag = await _context.DocumentTags
                .FirstOrDefaultAsync(dt => dt.DocumentId == documentId && dt.TagId == tagId);

            if (documentTag == null)
            {
                return NotFound("Tag association not found.");
            }

            _context.DocumentTags.Remove(documentTag);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DocumentExists(int id)
        {
            return _context.Documents.Any(e => e.Id == id);
        }
    }
}