using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduShelf.Api.Models.Entities
{
    public class Answer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [Required]
        public string Text { get; set; }

        [Required]
        public bool IsCorrect { get; set; }

        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; }
    }
}