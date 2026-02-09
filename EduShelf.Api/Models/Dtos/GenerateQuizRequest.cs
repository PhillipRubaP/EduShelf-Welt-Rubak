using System.ComponentModel.DataAnnotations;

namespace EduShelf.Api.Models.Dtos
{
    public class GenerateQuizRequest
    {
        [Required]
        public int DocumentId { get; set; }

        [Range(1, 10)]
        public int Count { get; set; } = 5;
    }
}
