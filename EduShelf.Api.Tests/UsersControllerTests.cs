using Xunit;
using EduShelf.Api.Controllers;
using EduShelf.Api.Data;
using EduShelf.Api.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using EduShelf.Api.Models.Dtos;
using System.Collections.Generic;

namespace EduShelf.Api.Tests
{
    public class UsersControllerTests
    {
        private async Task<(ApiDbContext, UsersController)> SetupTest()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var inMemorySettings = new Dictionary<string, string> {
                {"Jwt:Key", "your_super_secret_key_that_is_long_enough"},
                {"Jwt:Issuer", "your_issuer"},
                {"Jwt:Audience", "your_audience"},
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var context = new TestApiDbContext(dbContextOptions, configuration);
            
            // Seed with a default role and an existing user
            var role = new Role { RoleId = 1, Name = "Schüler" };
            context.Roles.Add(role);
            var user = new User { UserId = 1, Username = "testuser", Email = "test@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123") };
            user.UserRoles.Add(new UserRole { Role = role });
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var controller = new UsersController(context, configuration);
            
            return (context, controller);
        }

        [Fact]
        public async Task PostUser_ShouldCreateUser_WhenUsernameAndEmailAreUnique()
        {
            // Arrange
            var (context, controller) = await SetupTest();
            var newUser = new UserRegister { Username = "newuser", Email = "new@example.com", Password = "password123" };

            // Act
            var result = await controller.PostUser(newUser);

            // Assert
            var actionResult = Assert.IsType<ActionResult<UserDto>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var userDto = Assert.IsType<UserDto>(createdAtActionResult.Value);
            Assert.Equal("newuser", userDto.Username);

            // Cleanup
            await context.DisposeAsync();
        }

        [Fact]
        public async Task PostUser_ShouldReturnConflict_WhenUsernameExists()
        {
            // Arrange
            var (context, controller) = await SetupTest();
            var existingUser = new UserRegister { Username = "testuser", Email = "another@example.com", Password = "password123" };

            // Act
            var result = await controller.PostUser(existingUser);

            // Assert
            var actionResult = Assert.IsType<ActionResult<UserDto>>(result);
            var conflictResult = Assert.IsType<ConflictObjectResult>(actionResult.Result);
            Assert.Equal("Username already exists.", conflictResult.Value);

            // Cleanup
            await context.DisposeAsync();
        }

        [Fact]
        public async Task PostUser_ShouldReturnConflict_WhenEmailExists()
        {
            // Arrange
            var (context, controller) = await SetupTest();
            var existingUser = new UserRegister { Username = "anotheruser", Email = "test@example.com", Password = "password123" };

            // Act
            var result = await controller.PostUser(existingUser);

            // Assert
            var actionResult = Assert.IsType<ActionResult<UserDto>>(result);
            var conflictResult = Assert.IsType<ConflictObjectResult>(actionResult.Result);
            Assert.Equal("Email already exists.", conflictResult.Value);

            // Cleanup
            await context.DisposeAsync();
        }

        [Fact]
        public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // Arrange
            var (context, controller) = await SetupTest();
            var login = new UserLogin { Email = "test@example.com", Password = "password123" };

            // Act
            var result = await controller.Login(login);

            // Assert
            var actionResult = Assert.IsType<ActionResult<LoginResponseDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var loginResponse = Assert.IsType<LoginResponseDto>(okResult.Value);
            Assert.NotNull(loginResponse.Token);

            // Cleanup
            await context.DisposeAsync();
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
        {
            // Arrange
            var (context, controller) = await SetupTest();
            var login = new UserLogin { Email = "test@example.com", Password = "wrongpassword" };

            // Act
            var result = await controller.Login(login);

            // Assert
            var actionResult = Assert.IsType<ActionResult<LoginResponseDto>>(result);
            Assert.IsType<UnauthorizedResult>(actionResult.Result);

            // Cleanup
            await context.DisposeAsync();
        }
    }
}