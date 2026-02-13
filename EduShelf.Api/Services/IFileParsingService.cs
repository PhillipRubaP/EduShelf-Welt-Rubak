using System.IO;
using System.Threading.Tasks;

namespace EduShelf.Api.Services
{
    public interface IFileParsingService
    {
        Task<string> ExtractTextAsync(Stream stream, string fileExtension);
        Task<Stream> UpdateContentAsync(Stream originalStream, string content, string fileExtension);
    }
}
