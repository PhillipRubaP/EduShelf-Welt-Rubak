using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Models.Entities;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EduShelf.Api.Services
{
    public interface IDocumentService
    {
        Task<PagedResult<DocumentDto>> GetDocumentsAsync(int userId, string role, int page, int pageSize);
        Task<DocumentDto> GetDocumentAsync(int id, int userId, string role);
        Task<DocumentDto> UploadDocumentAsync(Stream fileStream, string fileName, string contentType, int userId, List<string> tags);
        Task UpdateDocumentAsync(int id, Document document, int userId, string role);
        Task DeleteDocumentAsync(int id, int userId, string role);
        
        Task<IEnumerable<Tag>> GetTagsForDocumentAsync(int documentId);
        Task AddTagToDocumentAsync(int documentId, int tagId);
        Task UpdateTagsForDocumentAsync(int documentId, List<string> tags);
        Task RemoveTagFromDocumentAsync(int documentId, int tagId);
        
        Task<PagedResult<DocumentDto>> SearchDocumentsAsync(string query, int userId, string role, int page, int pageSize, string? tag = null);
        
        Task<(Stream FileStream, string ContentType, string FileName)> DownloadDocumentAsync(int id, int userId, string role);
        Task<string> GetDocumentContentAsync(int id, int userId, string role);
        Task UpdateDocumentContentAsync(int id, DocumentContentUpdateDto contentDto, int userId, string role);
        Task ShareDocumentAsync(int documentId, string emailOrUsername, int currentUserId);
        Task DeleteAllDocumentsForUserAsync(int userId);
    }
}
