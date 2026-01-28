using System.Collections.Generic;

namespace EduShelf.Api.Models.Dtos
{
    public class AnswerCreateDto
    {
        public required string Text { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class QuestionCreateDto
    {
        public required string Text { get; set; }
        public required ICollection<AnswerCreateDto> Answers { get; set; }
    }

    public class QuizCreateDto
    {
        public required string Title { get; set; }
        public required ICollection<QuestionCreateDto> Questions { get; set; }
    }
}