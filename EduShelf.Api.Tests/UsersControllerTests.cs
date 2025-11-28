using Xunit;
using EduShelf.Api.Controllers;
using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using EduShelf.Api.Services;
using System;
using EduShelf.Api.Exceptions;

namespace EduShelf.Api.Tests
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new UsersController(_mockUserService.Object);
        }

        [Fact]
        public async Task PostUser_ShouldCreateUser_WhenUsernameAndEmailAreUnique()
        {
            // Arrange
            var newUser = new UserRegister { Username = "newuser", Email = "new@example.com", Password = "password123" };
            var userDto = new UserDto { UserId = 1, Username = "newuser", Email = "new@example.com" };
            _mockUserService.Setup(service => service.RegisterUserAsync(newUser)).ReturnsAsync(userDto);

            // Act
            var result = await _controller.PostUser(newUser);

            // Assert
            var actionResult = Assert.IsType<ActionResult<UserDto>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnedUserDto = Assert.IsType<UserDto>(createdAtActionResult.Value);
            Assert.Equal("newuser", returnedUserDto.Username);
        }

        [Fact]
        public async Task PostUser_ShouldReturnConflict_WhenUsernameExists()
        {
            // Arrange
            var existingUser = new UserRegister { Username = "testuser", Email = "another@example.com", Password = "password123" };
            _mockUserService.Setup(service => service.RegisterUserAsync(existingUser)).ThrowsAsync(new BadRequestException("Username already exists."));

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _controller.PostUser(existingUser));
        }

        [Fact]
        public async Task PostUser_ShouldReturnConflict_WhenEmailExists()
        {
            // Arrange
            var existingUser = new UserRegister { Username = "anotheruser", Email = "test@example.com", Password = "password123" };
            _mockUserService.Setup(service => service.RegisterUserAsync(existingUser)).ThrowsAsync(new BadRequestException("Email already exists."));

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _controller.PostUser(existingUser));
        }

        [Fact]
        public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // Arrange
            var login = new UserLogin { Email = "test@example.com", Password = "password123" };
            var userDto = new UserDto { UserId = 1, Username = "testuser", Email = "test@example.com" };
            _mockUserService.Setup(service => service.LoginAsync(login)).ReturnsAsync(userDto);

            // Act
            var result = await _controller.Login(login);

            // Assert
            var actionResult = Assert.IsType<ActionResult<UserDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedUser = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal("testuser", returnedUser.Username);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
        {
            // Arrange
            var login = new UserLogin { Email = "test@example.com", Password = "wrongpassword" };
            _mockUserService.Setup(service => service.LoginAsync(login)).ThrowsAsync(new UnauthorizedAccessException());

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Login(login));
        }
    }
}