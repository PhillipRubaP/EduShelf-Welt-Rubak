using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EduShelf.Api.Models.Dtos;

public class FlashcardCreateDto
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public string Question { get; set; }

    [Required]
    public string Answer { get; set; }

    public ICollection<string> Tags { get; set; }
}