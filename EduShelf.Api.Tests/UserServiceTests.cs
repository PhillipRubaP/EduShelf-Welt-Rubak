using EduShelf.Api.Data;
using EduShelf.Api.Exceptions;
using EduShelf.Api.Models.Entities;
using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace EduShelf.Api.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<IConfiguration> _mockConfiguration;

        public UserServiceTests()
        {
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockConfiguration = new Mock<IConfiguration>();
        }

        private ApiDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new TestApiDbContext(options, _mockConfiguration.Object);
        }

        private UserService CreateService(ApiDbContext context, int currentUserId = 0, string role = "User")
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, currentUserId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddAuthentication().AddCookie();
            serviceCollection.AddLogging();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;
            httpContext.User = claimsPrincipal;

            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            return new UserService(context, _mockHttpContextAccessor.Object);
        }

        /*
        [Fact]
        public async Task RegisterUserAsync_ShouldHashPassword_AndCreateUser()
        {
            // Arrange
            using var context = CreateContext();
            // Seed generic role
            var role = new Role { RoleId = 1, Name = "Schüler" };
            context.Roles.Add(role);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var registerDto = new UserRegister { Username = "newuser", Email = "new@example.com", Password = "plainpassword" };

            // Act
            var result = await service.RegisterUserAsync(registerDto);

            // Assert
            var user = await context.Users.FindAsync(result.UserId);
            Assert.NotNull(user);
            Assert.Equal("newuser", user.Username);
            Assert.NotEqual("plainpassword", user.PasswordHash);
            Assert.True(BCrypt.Net.BCrypt.Verify("plainpassword", user.PasswordHash));
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldThrowBadRequest_WhenUsernameExists()
        {
            // Arrange
            using var context = CreateContext();
            var existingUser = new User { UserId = 1, Username = "existing", Email = "old@example.com", PasswordHash = "hash" };
            context.Users.Add(existingUser);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var registerDto = new UserRegister { Username = "existing", Email = "new@example.com", Password = "pw" };

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => service.RegisterUserAsync(registerDto));
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnUser_WhenCredentialsCorrect()
        {
            // Arrange
            using var context = CreateContext();
            var password = "mysecretpassword";
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User { UserId = 1, Username = "loginuser", Email = "login@example.com", PasswordHash = hash };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var loginDto = new UserLogin { Email = "login@example.com", Password = password };

            // Act
            var result = await service.LoginAsync(loginDto);

            // Assert
            Assert.Equal(1, result.UserId);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowUnauthorized_WhenPasswordWrong()
        {
            // Arrange
            using var context = CreateContext();
            var hash = BCrypt.Net.BCrypt.HashPassword("correct");
            var user = new User { UserId = 1, Username = "loginuser", Email = "login@example.com", PasswordHash = hash };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var loginDto = new UserLogin { Email = "login@example.com", Password = "wrong" };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(loginDto));
        }
        */

        [Fact]
        public async Task UpdateUserAsync_ShouldUpdate_WhenUserIsSelf()
        {
             // Arrange
            using var context = CreateContext();
            var user = new User { UserId = 1, Username = "original", Email = "original@example.com", PasswordHash = "hash" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = CreateService(context, currentUserId: 1); // Logged in as User 1
            var updateDto = new User { UserId = 1, Username = "updated", Email = "updated@example.com" };

            // Act
            await service.UpdateUserAsync(1, updateDto);

            // Assert
            var dbUser = await context.Users.FindAsync(1);
            Assert.Equal("updated", dbUser.Username);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldThrowForbid_WhenUserIsNotSelfAndNotAdmin()
        {
             // Arrange
            using var context = CreateContext();
            var user = new User { UserId = 1, Username = "victim", Email = "victim@example.com", PasswordHash = "hash" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = CreateService(context, currentUserId: 2); // Logged in as User 2 (Attacker)
            var updateDto = new User { UserId = 1, Username = "hacked", Email = "hacked@example.com" };

            // Act & Assert
            await Assert.ThrowsAsync<EduShelf.Api.Services.ForbidException>(() => service.UpdateUserAsync(1, updateDto));
        }
    }
}
