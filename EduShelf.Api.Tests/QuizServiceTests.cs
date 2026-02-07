using EduShelf.Api.Data;
using EduShelf.Api.Exceptions;
using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Models.Entities;
using EduShelf.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.DependencyInjection; // For service provider if needed

namespace EduShelf.Api.Tests
{
    public class QuizServiceTests
    {
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IDocumentService> _mockDocumentService;
        private readonly Mock<IChatCompletionService> _mockChatCompletionService;
        private readonly Kernel _mockKernel;

        public QuizServiceTests()
        {
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockDocumentService = new Mock<IDocumentService>();
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

        private QuizService CreateService(ApiDbContext context, int userId = 1, string role = "User")
        {
             var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext();
            httpContext.User = claimsPrincipal;

            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            return new QuizService(
                context, 
                _mockHttpContextAccessor.Object, 
                _mockKernel, 
                _mockConfiguration.Object, 
                _mockDocumentService.Object);
        }

        [Fact]
        public async Task GetQuizzesAsync_ShouldReturnUserQuizzes_WhenNotAdmin()
        {
            // Arrange
            using var context = CreateContext();
            var quiz1 = new Quiz { Id = 1, Title = "My Quiz", UserId = 1, CreatedAt = DateTime.UtcNow };
            var quiz2 = new Quiz { Id = 2, Title = "Other Quiz", UserId = 2, CreatedAt = DateTime.UtcNow };
            context.Quizzes.AddRange(quiz1, quiz2);
            await context.SaveChangesAsync();

            var service = CreateService(context, userId: 1);

            // Act
            var result = await service.GetQuizzesAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("My Quiz", result.First().Title);
        }

        [Fact]
        public async Task CreateQuizAsync_ShouldCreateQuiz_WithQuestions()
        {
            // Arrange
            using var context = CreateContext();
            var service = CreateService(context, userId: 1);
            var quizDto = new QuizCreateDto
            {
                Title = "New Quiz",
                Questions = new List<QuestionCreateDto>
                {
                    new QuestionCreateDto
                    {
                        Text = "Q1",
                        Answers = new List<AnswerCreateDto> { new AnswerCreateDto { Text = "A1", IsCorrect = true } }
                    }
                }
            };

            // Act
            var result = await service.CreateQuizAsync(quizDto);

            // Assert
            Assert.Equal("New Quiz", result.Title);
            Assert.Single(context.Quizzes);
            Assert.Single(context.Questions);
            Assert.Single(context.Answers);
        }

        [Fact]
        public async Task DeleteQuizAsync_ShouldRemoveQuiz_WhenOwner()
        {
            // Arrange
            using var context = CreateContext();
            var quiz = new Quiz { Id = 1, Title = "To Delete", UserId = 1 };
            context.Quizzes.Add(quiz);
            await context.SaveChangesAsync();

            var service = CreateService(context, userId: 1);

            // Act
            await service.DeleteQuizAsync(1);

            // Assert
            Assert.Empty(context.Quizzes);
        }

         [Fact]
        public async Task DeleteQuizAsync_ShouldThrowForbid_WhenNotOwner()
        {
            // Arrange
            using var context = CreateContext();
            var quiz = new Quiz { Id = 1, Title = "To Delete", UserId = 2 };
            context.Quizzes.Add(quiz);
            await context.SaveChangesAsync();

            var service = CreateService(context, userId: 1);

            // Act & Assert
            await Assert.ThrowsAsync<ForbidException>(() => service.DeleteQuizAsync(1));
            Assert.NotEmpty(context.Quizzes);
        }

        [Fact]
        public async Task UpdateQuizAsync_ShouldUpdateTitleAndQuestions()
        {
            // Arrange
            using var context = CreateContext();
            var quiz = new Quiz 
            { 
                Id = 1, 
                Title = "Old Title", 
                UserId = 1,
                Questions = new List<Question> 
                { 
                    new Question { Id = 1, Text = "Old Q" } 
                } 
            };
            context.Quizzes.Add(quiz);
            await context.SaveChangesAsync();

            var service = CreateService(context, userId: 1);
            var updateDto = new QuizUpdateDto
            {
                Title = "New Title",
                Questions = new List<QuestionUpdateDto>
                {
                    new QuestionUpdateDto { Id = 1, Text = "Updated Q", Answers = new List<AnswerUpdateDto>() } 
                }
            };

            // Act
            var result = await service.UpdateQuizAsync(1, updateDto);

            // Assert
            Assert.Equal("New Title", result.Title);
            var dbQuiz = await context.Quizzes.Include(q => q.Questions).FirstAsync();
            Assert.Equal("Updated Q", dbQuiz.Questions.First().Text);
        }
    }
}
