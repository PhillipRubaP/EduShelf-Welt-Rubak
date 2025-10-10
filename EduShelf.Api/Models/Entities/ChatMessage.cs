using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduShelf.Api.Models.Entities;

public class ChatMessage
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ChatSessionId { get; set; }

    [Required]
    public string Message { get; set; }

    public string? Response { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("ChatSessionId")]
    public virtual ChatSession ChatSession { get; set; }
}