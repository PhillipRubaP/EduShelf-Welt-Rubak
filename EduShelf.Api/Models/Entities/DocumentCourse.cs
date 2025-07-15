using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduShelf.Api.Models.Entities;

public class DocumentCourse
{
    public int Id { get; set; }

    [Required]
    public int DocumentId { get; set; }

    [Required]
    public int CourseId { get; set; }

    [ForeignKey("DocumentId")]
    public virtual Document Document { get; set; }

    [ForeignKey("CourseId")]
    public virtual Course Course { get; set; }
}