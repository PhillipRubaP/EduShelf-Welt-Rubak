using Xunit;
using EduShelf.Api.Controllers;
using EduShelf.Api.Data;
using EduShelf.Api.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using EduShelf.Api.Models.Dtos;
using System;

namespace EduShelf.Api.Tests
{
    public class FlashcardsControllerTests
    {
        private async Task<(ApiDbContext, FlashcardsController)> SetupTest(int userId, string role = "User")
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique name for each test
                .Options;
            
            var configurationMock = new Mock<IConfiguration>();

            var context = new TestApiDbContext(dbContextOptions, configurationMock.Object);
            
            context.Flashcards.AddRange(
                new Flashcard { Id = 1, Question = "Q1", Answer = "A1", UserId = 1 },
                new Flashcard { Id = 2, Question = "Q2", Answer = "A2", UserId = 1 },
                new Flashcard { Id = 3, Question = "Q3", Answer = "A3", UserId = 2 }
            );
            await context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };

            if (!string.IsNullOrEmpty(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthentication"));
            
            var controller = new FlashcardsController(context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                }
            };
            
            return (context, controller);
        }

        [Fact]
        public async Task GetFlashcards_AsAdmin_ShouldReturnAllFlashcards()
        {
            // Arrange
            var (context, controller) = await SetupTest(99, "Admin");

            // Act
            var result = await controller.GetFlashcards();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<FlashcardDto>>>(result);
            var flashcards = Assert.IsAssignableFrom<IEnumerable<FlashcardDto>>(actionResult.Value);
            Assert.Equal(3, flashcards.Count());

            // Cleanup
            await context.DisposeAsync();
        }

        [Fact]
        public async Task GetFlashcards_AsNonAdmin_ShouldReturnOnlyOwnFlashcards()
        {
            // Arrange
            var (context, controller) = await SetupTest(1, "User");

            // Act
            var result = await controller.GetFlashcards();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<FlashcardDto>>>(result);
            var flashcards = Assert.IsAssignableFrom<IEnumerable<FlashcardDto>>(actionResult.Value);
            Assert.Equal(2, flashcards.Count());
            Assert.All(flashcards, f => Assert.Equal(1, f.UserId));

            // Cleanup
            await context.DisposeAsync();
        }
    }
}