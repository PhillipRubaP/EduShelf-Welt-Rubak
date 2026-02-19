using EduShelf.Api.Services.FileStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;
using System.Threading.Tasks;

namespace EduShelf.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IFileStorageService _fileStorageService;

        public ImagesController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        [HttpGet("{fileName}")]
        [AllowAnonymous] // Allow public access to images so they can be easily loaded by <img> tags
        public async Task<IActionResult> GetImage(string fileName)
        {
            var stream = await _fileStorageService.DownloadFileAsync(fileName);
            if (stream == null)
            {
                return NotFound();
            }

            var contentType = GetContentType(fileName);
            return File(stream, contentType);
        }

        private string GetContentType(string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }
    }
}
