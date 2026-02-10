using Xunit;
using EduShelf.Api.Controllers;
using EduShelf.Api.Models.Entities;
using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace EduShelf.Api.Tests
{
    public class FlashcardsControllerTests
    {
        private readonly Mock<IFlashcardService> _mockFlashcardService;
        private readonly FlashcardsController _controller;
        private readonly ClaimsPrincipal _user;

        public FlashcardsControllerTests()
        {
            _mockFlashcardService = new Mock<IFlashcardService>();
            
            _user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Student")
            }, "mock"));

            _controller = new FlashcardsController(_mockFlashcardService.Object);
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = _user }
            };
        }

        [Fact]
        public async Task GetFlashcards_ShouldReturnFlashcardsFromService()
        {
            // Arrange
            var flashcards = new List<FlashcardDto>
            {
                new FlashcardDto { Id = 1, Question = "Q1", Answer = "A1", UserId = 1, Tags = new List<string>() },
                new FlashcardDto { Id = 2, Question = "Q2", Answer = "A2", UserId = 1, Tags = new List<string>() }
            };

            _mockFlashcardService.Setup(s => s.GetFlashcardsAsync(1, false))
                .ReturnsAsync(flashcards);

            // Act
            var result = await _controller.GetFlashcards();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<FlashcardDto>>>(result);
            var returnedFlashcards = Assert.IsAssignableFrom<IEnumerable<FlashcardDto>>(
                (actionResult.Result as OkObjectResult)?.Value ?? actionResult.Value);
            
            // Handle both ActionResult wrapping or direct value
            if (actionResult.Result is OkObjectResult okResult)
            {
                returnedFlashcards = Assert.IsAssignableFrom<IEnumerable<FlashcardDto>>(okResult.Value);
            }
            
            Assert.Equal(2, ((List<FlashcardDto>)returnedFlashcards).Count);
        }

        [Fact]
        public async Task GetFlashcard_ShouldReturnFlashcard()
        {
            // Arrange
            var flashcard = new FlashcardDto { Id = 1, Question = "Q1", Answer = "A1", UserId = 1, Tags = new List<string>() };
            _mockFlashcardService.Setup(s => s.GetFlashcardAsync(1, 1, false))
                .ReturnsAsync(flashcard);

            // Act
            var result = await _controller.GetFlashcard(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<FlashcardDto>>(result);
            if (actionResult.Result is OkObjectResult okResult)
            {
                Assert.Equal(flashcard, okResult.Value);
            }
            else 
            {
               Assert.Equal(flashcard, actionResult.Value);
            }
        }
    }
}