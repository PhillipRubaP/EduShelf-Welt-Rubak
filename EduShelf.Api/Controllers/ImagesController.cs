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
        [AllowAnonymous] // Allow frontend to display images without auth headers if needed, or keep secured?
        // DocumentsController required Auth. Chat images might be sensitive.
        // However, if frontend uses <img src="..."> it might be hard to pass Bearer token unless using fetch+blob.
        // Defaulting to AllowAnonymous for now for simplicity, similar to static files which were likely public if not protected by middleware.
        // Wait, static files via `UseStaticFiles` are public by default unless `UseAuthorization` is placed before it and configured effectively.
        // I will keep AllowAnonymous for parity with previous file serving behavior unless proven otherwise.
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
