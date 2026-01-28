using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduShelf.Api.Models.Entities;

public class DocumentTag
{
    public int Id { get; set; }

    [Required]
    public int DocumentId { get; set; }

    [Required]
    public int TagId { get; set; }

    [ForeignKey("DocumentId")]
    public virtual Document Document { get; set; } = null!;

    [ForeignKey("TagId")]
    public virtual Tag Tag { get; set; } = null!;
}