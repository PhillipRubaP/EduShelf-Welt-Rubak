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
using Docnet.Core;
using Docnet.Core.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using EduShelf.Api.Services.FileStorage;

namespace EduShelf.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ApiExplorerSettings(GroupName = "Documents")]
    public class DocumentsController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly IndexingService _indexingService;
        private readonly IImageProcessingService _imageProcessingService;
        private readonly IFileStorageService _fileStorageService;

        public DocumentsController(ApiDbContext context, IndexingService indexingService, IImageProcessingService imageProcessingService, IFileStorageService fileStorageService)
        {
            _context = context;
            _indexingService = indexingService;
            _imageProcessingService = imageProcessingService;
            _fileStorageService = fileStorageService;
        }

        // GET: api/Documents
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentDto>>> GetDocuments()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid user identifier.");
            }

            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var query = _context.Documents.AsQueryable();

            if (userRole != "Admin")
            {
                query = query.Where(d => d.UserId == userId);
            }

            var documents = await query
                .Include(d => d.DocumentTags)
                .ThenInclude(dt => dt.Tag)
                .Select(d => new DocumentDto
                {
                    Id = d.Id,
                    Title = d.Title,
                    FileType = d.FileType,
                    CreatedAt = d.CreatedAt,
                    Tags = d.DocumentTags.Select(dt => new TagDto { Id = dt.Tag.Id, Name = dt.Tag.Name }).ToList()
                })
                .ToListAsync();

            return Ok(documents);
        }

        // GET: api/Documents/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentDto>> GetDocument(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid user identifier.");
            }

            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var document = await _context.Documents
                .Include(d => d.DocumentTags)
                .ThenInclude(dt => dt.Tag)
                .Where(d => d.Id == id)
                .Select(d => new DocumentDto
                {
                    Id = d.Id,
                    Title = d.Title,
                    FileType = d.FileType,
                    CreatedAt = d.CreatedAt,
                    Tags = d.DocumentTags.Select(dt => new TagDto { Id = dt.Tag.Id, Name = dt.Tag.Name }).ToList(),
                    UserId = d.UserId
                })
                .FirstOrDefaultAsync();

            if (document != null && userRole != "Admin" && document.UserId != userId)
            {
                return Forbid();
            }

            if (document == null)
            {
                return NotFound();
            }

            return Ok(document);
        }

        // POST: api/Documents
        [HttpPost]
        public async Task<ActionResult<Models.Entities.Document>> PostDocument([FromForm] IFormFile file, [FromForm] int userId, [FromForm] List<string> tags)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var originalFileName = Path.GetFileName(file.FileName);
            var fileExtension = Path.GetExtension(originalFileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".pdf", ".docx", ".txt", ".doc" };

            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest("Invalid file type. Only .pdf, .docx, .txt and .doc files are allowed.");
            }

            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            
            using (var stream = file.OpenReadStream())
            {
                await _fileStorageService.UploadFileAsync(stream, uniqueFileName, file.ContentType);
            }

            var document = new Models.Entities.Document
            {
                UserId = userId,
                Title = Path.GetFileNameWithoutExtension(originalFileName),
                Path = uniqueFileName,
                FileType = fileExtension.TrimStart('.')
            };

            if (tags != null && tags.Any())
            {
                document.DocumentTags = new List<DocumentTag>();
                foreach (var tagName in tags)
                {
                    var normalizedTagName = tagName.Trim().ToLower();
                    var existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == normalizedTagName);

                    if (existingTag == null)
                    {
                        existingTag = new Models.Entities.Tag { Name = normalizedTagName };
                        _context.Tags.Add(existingTag);
                    }

                    document.DocumentTags.Add(new DocumentTag { Tag = existingTag });
                }
            }

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
        public async Task<IActionResult> PutDocument(int id, Models.Entities.Document document)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid user identifier.");
            }

            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var doc = await _context.Documents.FindAsync(id);

            if (doc == null)
            {
                return NotFound();
            }

            if (userRole != "Admin" && doc.UserId != userId)
            {
                return Forbid();
            }

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
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid user identifier.");
            }

            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var document = await _context.Documents.FindAsync(id);

            if (document == null)
            {
                return NotFound();
            }

            if (userRole != "Admin" && document.UserId != userId)
            {
                return Forbid();
            }

            try
            {
                await _fileStorageService.DeleteFileAsync(document.Path);
            }
            catch (Exception ex)
            {
                // Verify if file exists logic needed? DeleteFileAsync usually idempotent or throws if not found
                // MinIO RemoveObject is idempotent.
                // Just logging or ignoring might be safer to ensure DB deletion.
                Console.WriteLine($"Error deleting file {document.Path}: {ex.Message}");
            }

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            return NoContent();
        }
