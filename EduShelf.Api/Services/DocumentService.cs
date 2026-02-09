using EduShelf.Api.Data;
using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Models.Entities;
using EduShelf.Api.Services.FileStorage;
using EduShelf.Api.Exceptions;
using EduShelf.Api.Constants;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Docnet.Core;
using UglyToad.PdfPig;
using System.Text;
using System.IO;

using Tag = EduShelf.Api.Models.Entities.Tag;

using EduShelf.Api.Services.Background;
using Microsoft.Extensions.DependencyInjection;

namespace EduShelf.Api.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly ApiDbContext _context;
        private readonly IBackgroundJobQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IImageProcessingService _imageProcessingService;
        private readonly IFileStorageService _fileStorageService;

        public DocumentService(
            ApiDbContext context, 
            IBackgroundJobQueue queue,
            IServiceScopeFactory scopeFactory,
            IImageProcessingService imageProcessingService, 
            IFileStorageService fileStorageService)
        {
            _context = context;
            _queue = queue;
            _scopeFactory = scopeFactory;
            _imageProcessingService = imageProcessingService;
            _fileStorageService = fileStorageService;
        }

        public async Task<PagedResult<DocumentDto>> GetDocumentsAsync(int userId, string role, int page, int pageSize)
        {
            var query = _context.Documents.AsQueryable();

            if (role != Roles.Admin)
            {
                // Get owned documents OR shared documents
                query = query
                    .Include(d => d.DocumentTags)
                    .ThenInclude(dt => dt.Tag)
                    .Include(d => d.User) // Include owner info
                    .Where(d => d.UserId == userId || _context.DocumentShares.Any(ds => ds.DocumentId == d.Id && ds.UserId == userId));
            }
            else
            {
                query = query
                    .Include(d => d.DocumentTags)
                    .ThenInclude(dt => dt.Tag);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(d => d.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DocumentDto
                {
                    Id = d.Id,
                    Title = d.Title,
                    FileType = d.FileType,
                    CreatedAt = d.CreatedAt,
                    Tags = d.DocumentTags.Select(dt => new TagDto { Id = dt.Tag.Id, Name = dt.Tag.Name }).ToList(),
                    UserId = d.UserId,
                    IsShared = d.UserId != userId,
                    OwnerName = d.UserId != userId && d.User != null ? d.User.Username : null
                })
                .ToListAsync();

            return new PagedResult<DocumentDto>(items, totalCount, page, pageSize);
        }

        public async Task<DocumentDto> GetDocumentAsync(int id, int userId, string role)
        {
            var document = await _context.Documents
                .Include(d => d.DocumentTags)
                .ThenInclude(dt => dt.Tag)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null)
            {
                throw new NotFoundException("Document not found.");
            }

            var isShared = await _context.DocumentShares.AnyAsync(ds => ds.DocumentId == id && ds.UserId == userId);

            if (role != Roles.Admin && document.UserId != userId && !isShared)
            {
                throw new ForbidException("You are not authorized to view this document.");
            }

            return new DocumentDto
            {
                Id = document.Id,
                Title = document.Title,
                FileType = document.FileType,
                CreatedAt = document.CreatedAt,
                Tags = document.DocumentTags.Select(dt => new TagDto { Id = dt.Tag.Id, Name = dt.Tag.Name }).ToList(),
                UserId = document.UserId,
                IsShared = document.UserId != userId,
                OwnerName = (document.UserId != userId && document.User != null) ? document.User.Username : null
            };
        }

        public async Task<DocumentDto> UploadDocumentAsync(Stream fileStream, string fileName, string contentType, int userId, List<string> tags)
        {
            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".pdf", ".docx", ".txt", ".doc" };

            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new BadRequestException("Invalid file type. Only .pdf, .docx, .txt and .doc files are allowed.");
            }

            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

            await _fileStorageService.UploadFileAsync(fileStream, uniqueFileName, contentType);

            var document = new Models.Entities.Document
            {
                UserId = userId,
                Title = Path.GetFileNameWithoutExtension(fileName),
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
            // Queue background indexing
            Console.WriteLine($"[DocumentService] Queuing background indexing for document {document.Id} ({document.Path})");
            await _queue.QueueBackgroundWorkItemAsync(async token =>
            {
                Console.WriteLine($"[BackgroundJob] Starting background work item for document {document.Id}");
                using var scope = _scopeFactory.CreateScope();
                var indexingService = scope.ServiceProvider.GetRequiredService<IndexingService>();
                try
                {
                    await indexingService.IndexDocumentAsync(document.Id, document.Path);
                    Console.WriteLine($"[BackgroundJob] Completed indexing for document {document.Id}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BackgroundJob] Error indexing document {document.Id}: {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                }
            });

            return new DocumentDto
            {
                Id = document.Id,
                Title = document.Title,
                FileType = document.FileType,
                CreatedAt = document.CreatedAt,
                Tags = document.DocumentTags?.Select(dt => new TagDto { Id = dt.Tag.Id, Name = dt.Tag.Name }).ToList() ?? new List<TagDto>(),
                UserId = document.UserId
            };
        }

        public async Task UpdateDocumentAsync(int id, Models.Entities.Document documentUpdate, int userId, string role)
        {
            var document = await _context.Documents.FindAsync(id);

            if (document == null)
            {
                throw new NotFoundException("Document not found.");
            }

            if (role != Roles.Admin && document.UserId != userId)
            {
                throw new ForbidException("You are not authorized to update this document.");
            }

            if (id != documentUpdate.Id)
            {
                throw new BadRequestException("ID mismatch.");
            }

            document.Title = documentUpdate.Title;
            // FileType and Path usually not updated here

            await _context.SaveChangesAsync();
        }

        public async Task DeleteDocumentAsync(int id, int userId, string role)
        {
            var document = await _context.Documents.FindAsync(id);

            if (document == null)
            {
                throw new NotFoundException("Document not found.");
            }

            if (role != Roles.Admin && document.UserId != userId)
            {
                throw new ForbidException("You are not authorized to delete this document.");
            }

            try
            {
                await _fileStorageService.DeleteFileAsync(document.Path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file {document.Path}: {ex.Message}");
            }

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Tag>> GetTagsForDocumentAsync(int documentId)
        {
            var documentExists = await _context.Documents.AnyAsync(d => d.Id == documentId);
            if (!documentExists)
            {
                throw new NotFoundException("Document not found.");
            }

            return await _context.DocumentTags
                .Where(dt => dt.DocumentId == documentId)
                .Select(dt => dt.Tag)
                .ToListAsync();
        }

        public async Task AddTagToDocumentAsync(int documentId, int tagId)
        {
            var documentExists = await _context.Documents.AnyAsync(d => d.Id == documentId);
            if (!documentExists)
            {
                throw new NotFoundException("Document not found.");
            }

            var tagExists = await _context.Tags.AnyAsync(t => t.Id == tagId);
            if (!tagExists)
            {
                throw new NotFoundException("Tag not found.");
            }

            var documentTagExists = await _context.DocumentTags.AnyAsync(dt => dt.DocumentId == documentId && dt.TagId == tagId);
            if (documentTagExists)
            {
                throw new ConflictException("This tag is already associated with the document.");
            }

            var documentTag = new DocumentTag
            {
                DocumentId = documentId,
                TagId = tagId
            };

            _context.DocumentTags.Add(documentTag);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTagsForDocumentAsync(int documentId, List<int> tagIds)
        {
             var document = await _context.Documents
                .Include(d => d.DocumentTags)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
            {
                 throw new NotFoundException("Document not found.");
            }

            var validTags = await _context.Tags.Where(t => tagIds.Contains(t.Id)).ToListAsync();
            if (validTags.Count != tagIds.Count)
            {
                throw new BadRequestException("One or more tags are invalid.");
            }

            document.DocumentTags.Clear();
            foreach (var tagId in tagIds)
            {
                document.DocumentTags.Add(new DocumentTag { DocumentId = documentId, TagId = tagId });
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemoveTagFromDocumentAsync(int documentId, int tagId)
        {
             var documentTag = await _context.DocumentTags
                .FirstOrDefaultAsync(dt => dt.DocumentId == documentId && dt.TagId == tagId);

            if (documentTag == null)
            {
                 throw new NotFoundException("Tag association not found.");
            }

            _context.DocumentTags.Remove(documentTag);
            await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<DocumentDto>> SearchDocumentsAsync(string querySearch, int userId, string role, int page, int pageSize)
        {
            var documentsQuery = _context.Documents.AsQueryable();

            if (!string.IsNullOrEmpty(querySearch))
            {
                documentsQuery = documentsQuery.Where(d => d.Title.Contains(querySearch));
            }

            if (role != Roles.Admin)
            {
                 documentsQuery = documentsQuery
                    .Include(d => d.DocumentTags)
                    .ThenInclude(dt => dt.Tag)
                    .Include(d => d.User)
                    .Where(d => (d.UserId == userId || _context.DocumentShares.Any(ds => ds.DocumentId == d.Id && ds.UserId == userId)));
            }
            else
            {
                documentsQuery = documentsQuery
                    .Include(d => d.DocumentTags)
                    .ThenInclude(dt => dt.Tag);
            }

            var totalCount = await documentsQuery.CountAsync();
            var items = await documentsQuery
                .OrderByDescending(d => d.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DocumentDto
                {
                    Id = d.Id,
                    Title = d.Title,
                    FileType = d.FileType,
                    CreatedAt = d.CreatedAt,
                    Tags = d.DocumentTags.Select(dt => new TagDto { Id = dt.Tag.Id, Name = dt.Tag.Name }).ToList(),
                    UserId = d.UserId,
                    IsShared = d.UserId != userId,
                    OwnerName = d.UserId != userId && d.User != null ? d.User.Username : null
                })
                .ToListAsync();

            return new PagedResult<DocumentDto>(items, totalCount, page, pageSize);
        }

        public async Task<(Stream FileStream, string ContentType, string FileName)> DownloadDocumentAsync(int id, int userId, string role)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                throw new NotFoundException("Document not found.");
            }

            var isShared = await _context.DocumentShares.AnyAsync(ds => ds.DocumentId == id && ds.UserId == userId);
            
            if (role != Roles.Admin && document.UserId != userId && !isShared)
            {
                throw new ForbidException("You are not authorized to download this document.");
            }

            var fileStream = await _fileStorageService.DownloadFileAsync(document.Path);
            if (fileStream == null)
            {
                throw new NotFoundException("File not found in storage.");
            }

            return (fileStream, GetContentType(document.Path), document.Title + "." + document.FileType);
        }

        public async Task<string> GetDocumentContentAsync(int id, int userId, string role)
        {
            var document = await _context.Documents.FindAsync(id);

            if (document == null)
            {
               throw new NotFoundException("Document not found.");
            }

            var isShared = await _context.DocumentShares.AnyAsync(ds => ds.DocumentId == id && ds.UserId == userId);

            if (role != Roles.Admin && document.UserId != userId && !isShared)
            {
                throw new ForbidException("You are not authorized to view this document.");
            }

            using var fileStream = await _fileStorageService.DownloadFileAsync(document.Path);
            if (fileStream == null)
            {
                 throw new NotFoundException("File not found in storage.");
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
                    var body = wordDoc.MainDocumentPart?.Document?.Body;
                    if (body != null)
                    {
                        foreach (var p in body.Elements<Paragraph>())
                        {
                            stringBuilder.AppendLine(p.InnerText);
                        }
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
                throw new BadRequestException("Unsupported file type for content extraction.");
            }

            return content;
        }

        public async Task UpdateDocumentContentAsync(int id, DocumentContentUpdateDto documentUpdateDto, int userId, string role)
        {
            var document = await _context.Documents.FindAsync(id);

            if (document == null)
            {
                 throw new NotFoundException("Document not found.");
            }

            if (role != Roles.Admin && document.UserId != userId)
            {
                throw new ForbidException("You are not authorized to update this document.");
            }

            using var originalStream = await _fileStorageService.DownloadFileAsync(document.Path);
            if (originalStream == null)
            {
                 throw new NotFoundException("File not found in storage.");
            }

            var workingStream = new MemoryStream();
            await originalStream.CopyToAsync(workingStream);
            workingStream.Position = 0;

            var fileExtension = Path.GetExtension(document.Path).ToLowerInvariant();
            var contentType = GetContentType(document.Path);

            if (fileExtension == ".pdf")
            {
                throw new BadRequestException("Direct content update for PDF files is not supported.");
            }
            else if (fileExtension == ".docx")
            {
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
                 throw new BadRequestException("Unsupported file type for content update.");
            }

            // Fire and forget re-indexing
            // Queue background re-indexing
            await _queue.QueueBackgroundWorkItemAsync(async token =>
            {
                using var scope = _scopeFactory.CreateScope();
                var indexingService = scope.ServiceProvider.GetRequiredService<IndexingService>();
                try
                {
                    await indexingService.IndexDocumentAsync(document.Id, document.Path);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error re-indexing document {document.Id}: {ex.Message}");
                }
            });
        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            if (types.ContainsKey(ext)) return types[ext];
            return "application/octet-stream";
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
        public async Task ShareDocumentAsync(int documentId, string emailOrUsername, int currentUserId)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null)
            {
                throw new NotFoundException("Document not found.");
            }

            if (document.UserId != currentUserId) // Only owner can share
            {
                throw new ForbidException("You are not authorized to share this document.");
            }

            var targetUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == emailOrUsername || u.Username == emailOrUsername);

            if (targetUser == null)
            {
                throw new NotFoundException("User not found.");
            }

            if (targetUser.UserId == currentUserId)
            {
                throw new BadRequestException("You cannot share a document with yourself.");
            }

            var alreadyShared = await _context.DocumentShares
                .AnyAsync(ds => ds.DocumentId == documentId && ds.UserId == targetUser.UserId);

            if (alreadyShared)
            {
                throw new ConflictException("Document is already shared with this user.");
            }

            var share = new DocumentShare
            {
                DocumentId = documentId,
                UserId = targetUser.UserId,
                SharedAt = DateTime.UtcNow
            };

            _context.DocumentShares.Add(share);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAllDocumentsForUserAsync(int userId)
        {
            var documents = await _context.Documents
                .Where(d => d.UserId == userId)
                .ToListAsync();

            foreach (var document in documents)
            {
                try
                {
                    await _fileStorageService.DeleteFileAsync(document.Path);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting file for user deletion: {ex.Message}");
                    // Continue deleting other files and the user record
                }
            }
            
            // Database records will be deleted via Cascade Delete on User
        }
    }
}
