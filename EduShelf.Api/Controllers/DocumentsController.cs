using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Models.Entities;
using EduShelf.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using EduShelf.Api.Constants;

namespace EduShelf.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ApiExplorerSettings(GroupName = "Documents")]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        // GET: api/Documents
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentDto>>> GetDocuments()
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var documents = await _documentService.GetDocumentsAsync(userId, userRole);
            return Ok(documents);
        }

        // GET: api/Documents/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentDto>> GetDocument(int id)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var document = await _documentService.GetDocumentAsync(id, userId, userRole);
            return Ok(document);
        }

        // POST: api/Documents
        [HttpPost]
        public async Task<ActionResult<DocumentDto>> PostDocument([FromForm] IFormFile file, [FromForm] int userId, [FromForm] List<string> tags)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            using var stream = file.OpenReadStream();
            var documentDto = await _documentService.UploadDocumentAsync(stream, file.FileName, file.ContentType, userId, tags);
            
            return CreatedAtAction("GetDocument", new { id = documentDto.Id }, documentDto);
        }

        // PUT: api/Documents/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDocument(int id, Document document)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            await _documentService.UpdateDocumentAsync(id, document, userId, userRole);
            return NoContent();
        }

        // DELETE: api/Documents/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            await _documentService.DeleteDocumentAsync(id, userId, userRole);
            return NoContent();
        }

        // GET: api/Documents/5/tags
        [HttpGet("{documentId}/tags")]
        public async Task<ActionResult<IEnumerable<Tag>>> GetTagsForDocument(int documentId)
        {
            var tags = await _documentService.GetTagsForDocumentAsync(documentId);
            return Ok(tags);
        }
        
        // POST: api/Documents/5/tags
        [HttpPost("{documentId}/tags")]
        public async Task<IActionResult> AddTagToDocument(int documentId, [FromBody] int tagId)
        {
            await _documentService.AddTagToDocumentAsync(documentId, tagId);
            return Ok();
        }

        // PUT: api/Documents/5/tags
        [HttpPut("{documentId}/tags")]
        public async Task<IActionResult> UpdateTagsForDocument(int documentId, [FromBody] List<int> tagIds)
        {
             await _documentService.UpdateTagsForDocumentAsync(documentId, tagIds);
             return NoContent();
        }
    
        // DELETE: api/Documents/5/tags/2
        [HttpDelete("{documentId}/tags/{tagId}")]
        public async Task<IActionResult> RemoveTagFromDocument(int documentId, int tagId)
        {
            await _documentService.RemoveTagFromDocumentAsync(documentId, tagId);
            return NoContent();
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<DocumentDto>>> SearchDocuments([FromQuery] string query)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var documents = await _documentService.SearchDocumentsAsync(query, userId, userRole);
            return Ok(documents);
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadDocument(int id)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var (fileStream, contentType, fileName) = await _documentService.DownloadDocumentAsync(id, userId, userRole);
            return File(fileStream, contentType, fileName);
        }
        
        [HttpGet("{id}/content")]
        public async Task<IActionResult> GetDocumentContent(int id)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var content = await _documentService.GetDocumentContentAsync(id, userId, userRole);
            return Ok(content);
        }

        [HttpGet("{id}/raw")]
        public async Task<IActionResult> GetDocumentRaw(int id)
        {
            // Raw is essentially same as download for viewing in browser if MIME type supported
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var (fileStream, contentType, fileName) = await _documentService.DownloadDocumentAsync(id, userId, userRole);
            return File(fileStream, contentType, fileName);
        }

        [HttpPatch("{id}/content")]
        public async Task<IActionResult> UpdateDocumentContent(int id, [FromBody] DocumentContentUpdateDto documentUpdateDto)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            await _documentService.UpdateDocumentContentAsync(id, documentUpdateDto, userId, userRole);
            return NoContent();
        }

        // Helper methods
        private int GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                throw new System.UnauthorizedAccessException("Invalid user identifier.");
            }
            return userId;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirstValue(ClaimTypes.Role) ?? "";
        }
    }
}