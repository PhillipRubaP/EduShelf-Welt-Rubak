using Xunit;
using EduShelf.Api.Services;

namespace EduShelf.Api.Tests
{
    public class TokenServiceTests
    {
        private readonly TokenService _tokenService;

        public TokenServiceTests()
        {
            _tokenService = new TokenService();
        }

        [Fact]
        public void CountTokens_ShouldReturnCorrectCount()
        {
            var text = "Hello world";
            var count = _tokenService.CountTokens(text);
            Assert.True(count > 0);
        }

        [Fact]
        public void Truncate_ShouldTruncateToMaxTokens()
        {
            var text = "This is a long sentence that should be truncated.";
            var maxTokens = 2;
            var truncated = _tokenService.Truncate(text, maxTokens);
            
            var count = _tokenService.CountTokens(truncated);
            Assert.True(count <= maxTokens);
            Assert.NotEqual(text, truncated);
        }

        [Fact]
        public void Truncate_ShouldNotTruncateIfShortEnough()
        {
            var text = "Short text";
            var maxTokens = 100;
            var truncated = _tokenService.Truncate(text, maxTokens);
            
            Assert.Equal(text, truncated);
        }
    }
}
