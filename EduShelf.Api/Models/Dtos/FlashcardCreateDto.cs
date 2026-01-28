using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EduShelf.Api.Models.Dtos;

public class FlashcardCreateDto
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public required string Question { get; set; }

    [Required]
    public required string Answer { get; set; }

    public ICollection<string> Tags { get; set; } = new List<string>();
}