using System;
using System.Collections.Generic;

namespace EduShelf.Api.Models.Dtos;

public class FlashcardDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string Question { get; set; }
    public required string Answer { get; set; }
    public DateTime CreatedAt { get; set; }
    public required ICollection<string> Tags { get; set; }
}