using System;
using System.Collections.Generic;

namespace EduShelf.Api.Models.Dtos
{
    public class AnswerDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class QuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public ICollection<AnswerDto> Answers { get; set; }
    }

    public class QuizDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<QuestionDto> Questions { get; set; }
    }
}