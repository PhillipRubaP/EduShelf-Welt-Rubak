using EduShelf.Api.Helpers;
using Xunit;

namespace EduShelf.Api.Tests.Helpers
{
    public class JsonHelperTests
    {
        [Fact]
        public void ExtractJson_ShouldRemoveMarkdownCodeBlocks()
        {
            var input = "```json\n{\"foo\": \"bar\"}\n```";
            var expected = "{\"foo\": \"bar\"}";
            var actual = JsonHelper.ExtractJson(input);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ExtractJson_ShouldHandleTextAroundJson()
        {
            var input = "Here is the JSON you requested:\n\n[{\"id\": 1}]\n\nHope this helps!";
            var expected = "[{\"id\": 1}]";
            var actual = JsonHelper.ExtractJson(input);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ExtractJson_ShouldHandleNoMarkdown()
        {
            var input = "{ \"key\": \"value\" }";
            var expected = "{ \"key\": \"value\" }";
            var actual = JsonHelper.ExtractJson(input);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ExtractJson_ShouldReturnEmptyObject_WhenInputNullOrEmpty()
        {
            Assert.Equal("{}", JsonHelper.ExtractJson(null));
            Assert.Equal("{}", JsonHelper.ExtractJson(""));
            Assert.Equal("{}", JsonHelper.ExtractJson("   "));
        }

        [Fact]
        public void ExtractJson_ShouldReturnOriginal_WhenNoBracketsFound()
        {
            var input = "Just some text without JSON.";
            var actual = JsonHelper.ExtractJson(input);
            Assert.Equal("Just some text without JSON.", actual);
        }
         [Fact]
        public void ExtractJson_ShouldHandleMarkdownWithoutLanguageIdentifier()
        {
            var input = "```\n{\"foo\": \"bar\"}\n```";
            var expected = "{\"foo\": \"bar\"}";
            var actual = JsonHelper.ExtractJson(input);
            Assert.Equal(expected, actual);
        }
    }
}
