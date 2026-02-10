using System.ComponentModel.DataAnnotations;

namespace EduShelf.Api.Models.Dtos
{
    public class GenerateFlashcardsRequest
    {
        public int? DocumentId { get; set; }
        public List<int>? DocumentIds { get; set; }
        public string? Context { get; set; }

        [Range(1, 20)]
        public int Count { get; set; } = 5;
    }
}
