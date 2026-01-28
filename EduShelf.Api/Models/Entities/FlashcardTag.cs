using System.ComponentModel.DataAnnotations.Schema;

namespace EduShelf.Api.Models.Entities;

public class FlashcardTag
{
    public int FlashcardId { get; set; }
    [ForeignKey("FlashcardId")]
    public virtual Flashcard Flashcard { get; set; } = null!;

    public int TagId { get; set; }
    [ForeignKey("TagId")]
    public virtual Tag Tag { get; set; } = null!;
}