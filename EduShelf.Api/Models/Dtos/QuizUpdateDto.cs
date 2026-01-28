using System.Collections.Generic;

namespace EduShelf.Api.Models.Dtos
{
    public class AnswerUpdateDto
    {
        public int Id { get; set; }
        public required string Text { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class QuestionUpdateDto
    {
        public int Id { get; set; }
        public required string Text { get; set; }
        public required ICollection<AnswerUpdateDto> Answers { get; set; }
    }

    public class QuizUpdateDto
    {
        public required string Title { get; set; }
        public required ICollection<QuestionUpdateDto> Questions { get; set; }
    }
}