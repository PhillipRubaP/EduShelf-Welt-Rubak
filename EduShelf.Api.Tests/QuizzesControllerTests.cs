using EduShelf.Api.Controllers;
using EduShelf.Api.Data;
using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace EduShelf.Api.Tests
{
    public class QuizzesControllerTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;

        public QuizzesControllerTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
        }

        private ApiDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new TestApiDbContext(options, _mockConfiguration.Object);
        }

        private QuizzesController CreateController(ApiDbContext context, int userId, string role = "User")
        {
            var controller = new QuizzesController(context);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            }, "TestAuthentication"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            return controller;
        }

        [Fact]
        public async Task GetQuizzes_ShouldReturnUserQuizzes_WhenNotAdmin()
        {
            // Arrange
            using var context = CreateContext();
            var quiz1 = new Quiz { Id = 1, Title = "My Quiz", UserId = 1, CreatedAt = DateTime.UtcNow };
            var quiz2 = new Quiz { Id = 2, Title = "Other Quiz", UserId = 2, CreatedAt = DateTime.UtcNow };
            context.Quizzes.AddRange(quiz1, quiz2);
            await context.SaveChangesAsync();

            var controller = CreateController(context, userId: 1);

            // Act
            var result = await controller.GetQuizzes();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<QuizDto>>>(result);
            var quizzes = Assert.IsAssignableFrom<IEnumerable<QuizDto>>(actionResult.Value);
            Assert.Single(quizzes);
            Assert.Equal("My Quiz", quizzes.First().Title);
        }

        [Fact]
        public async Task PostQuiz_ShouldCreateQuiz()
        {
            // Arrange
            using var context = CreateContext();
            var controller = CreateController(context, userId: 1);
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
            var result = await controller.PostQuiz(quizDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<QuizDto>>(result);
            var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnDto = Assert.IsType<QuizDto>(createdResult.Value);
            
            Assert.Equal("New Quiz", returnDto.Title);
            Assert.Single(context.Quizzes);
            Assert.Single(context.Questions);
            Assert.Single(context.Answers);
        }

        [Fact]
        public async Task DeleteQuiz_ShouldRemoveQuiz_WhenOwner()
        {
            // Arrange
            using var context = CreateContext();
            var quiz = new Quiz { Id = 1, Title = "To Delete", UserId = 1 };
            context.Quizzes.Add(quiz);
            await context.SaveChangesAsync();

            var controller = CreateController(context, userId: 1);

            // Act
            var result = await controller.DeleteQuiz(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Empty(context.Quizzes);
        }

         [Fact]
        public async Task DeleteQuiz_ShouldReturnForbid_WhenNotOwner()
        {
            // Arrange
            using var context = CreateContext();
            var quiz = new Quiz { Id = 1, Title = "To Delete", UserId = 2 };
            context.Quizzes.Add(quiz);
            await context.SaveChangesAsync();

            var controller = CreateController(context, userId: 1);

            // Act
            var result = await controller.DeleteQuiz(1);

            // Assert
            Assert.IsType<ForbidResult>(result);
            Assert.NotEmpty(context.Quizzes);
        }
    }
}
