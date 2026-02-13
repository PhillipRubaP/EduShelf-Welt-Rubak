using Xunit;
using EduShelf.Api.Services;
using EduShelf.Api.Models.Entities;
using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Data;
using EduShelf.Api.Exceptions;
using EduShelf.Api.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace EduShelf.Api.Tests
{
    public class FlashcardServiceTests
    {
        private readonly Mock<IDocumentService> _mockDocumentService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<FlashcardService>> _mockLogger;
        private readonly Mock<IChatCompletionService> _mockChatCompletionService;
        private readonly Kernel _mockKernel;

        public FlashcardServiceTests()
        {
            _mockDocumentService = new Mock<IDocumentService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<FlashcardService>>();
            _mockChatCompletionService = new Mock<IChatCompletionService>();

            // Setup Kernel with mock ChatCompletionService
            var services = new ServiceCollection();
            services.AddSingleton(_mockChatCompletionService.Object);
            _mockKernel = new Kernel(services.BuildServiceProvider());
        }

        private ApiDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new TestApiDbContext(options, _mockConfiguration.Object);
        }

        private FlashcardService CreateService(ApiDbContext context)
        {
            return new FlashcardService(
                context, 
                _mockKernel, 
                _mockDocumentService.Object, 
                _mockConfiguration.Object, 
                _mockLogger.Object);
        }

        [Fact]
        public async Task GenerateFlashcardsAsync_ValidContent_ShouldCreateFlashcards()
        {
            // Arrange
            using var context = CreateContext();
            var service = CreateService(context);
            var userId = 1;
            var request = new GenerateFlashcardsRequest { DocumentId = 1, Count = 2 };
            
            // Mock Document Service
            _mockDocumentService.Setup(d => d.GetDocumentAsync(1, userId, Roles.Student))
                .ReturnsAsync(new DocumentDto { Id = 1, Title = "Test Doc", FileType = "txt" });
            
            _mockDocumentService.Setup(d => d.GetDocumentContentAsync(1, userId, Roles.Student))
                .ReturnsAsync("This is some content about AI.");

            // Mock Configuration
            _mockConfiguration.Setup(c => c.GetSection("AIService:Prompts:Flashcard").Value)
                .Returns("Generate {Count} flashcards.");

            // Mock Chat Completion
            var jsonResponse = "[{\"Question\": \"What is AI?\", \"Answer\": \"Artificial Intelligence\"}, {\"Question\": \"Is it cool?\", \"Answer\": \"Yes\"}]";
            var chatMessageContent = new ChatMessageContent(AuthorRole.Assistant, jsonResponse);
            
            _mockChatCompletionService.Setup(c => c.GetChatMessageContentsAsync(
                It.IsAny<ChatHistory>(), 
                It.IsAny<PromptExecutionSettings>(), 
                It.IsAny<Kernel>(), 
                It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(new List<ChatMessageContent> { chatMessageContent });

            // Act
            var result = await service.GenerateFlashcardsAsync(request, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.ToList().Count);
            // Service returns OrderByDescending(Id), so the last added one comes first
            Assert.Equal("Is it cool?", result.First().Question);
            Assert.Equal("What is AI?", result.Last().Question);
            
            // Verify DB persistence
            Assert.Equal(2, context.Flashcards.Count());
            Assert.Single(context.Tags); // Should have created "Test Doc" tag
        }

        [Fact]
        public async Task GenerateFlashcardsAsync_NoContent_ShouldThrowBadRequest()
        {
             // Arrange
            using var context = CreateContext();
            var service = CreateService(context);
            var userId = 1;
            var request = new GenerateFlashcardsRequest { DocumentId = 1, Count = 2 };

            _mockDocumentService.Setup(d => d.GetDocumentAsync(1, userId, Roles.Student))
                .ReturnsAsync(new DocumentDto { Id = 1, Title = "Test Doc", FileType = "txt" });
            
            _mockDocumentService.Setup(d => d.GetDocumentContentAsync(1, userId, Roles.Student))
                .ReturnsAsync(""); // Empty content

             // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => service.GenerateFlashcardsAsync(request, userId));
        }

        [Fact]
        public async Task GenerateFlashcardsAsync_InvalidJsonFromAI_ShouldThrowKernelServiceException()
        {
            // Arrange
            using var context = CreateContext();
            var service = CreateService(context);
            var userId = 1;
            var request = new GenerateFlashcardsRequest { DocumentId = 1, Count = 2 };
            
            _mockDocumentService.Setup(d => d.GetDocumentAsync(1, userId, Roles.Student))
                .ReturnsAsync(new DocumentDto { Id = 1, Title = "Test Doc", FileType = "txt" });
            _mockDocumentService.Setup(d => d.GetDocumentContentAsync(1, userId, Roles.Student))
                .ReturnsAsync("Content");
            _mockConfiguration.Setup(c => c.GetSection("AIService:Prompts:Flashcard").Value).Returns("Prompt");

            // Mock AI returning bad JSON
            var chatMessageContent = new ChatMessageContent(AuthorRole.Assistant, "This is not JSON");
            _mockChatCompletionService.Setup(c => c.GetChatMessageContentsAsync(It.IsAny<ChatHistory>(), null, null, default))
                .ReturnsAsync(new List<ChatMessageContent> { chatMessageContent });

            // Act & Assert
            await Assert.ThrowsAsync<KernelServiceException>(() => service.GenerateFlashcardsAsync(request, userId));
        }
    }
}
