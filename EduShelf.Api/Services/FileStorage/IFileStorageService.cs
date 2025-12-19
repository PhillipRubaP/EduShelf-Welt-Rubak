using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace EduShelf.Api.Services.FileStorage
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
        Task<Stream> DownloadFileAsync(string fileName);
        Task DeleteFileAsync(string fileName);
        Task<bool> FileExistsAsync(string fileName);
        string GetFileUrl(string fileName);
    }
}
