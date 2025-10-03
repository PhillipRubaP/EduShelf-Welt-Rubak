using System.ComponentModel.DataAnnotations.Schema;

namespace EduShelf.Api.Models.Entities;

public class FlashcardTag
{
    public int FlashcardId { get; set; }
    [ForeignKey("FlashcardId")]
    public Flashcard Flashcard { get; set; }

    public int TagId { get; set; }
    [ForeignKey("TagId")]
    public Tag Tag { get; set; }
}