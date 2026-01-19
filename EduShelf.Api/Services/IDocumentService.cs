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
        Task<IEnumerable<DocumentDto>> GetDocumentsAsync(int userId, string role);
        Task<DocumentDto> GetDocumentAsync(int id, int userId, string role);
        Task<DocumentDto> UploadDocumentAsync(Stream fileStream, string fileName, string contentType, int userId, List<string> tags);
        Task UpdateDocumentAsync(int id, Document document, int userId, string role);
        Task DeleteDocumentAsync(int id, int userId, string role);
        
        Task<IEnumerable<Tag>> GetTagsForDocumentAsync(int documentId);
        Task AddTagToDocumentAsync(int documentId, int tagId);
        Task UpdateTagsForDocumentAsync(int documentId, List<int> tagIds);
        Task RemoveTagFromDocumentAsync(int documentId, int tagId);
        
        Task<IEnumerable<DocumentDto>> SearchDocumentsAsync(string query, int userId, string role);
        
        Task<(Stream FileStream, string ContentType, string FileName)> DownloadDocumentAsync(int id, int userId, string role);
        Task<string> GetDocumentContentAsync(int id, int userId, string role);
        Task UpdateDocumentContentAsync(int id, DocumentContentUpdateDto contentDto, int userId, string role);
    }
}
