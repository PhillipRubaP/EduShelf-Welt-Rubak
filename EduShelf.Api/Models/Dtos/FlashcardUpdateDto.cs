using System.Collections.Generic;

namespace EduShelf.Api.Models.Dtos
{
    public class FlashcardUpdateDto
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
    }
}
