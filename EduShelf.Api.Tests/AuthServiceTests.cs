using Xunit;
using EduShelf.Api.Services;
using EduShelf.Api.Models.Entities;
using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Data;
using EduShelf.Api.Exceptions;
using EduShelf.Api.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace EduShelf.Api.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        
        // Helper to setup InMemory DB
        private ApiDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new TestApiDbContext(options, _mockConfiguration.Object);
        }

        // Helper to setup Service with mocks
        private AuthService CreateService(ApiDbContext context)
        {
            return new AuthService(context, _mockHttpContextAccessor.Object, _mockEmailService.Object);
        }

        public AuthServiceTests()
        {
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockEmailService = new Mock<IEmailService>();
            _mockConfiguration = new Mock<IConfiguration>();
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ShouldReturnUserDtoAndSignIn()
        {
            // Arrange
            using var context = CreateContext();
            var password = "password123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            
            var role = new Role { RoleId = 1, Name = Roles.Student };
            context.Roles.Add(role);

            var user = new User 
            { 
                UserId = 1, 
                Username = "testuser", 
                Email = "test@example.com", 
                PasswordHash = hashedPassword,
                IsEmailConfirmed = true 
            };
            user.UserRoles.Add(new UserRole { Role = role });
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Mock HttpContext for SignInAsync
            var mockHttpContext = new Mock<HttpContext>();
            var mockAuthService = new Mock<IAuthenticationService>();
            
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(s => s.GetService(typeof(IAuthenticationService)))
                .Returns(mockAuthService.Object);
            
            mockHttpContext.Setup(h => h.RequestServices).Returns(serviceProvider.Object);
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);

            var service = CreateService(context);
            var loginDto = new UserLogin { Email = "test@example.com", Password = password };

            // Act
            var result = await service.LoginAsync(loginDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testuser", result.Username);
            Assert.Equal("test@example.com", result.Email);
            
            // Verify SignInAsync was called via IAuthenticationService logic (indirectly)
            // Note: verifying extension methods like SignInAsync is hard because they are static.
            // Usually we rely on the fact it didn't throw and returned success.
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ShouldThrowUnauthorized()
        {
            // Arrange
            using var context = CreateContext();
            var password = "password123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User 
            { 
                UserId = 1, 
                Username = "testuser", 
                Email = "test@example.com", 
                PasswordHash = hashedPassword,
                IsEmailConfirmed = true 
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var loginDto = new UserLogin { Email = "test@example.com", Password = "wrongpassword" };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(loginDto));
        }

        [Fact]
        public async Task RegisterUserAsync_ValidData_ShouldCreateUserAndSendEmail()
        {
            // Arrange
            using var context = CreateContext();
            var role = new Role { RoleId = 1, Name = Roles.Student };
            context.Roles.Add(role);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var registerDto = new UserRegister { Username = "newuser", Email = "new@example.com", Password = "password123" };

            // Act
            var result = await service.RegisterUserAsync(registerDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("newuser", result.Username);
            Assert.Single(context.Users);
            
            var createdUser = await context.Users.FirstAsync();
            Assert.False(createdUser.IsEmailConfirmed);
            Assert.NotNull(createdUser.EmailConfirmationToken);
            
            _mockEmailService.Verify(x => x.SendEmailAsync(It.Is<string>(s => s == "new@example.com"), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ConfirmEmailAsync_ValidToken_ShouldConfirmEmail()
        {
            // Arrange
            using var context = CreateContext();
            var user = new User 
            { 
                UserId = 1, 
                Email = "test@example.com", 
                IsEmailConfirmed = false, 
                EmailConfirmationToken = "valid_token",
                EmailConfirmationTokenExpiresAt = DateTime.UtcNow.AddHours(24)
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var confirmDto = new ConfirmEmailDto { Email = "test@example.com", Token = "valid_token" };

            // Act
            await service.ConfirmEmailAsync(confirmDto);

            // Assert
            var dbUser = await context.Users.FirstAsync();
            Assert.True(dbUser.IsEmailConfirmed);
            Assert.Null(dbUser.EmailConfirmationToken);
        }

        [Fact]
        public async Task ConfirmEmailAsync_ExpiredToken_ShouldThrowBadRequest()
        {
            // Arrange
            using var context = CreateContext();
            var user = new User 
            { 
                UserId = 1, 
                Email = "test@example.com", 
                IsEmailConfirmed = false, 
                EmailConfirmationToken = "expired_token",
                EmailConfirmationTokenExpiresAt = DateTime.UtcNow.AddHours(-1)
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var confirmDto = new ConfirmEmailDto { Email = "test@example.com", Token = "expired_token" };

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => service.ConfirmEmailAsync(confirmDto));
        }
    }
}
