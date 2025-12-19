using Xunit;
using EduShelf.Api.Services;
using EduShelf.Api.Data;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using EduShelf.Api.Models.Entities;
using System;
using Microsoft.SemanticKernel.ChatCompletion;
using EduShelf.Api.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EduShelf.Api.Tests
{
    public class ChatServiceTests
    {
        private readonly Mock<IChatCompletionService> _mockChatCompletionService;
        private readonly Kernel _kernel;
        private readonly Mock<ILogger<ChatService>> _mockLogger;
        private readonly Mock<IImageProcessingService> _mockImageProcessingService;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly Mock<IConfiguration> _mockConfiguration;

        public ChatServiceTests()
        {
            _mockChatCompletionService = new Mock<IChatCompletionService>();
            
            var services = new ServiceCollection();
            services.AddSingleton(_mockChatCompletionService.Object);
            var serviceProvider = services.BuildServiceProvider();
            
            _kernel = new Kernel(serviceProvider);

            _mockLogger = new Mock<ILogger<ChatService>>();
            _mockImageProcessingService = new Mock<IImageProcessingService>();
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockConfiguration = new Mock<IConfiguration>();
        }

        private ApiDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new TestApiDbContext(options, _mockConfiguration.Object);
        }

        [Fact]
        public async Task CreateChatSessionAsync_ShouldCreateSession_WhenTitleIsValid()
        {
            // Arrange
            using var context = CreateContext();
            
            // Create a user first
            var user = new User { UserId = 1, Username = "testuser", Email = "test@example.com", PasswordHash = "hash" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new ChatService(
                context,
                _kernel,
                _mockLogger.Object,
                null!, null!, null!, 
                _mockImageProcessingService.Object,
                _mockEnvironment.Object);

            // Act
            var session = await service.CreateChatSessionAsync(1, "Test Session");

            // Assert
            Assert.NotNull(session);
            Assert.Equal("Test Session", session.Title);
            Assert.Equal(1, session.UserId);
            Assert.NotEqual(0, session.Id);
        }

        [Fact]
        public async Task CreateChatSessionAsync_ShouldThrowBadRequest_WhenTitleIsEmpty()
        {
             // Arrange
            using var context = CreateContext();
            
            var service = new ChatService(
                context,
                _kernel,
                _mockLogger.Object,
                null!, null!, null!, 
                _mockImageProcessingService.Object,
                _mockEnvironment.Object);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => service.CreateChatSessionAsync(1, ""));
        }

        [Fact]
        public async Task GetChatSessionsAsync_ShouldReturnSessions_ForUser()
        {
            // Arrange
            using var context = CreateContext();

            var user = new User { UserId = 1, Username = "testuser", Email = "test@example.com", PasswordHash = "hash" };
            context.Users.Add(user);
            
            var session1 = new ChatSession { UserId = 1, Title = "Session 1", CreatedAt = DateTime.UtcNow };
            var session2 = new ChatSession { UserId = 1, Title = "Session 2", CreatedAt = DateTime.UtcNow.AddMinutes(1) };
            var session3 = new ChatSession { UserId = 2, Title = "Other User Session", CreatedAt = DateTime.UtcNow };
            
            context.ChatSessions.AddRange(session1, session2, session3);
            await context.SaveChangesAsync();

            var service = new ChatService(
                context,
                _kernel,
                _mockLogger.Object,
                null!, null!, null!, 
                _mockImageProcessingService.Object,
                _mockEnvironment.Object);

            // Act
            var sessions = await service.GetChatSessionsAsync(1);

            // Assert
            Assert.Equal(2, sessions.Count);
            Assert.Contains(sessions, s => s.Title == "Session 1");
            Assert.Contains(sessions, s => s.Title == "Session 2");
            Assert.DoesNotContain(sessions, s => s.Title == "Other User Session");
        }
    }
}
