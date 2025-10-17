using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EduShelf.Api.Models.Dtos
{
    public class AnswerCreateDto
    {
        [Required]
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class QuestionCreateDto
    {
        [Required]
        public string Text { get; set; }
        public ICollection<AnswerCreateDto> Answers { get; set; }
    }

    public class QuizCreateDto
    {
        [Required]
        public string Title { get; set; }
        public ICollection<QuestionCreateDto> Questions { get; set; }
    }
}