using Microsoft.ML.Tokenizers;
using System.Linq;

namespace EduShelf.Api.Services
{
    public class TokenService : ITokenService
    {
        private readonly Tokenizer _tokenizer;

        public TokenService()
        {
            // using cl100k_base which is compatible with GPT-3.5/4 and many modern models
            // This requires Microsoft.ML.Tokenizers package
            _tokenizer = TiktokenTokenizer.CreateForModel("gpt-4");
        }

        public int CountTokens(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            return _tokenizer.CountTokens(text);
        }

        public string Truncate(string text, int maxTokens)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            var tokens = _tokenizer.EncodeToIds(text);
            if (tokens.Count <= maxTokens) return text;

            var truncatedTokens = tokens.Take(maxTokens).ToList();
            return _tokenizer.Decode(truncatedTokens);
        }
    }
}
