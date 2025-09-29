using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EduShelf.Api.Services
{
    public static class TextChunker
    {
        public static List<string> SplitBySentence(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new List<string>();
            }

            // Simple regex to split by sentences, handles basic cases.
            var sentences = Regex.Split(text, @"(?<=[\.!\?])\s+");
            return new List<string>(sentences);
        }
    }
}