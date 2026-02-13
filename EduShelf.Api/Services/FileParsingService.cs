using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Docnet.Core;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using EduShelf.Api.Exceptions;
using UglyToad.PdfPig;

namespace EduShelf.Api.Services
{
    public class FileParsingService : IFileParsingService
    {
        private readonly IImageProcessingService _imageProcessingService;

        public FileParsingService(IImageProcessingService imageProcessingService)
        {
            _imageProcessingService = imageProcessingService;
        }

        public async Task<string> ExtractTextAsync(Stream stream, string fileExtension)
        {
            string content;
            var ext = fileExtension.ToLowerInvariant();

            if (ext == ".pdf")
            {
                var stringBuilder = new StringBuilder();
                // PdfPig requires the stream to be readable and seekable. 
                // If it's not seekable, we might need to copy to MemoryStream, but usually FileStorage returns seekable.
                // However, safe to assume we might need a reset position or let the library handle it.
                // The original code passed 'fileStream' directly.
                
                using (var pdf = PdfDocument.Open(stream))
                {
                    foreach (var page in pdf.GetPages())
                    {
                        stringBuilder.AppendLine($"--- Page {page.Number} ---");
                        stringBuilder.AppendLine(page.Text);

                        foreach (var image in page.GetImages())
                        {
                            var imageData = image.RawBytes.ToArray();
                            if (imageData != null && imageData.Length > 0)
                            {
                                var ocrText = await _imageProcessingService.ProcessImageAsync(imageData, "Extract all text from this image.");
                                var description = await _imageProcessingService.ProcessImageAsync(imageData, "Describe this image in detail.");

                                stringBuilder.AppendLine("--- Image Content ---");
                                if (!string.IsNullOrWhiteSpace(ocrText))
                                {
                                    stringBuilder.AppendLine("Extracted Text:");
                                    stringBuilder.AppendLine(ocrText);
                                }
                                if (!string.IsNullOrWhiteSpace(description))
                                {
                                    stringBuilder.AppendLine("Image Description:");
                                    stringBuilder.AppendLine(description);
                                }
                                stringBuilder.AppendLine("--- End Image Content ---");
                            }
                        }
                    }
                }
                content = stringBuilder.ToString();
            }
            else if (ext == ".docx" || ext == ".doc")
            {
                using (var wordDoc = WordprocessingDocument.Open(stream, false))
                {
                    var stringBuilder = new StringBuilder();
                    var body = wordDoc.MainDocumentPart?.Document?.Body;
                    if (body != null)
                    {
                        foreach (var p in body.Elements<Paragraph>())
                        {
                            stringBuilder.AppendLine(p.InnerText);
                        }
                    }
                    content = stringBuilder.ToString();
                }
            }
            else if (ext == ".txt")
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true))
                {
                    content = await reader.ReadToEndAsync();
                }
            }
            else
            {
                throw new BadRequestException("Unsupported file type for content extraction.");
            }

            return content;
        }

        public async Task<Stream> UpdateContentAsync(Stream originalStream, string content, string fileExtension)
        {
            var ext = fileExtension.ToLowerInvariant();
            var workingStream = new MemoryStream();
            await originalStream.CopyToAsync(workingStream);
            workingStream.Position = 0;

            if (ext == ".pdf")
            {
                throw new BadRequestException("Direct content update for PDF files is not supported.");
            }
            else if (ext == ".docx")
            {
                using (var wordDoc = WordprocessingDocument.Open(workingStream, true))
                {
                    var mainPart = wordDoc.MainDocumentPart;
                    if (mainPart == null)
                    {
                        mainPart = wordDoc.AddMainDocumentPart();
                        mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
                    }
                    mainPart.Document.Body = new Body(new Paragraph(new Run(new Text(content))));
                    mainPart.Document.Save();
                }

                // Return a new stream with valid position
                var modifiedData = workingStream.ToArray();
                return new MemoryStream(modifiedData);
            }
            else if (ext == ".txt")
            {
                var bytes = Encoding.UTF8.GetBytes(content);
                return new MemoryStream(bytes);
            }
            else
            {
                throw new BadRequestException("Unsupported file type for content update.");
            }
        }
    }
}
