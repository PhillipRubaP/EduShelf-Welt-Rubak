using System.Collections.Generic;
using System.IO;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace EduShelf.Api.Services
{
    public class PdfImageExtractionService
    {
        public IEnumerable<byte[]> ExtractImages(Stream pdfStream)
        {
            var images = new List<byte[]>();

            using (var document = PdfDocument.Open(pdfStream))
            {
                foreach (var page in document.GetPages())
                {
                    foreach (var image in page.GetImages())
                    {
                        // TryGetContent is preferred to avoid exceptions for certain image types
                        if (image.TryGetContent(out var bytes))
                        {
                            images.Add(bytes.ToArray());
                        }
                    }
                }
            }

            return images;
        }
    }
}