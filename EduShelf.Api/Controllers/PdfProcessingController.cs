using EduShelf.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EduShelf.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfProcessingController : ControllerBase
    {
        private readonly PdfImageExtractionService _pdfImageExtractionService;
        private readonly ImageProcessingService _imageProcessingService;

        public PdfProcessingController(
            PdfImageExtractionService pdfImageExtractionService,
            ImageProcessingService imageProcessingService)
        {
            _pdfImageExtractionService = pdfImageExtractionService;
            _imageProcessingService = imageProcessingService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadPdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            if (Path.GetExtension(file.FileName).ToLower() != ".pdf")
            {
                return BadRequest("Invalid file type. Please upload a PDF.");
            }

            var allResults = new List<object>();
            int imageIndex = 0;

            using (var pdfStream = file.OpenReadStream())
            {
                var images = _pdfImageExtractionService.ExtractImages(pdfStream);

                if (!images.Any())
                {
                    return Ok(new { message = "No images found in the PDF." });
                }

                foreach (var imageData in images)
                {
                    using (var imageStream = new MemoryStream(imageData))
                    {
                        var results = await _imageProcessingService.ProcessImageAsync(imageStream);
                        allResults.Add(new { image = imageIndex++, results });
                    }
                }
            }

            return Ok(allResults);
        }
    }
}