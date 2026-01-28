using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduShelf.Api.Models.Entities;

public class Flashcard
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public required string Question { get; set; }

    [Required]
    public required string Answer { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    public virtual ICollection<FlashcardTag> FlashcardTags { get; set; } = new List<FlashcardTag>();
}