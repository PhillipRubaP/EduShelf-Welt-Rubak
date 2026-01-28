using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduShelf.Api.Models.Entities;

public class Document
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Title { get; set; }

    [Required]
    public required string Path { get; set; }

    [Required]
    [MaxLength(50)]
    public required string FileType { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("UserId")]
    public virtual User User { get; set; }

    public virtual ICollection<DocumentTag> DocumentTags { get; set; }
}