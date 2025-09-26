using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduShelf.Api.Models.Entities
{
    public class Question
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QuizId { get; set; }

        [Required]
        public string? Text { get; set; }

        [ForeignKey("QuizId")]
        public virtual Quiz? Quiz { get; set; }

        public virtual ICollection<Answer>? Answers { get; set; }
    }
}