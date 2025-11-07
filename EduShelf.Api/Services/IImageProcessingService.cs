using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace EduShelf.Api.Services
{
    public interface IImageProcessingService
    {
        Task<string> ProcessImageAsync(byte[] imageData, string prompt);
    }
}