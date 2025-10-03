using System;
using System.Collections.Generic;

namespace EduShelf.Api.Models.Dtos;

public class FlashcardDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Question { get; set; }
    public string Answer { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<string> Tags { get; set; }
}