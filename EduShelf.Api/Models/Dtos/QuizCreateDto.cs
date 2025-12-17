using System.Collections.Generic;

namespace EduShelf.Api.Models.Dtos
{
    public class AnswerCreateDto
    {
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class QuestionCreateDto
    {
        public string Text { get; set; }
        public ICollection<AnswerCreateDto> Answers { get; set; }
    }

    public class QuizCreateDto
    {
        public string Title { get; set; }
        public ICollection<QuestionCreateDto> Questions { get; set; }
    }
}