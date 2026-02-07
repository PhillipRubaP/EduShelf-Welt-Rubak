using System.ComponentModel.DataAnnotations;

namespace EduShelf.Api.Models.Dtos
{
    public class GenerateFlashcardsRequest
    {
        [Required]
        public int DocumentId { get; set; }

        [Range(1, 20)]
        public int Count { get; set; } = 5;
    }
}
