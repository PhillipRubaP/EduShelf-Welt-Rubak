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
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace EduShelf.Api.Tests
{
    public class IntentDetectionServiceTests
    {
        private readonly Kernel _kernel;
        private readonly Mock<ILogger<IntentDetectionService>> _loggerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IChatCompletionService> _chatCompletionServiceMock;
        private readonly IntentDetectionService _service;

        public IntentDetectionServiceTests()
        {
            _chatCompletionServiceMock = new Mock<IChatCompletionService>();
            
            var kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder.Services.AddSingleton(_chatCompletionServiceMock.Object);
            _kernel = kernelBuilder.Build();
            
            _loggerMock = new Mock<ILogger<IntentDetectionService>>();
            _configurationMock = new Mock<IConfiguration>();

            // Setup Configuration mock for the specific key
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(s => s.Value).Returns("system_prompt_here");
            _configurationMock.Setup(c => c.GetSection("AIService:Prompts:Intent")).Returns(configSectionMock.Object);

            _service = new IntentDetectionService(_kernel, _loggerMock.Object, _configurationMock.Object);
        }

        [Fact]
        public async Task GetIntentAsync_ShouldParseIntentCorrectly_WhenAiServiceReturnsValidJson()
        {
            // Arrange
            var userInput = "What is the summary of the document 'MyDocument'?";
            var expectedIntent = new Intent { Type = "summarize", DocumentName = "MyDocument" };
            var aiResponseJson = JsonSerializer.Serialize(expectedIntent);
            
            var chatMessageContent = new ChatMessageContent(AuthorRole.Assistant, aiResponseJson);
            var responseList = new List<ChatMessageContent> { chatMessageContent };

            _chatCompletionServiceMock
                .Setup(c => c.GetChatMessageContentsAsync(
                    It.IsAny<ChatHistory>(),
                    It.IsAny<PromptExecutionSettings>(),
                    It.IsAny<Kernel>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseList);

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
            var responseList = new List<ChatMessageContent> { chatMessageContent };

            _chatCompletionServiceMock
                .Setup(c => c.GetChatMessageContentsAsync(
                    It.IsAny<ChatHistory>(),
                    It.IsAny<PromptExecutionSettings>(),
                    It.IsAny<Kernel>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseList);

            // Act
            var result = await _service.GetIntentAsync(userInput);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("question", result.Type);
            Assert.Null(result.DocumentName);
        }
    }
}