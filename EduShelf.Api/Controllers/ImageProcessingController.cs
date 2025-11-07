using EduShelf.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EduShelf.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageProcessingController : ControllerBase
    {
        private readonly ImageProcessingService _imageProcessingService;

        public ImageProcessingController(ImageProcessingService imageProcessingService)
        {
            _imageProcessingService = imageProcessingService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file, string prompt)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            if (string.IsNullOrEmpty(prompt))
            {
                return BadRequest("A prompt is required.");
            }

            using var stream = file.OpenReadStream();
            var result = await _imageProcessingService.ProcessImageAsync(stream, prompt);

            return Ok(new { response = result });
        }
    }
}