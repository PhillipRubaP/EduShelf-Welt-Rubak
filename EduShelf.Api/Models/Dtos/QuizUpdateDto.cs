using System.Collections.Generic;

namespace EduShelf.Api.Models.Dtos
{
    public class AnswerUpdateDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class QuestionUpdateDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public ICollection<AnswerUpdateDto> Answers { get; set; }
    }

    public class QuizUpdateDto
    {
        public string Title { get; set; }
        public ICollection<QuestionUpdateDto> Questions { get; set; }
    }
}