// GET: api/Documents/5/tags
        [HttpGet("{documentId}/tags")]
        public async Task<ActionResult<IEnumerable<Models.Entities.Tag>>> GetTagsForDocument(int documentId)
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

        // PUT: api/Documents/5/tags
        [HttpPut("{documentId}/tags")]
        public async Task<IActionResult> UpdateTagsForDocument(int documentId, [FromBody] List<int> tagIds)
        {
            var document = await _context.Documents
                .Include(d => d.DocumentTags)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
            {
                return NotFound("Document not found.");
            }

            var tags = await _context.Tags.Where(t => tagIds.Contains(t.Id)).ToListAsync();
            if (tags.Count != tagIds.Count)
            {
                return BadRequest("One or more tags are invalid.");
            }

            document.DocumentTags.Clear();

            foreach (var tagId in tagIds)
            {
                document.DocumentTags.Add(new DocumentTag { DocumentId = documentId, TagId = tagId });
            }

            await _context.SaveChangesAsync();

            return NoContent();
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

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<DocumentDto>>> SearchDocuments([FromQuery] string query)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid user identifier.");
            }

            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var documentsQuery = _context.Documents.AsQueryable();

            if (userRole != "Admin")
            {
                documentsQuery = documentsQuery.Where(d => d.UserId == userId);
            }

            if (!string.IsNullOrEmpty(query))
            {
                documentsQuery = documentsQuery.Where(d => d.Title.Contains(query));
            }

            var documents = await documentsQuery
                .Include(d => d.DocumentTags)
                .ThenInclude(dt => dt.Tag)
                .Select(d => new DocumentDto
                {
                    Id = d.Id,
                    Title = d.Title,
                    FileType = d.FileType,
                    CreatedAt = d.CreatedAt,
                    Tags = d.DocumentTags.Select(dt => new TagDto { Id = dt.Tag.Id, Name = dt.Tag.Name }).ToList()
                })
                .ToListAsync();

            return Ok(documents);
        }


        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"}
            };
        }

        private bool DocumentExists(int id)
        {
            return _context.Documents.Any(e => e.Id == id);
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadDocument(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            var fileStream = await _fileStorageService.DownloadFileAsync(document.Path);
            if (fileStream == null)
            {
                return NotFound("File not found in storage.");
            }

            return File(fileStream, GetContentType(document.Path), document.Title + "." + document.FileType);
        }
        
        [HttpGet("{id}/content")]
        public async Task<IActionResult> GetDocumentContent(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid user identifier.");
            }

            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var document = await _context.Documents.FindAsync(id);

            if (document == null)
            {
                return NotFound();
            }

            if (userRole != "Admin" && document.UserId != userId)
            {
                return Forbid();
            }

            using var fileStream = await _fileStorageService.DownloadFileAsync(document.Path);
            if (fileStream == null)
            {
                return NotFound("File not found in storage.");
            }

            string content;
            var fileExtension = Path.GetExtension(document.Path).ToLowerInvariant();

            if (fileExtension == ".pdf")
            {
                var stringBuilder = new StringBuilder();
                using (var pdf = PdfDocument.Open(fileStream))
                {
                    foreach (var page in pdf.GetPages())
                    {
                        stringBuilder.AppendLine($"--- Page {page.Number} ---");
                        stringBuilder.AppendLine(page.Text);

                        foreach (var image in page.GetImages())
                        {
                            var imageData = image.RawBytes.ToArray();
                            if (imageData != null && imageData.Length > 0)
                            {
                                var ocrText = await _imageProcessingService.ProcessImageAsync(imageData, "Extract all text from this image.");
                                var description = await _imageProcessingService.ProcessImageAsync(imageData, "Describe this image in detail.");

                                stringBuilder.AppendLine("--- Image Content ---");
                                if (!string.IsNullOrWhiteSpace(ocrText))
                                {
                                    stringBuilder.AppendLine("Extracted Text:");
                                    stringBuilder.AppendLine(ocrText);
                                }
                                if (!string.IsNullOrWhiteSpace(description))
                                {
                                    stringBuilder.AppendLine("Image Description:");
                                    stringBuilder.AppendLine(description);
                                }
                                stringBuilder.AppendLine("--- End Image Content ---");
                            }
                        }
                    }
                }
                content = stringBuilder.ToString();
            }
            else if (fileExtension == ".docx" || fileExtension == ".doc")
            {
                using (var wordDoc = WordprocessingDocument.Open(fileStream, false))
                {
                    var stringBuilder = new StringBuilder();
                    foreach (var p in wordDoc.MainDocumentPart.Document.Body.Elements<Paragraph>())
                    {
                        stringBuilder.AppendLine(p.InnerText);
                    }
                    content = stringBuilder.ToString();
                }
            }
            else if (fileExtension == ".txt")
            {
                using (var reader = new StreamReader(fileStream))
                {
                    content = await reader.ReadToEndAsync();
                }
            }
            else
            {
                return BadRequest("Unsupported file type for content extraction.");
            }

            return Ok(content);
        }

        [HttpGet("{id}/raw")]
        public async Task<IActionResult> GetDocumentRaw(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid user identifier.");
            }

            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var document = await _context.Documents.FindAsync(id);

            if (document == null)
            {
                return NotFound();
            }

            if (userRole != "Admin" && document.UserId != userId)
            {
                return Forbid();
            }

            var fileStream = await _fileStorageService.DownloadFileAsync(document.Path);
            if (fileStream == null)
            {
                 return NotFound("File not found in storage.");
            }
            
            return File(fileStream, GetContentType(document.Path), document.Title + "." + document.FileType);
        }

        [HttpPatch("{id}/content")]
        public async Task<IActionResult> UpdateDocumentContent(int id, [FromBody] DocumentContentUpdateDto documentUpdateDto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid user identifier.");
            }

            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var document = await _context.Documents.FindAsync(id);

            if (document == null)
            {
                return NotFound();
            }

            if (userRole != "Admin" && document.UserId != userId)
            {
                return Forbid();
            }

            using var originalStream = await _fileStorageService.DownloadFileAsync(document.Path);
            if (originalStream == null)
            {
                return NotFound("File not found in storage.");
            }

            // Copy to a memory stream that we can modify and reuse
            var workingStream = new MemoryStream();
            await originalStream.CopyToAsync(workingStream);
            workingStream.Position = 0;

            var fileExtension = Path.GetExtension(document.Path).ToLowerInvariant();
            var contentType = GetContentType(document.Path);

            if (fileExtension == ".pdf")
            {
                return BadRequest("Direct content update for PDF files is not supported.");
            }
            else if (fileExtension == ".docx")
            {
                // WordprocessingDocument closes the stream on Dispose.
                // To persist changes, we must allow it to close this specific stream wrapper, 
                // but we need the data.
                // Strategy: Use the memory stream, let it close, then use ToArray() from the closed stream?
                // MemoryStream.ToArray() works even after Close().
                
                using (var wordDoc = WordprocessingDocument.Open(workingStream, true))
                {
                    var mainPart = wordDoc.MainDocumentPart;
                    if (mainPart == null)
                    {
                        mainPart = wordDoc.AddMainDocumentPart();
                        mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
                    }
                    mainPart.Document.Body = new Body(new Paragraph(new Run(new Text(documentUpdateDto.Content))));
                    mainPart.Document.Save();
                }

                // workingStream is now closed, but we can extract data as array
                var modifiedData = workingStream.ToArray();
                using var uploadStream = new MemoryStream(modifiedData);
                await _fileStorageService.UploadFileAsync(uploadStream, document.Path, contentType);
            }
            else if (fileExtension == ".txt")
            {
                var bytes = Encoding.UTF8.GetBytes(documentUpdateDto.Content);
                using var uploadStream = new MemoryStream(bytes);
                await _fileStorageService.UploadFileAsync(uploadStream, document.Path, contentType);
            }
            else
            {
                return BadRequest("Unsupported file type for content update.");
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    await _indexingService.IndexDocumentAsync(document.Id, document.Path);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error re-indexing document {document.Id}: {ex.Message}");
                }
            });

            return NoContent();
        }

    }
}