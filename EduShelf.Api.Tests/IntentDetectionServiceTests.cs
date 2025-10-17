using Xunit;
using EduShelf.Api.Services;
using Moq;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;
using EduShelf.Api.Models.Entities;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace EduShelf.Api.Tests
{
    public class IntentDetectionServiceTests
    {
        private readonly Mock<Kernel> _kernelMock;
        private readonly Mock<ILogger<IntentDetectionService>> _loggerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IChatCompletionService> _chatCompletionServiceMock;
        private readonly IntentDetectionService _service;
        private readonly Mock<IServiceProvider> _serviceProviderMock;

        public IntentDetectionServiceTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _kernelMock = new Mock<Kernel>(_serviceProviderMock.Object);
            _loggerMock = new Mock<ILogger<IntentDetectionService>>();
            _configurationMock = new Mock<IConfiguration>();
            _chatCompletionServiceMock = new Mock<IChatCompletionService>();

            // Setup Kernel mock to return the mocked chat completion service
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(IChatCompletionService))).Returns(_chatCompletionServiceMock.Object);

            // Setup Configuration mock
            _configurationMock.Setup(c => c.GetValue<string>("AIService:Prompts:Intent")).Returns("system_prompt_here");

            _service = new IntentDetectionService(_kernelMock.Object, _loggerMock.Object, _configurationMock.Object);
        }

        [Fact]
        public async Task GetIntentAsync_ShouldParseIntentCorrectly_WhenAiServiceReturnsValidJson()
        {
            // Arrange
            var userInput = "What is the summary of the document 'MyDocument'?";
            var expectedIntent = new Intent { Type = "summarize", DocumentName = "MyDocument" };
            var aiResponseJson = JsonSerializer.Serialize(expectedIntent);
            
            var chatMessageContent = new ChatMessageContent(AuthorRole.Assistant, aiResponseJson);

            _chatCompletionServiceMock
                .Setup(c => c.GetChatMessageContentAsync(
                    It.IsAny<ChatHistory>(),
                    It.IsAny<PromptExecutionSettings>(),
                    It.IsAny<Kernel>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(chatMessageContent);

            // Act
            var result = await _service.GetIntentAsync(userInput);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedIntent.Type, result.Type);
            Assert.Equal(expectedIntent.DocumentName, result.DocumentName);
        }

        [Fact]
        public async Task GetIntentAsync_ShouldReturnDefaultIntent_WhenAiServiceReturnsInvalidJson()
        {
            // Arrange
            var userInput = "Some random user input.";
            var aiResponseJson = "this is not json";

            var chatMessageContent = new ChatMessageContent(AuthorRole.Assistant, aiResponseJson);

            _chatCompletionServiceMock
                .Setup(c => c.GetChatMessageContentAsync(
                    It.IsAny<ChatHistory>(),
                    It.IsAny<PromptExecutionSettings>(),
                    It.IsAny<Kernel>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(chatMessageContent);

            // Act
            var result = await _service.GetIntentAsync(userInput);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("question", result.Type);
            Assert.Null(result.DocumentName);
        }
    }
}