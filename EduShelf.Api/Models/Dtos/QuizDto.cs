using System;
using System.Collections.Generic;

namespace EduShelf.Api.Models.Dtos
{
    public class AnswerDto
    {
        public int Id { get; set; }
        public required string Text { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class QuestionDto
    {
        public int Id { get; set; }
        public required string Text { get; set; }
        public required ICollection<AnswerDto> Answers { get; set; }
    }

    public class QuizDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public required ICollection<QuestionDto> Questions { get; set; }
    }
}