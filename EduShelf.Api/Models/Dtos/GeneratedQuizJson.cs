using System.Collections.Generic;

namespace EduShelf.Api.Models.Dtos
{
    public class GeneratedQuizJson
    {
        public string Title { get; set; } = string.Empty;
        public List<GeneratedQuestionJson> Questions { get; set; } = new();
    }

    public class GeneratedQuestionJson
    {
        public string Text { get; set; } = string.Empty;
        public List<GeneratedAnswerJson> Answers { get; set; } = new();
    }

    public class GeneratedAnswerJson
    {
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }
}
