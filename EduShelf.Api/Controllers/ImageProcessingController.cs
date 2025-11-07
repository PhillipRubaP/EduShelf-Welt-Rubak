using EduShelf.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace EduShelf.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageProcessingController : ControllerBase
    {
        private readonly IImageProcessingService _imageProcessingService;

        public ImageProcessingController(IImageProcessingService imageProcessingService)
        {
            _imageProcessingService = imageProcessingService;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessImage(IFormFile image, [FromForm] string prompt)
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("Image file is required.");
            }

            if (string.IsNullOrEmpty(prompt))
            {
                return BadRequest("A prompt is required.");
            }

            using (var memoryStream = new MemoryStream())
            {
                await image.CopyToAsync(memoryStream);
                var imageData = memoryStream.ToArray();
                var result = await _imageProcessingService.ProcessImageAsync(imageData, prompt);
                return Ok(new { response = result });
            }
        }
    }
}