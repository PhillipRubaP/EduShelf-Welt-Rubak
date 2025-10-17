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
using System;

namespace EduShelf.Api.Tests
{
    public class TagsControllerTests
    {
        private async Task<(ApiDbContext, TagsController)> SetupTest(string role = "User")
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            var configurationMock = new Mock<IConfiguration>();

            var context = new TestApiDbContext(dbContextOptions, configurationMock.Object);
            
            context.Tags.AddRange(
                new Tag { Id = 1, Name = "Tag1" },
                new Tag { Id = 2, Name = "Tag2" },
                new Tag { Id = 3, Name = "Tag3" }
            );
            await context.SaveChangesAsync();

            var claims = new List<Claim>();

            if (!string.IsNullOrEmpty(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthentication"));
            
            var controller = new TagsController(context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                }
            };
            
            return (context, controller);
        }

        [Fact]
        public async Task GetTags_ShouldReturnAllTags()
        {
            // Arrange
            var (context, controller) = await SetupTest();

            // Act
            var result = await controller.GetTags();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Tag>>>(result);
            var tags = Assert.IsAssignableFrom<IEnumerable<Tag>>(actionResult.Value);
            Assert.Equal(3, tags.Count());

            // Cleanup
            await context.DisposeAsync();
        }

        [Fact]
        public async Task GetTag_WithValidId_ShouldReturnTag()
        {
            // Arrange
            var (context, controller) = await SetupTest();

            // Act
            var result = await controller.GetTag(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Tag>>(result);
            var tag = Assert.IsType<Tag>(actionResult.Value);
            Assert.Equal(1, tag.Id);

            // Cleanup
            await context.DisposeAsync();
        }

        [Fact]
        public async Task GetTag_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var (context, controller) = await SetupTest();

            // Act
            var result = await controller.GetTag(99);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);

            // Cleanup
            await context.DisposeAsync();
        }

        [Fact]
        public async Task PostTag_AsAdmin_ShouldCreateTag()
        {
            // Arrange
            var (context, controller) = await SetupTest("Admin");
            var newTag = new Tag { Name = "New Tag" };

            // Act
            var result = await controller.PostTag(newTag);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Tag>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var tag = Assert.IsType<Tag>(createdAtActionResult.Value);
            Assert.Equal("New Tag", tag.Name);
            Assert.Equal(4, context.Tags.Count());

            // Cleanup
            await context.DisposeAsync();
        }

        [Fact]
        public async Task PutTag_AsAdmin_ShouldUpdateTag()
        {
            // Arrange
            var (context, controller) = await SetupTest("Admin");
            var updatedTag = new Tag { Id = 1, Name = "Updated Tag" };

            // Act
            var result = await controller.PutTag(1, updatedTag);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var tag = await context.Tags.FindAsync(1);
            Assert.NotNull(tag);
            Assert.Equal("Updated Tag", tag.Name);

            // Cleanup
            await context.DisposeAsync();
        }

        [Fact]
        public async Task PatchTag_AsAdmin_ShouldUpdateTagName()
        {
            // Arrange
            var (context, controller) = await SetupTest("Admin");
            var tagUpdate = new TagUpdateDto { Name = "Patched Tag" };

            // Act
            var result = await controller.PatchTag(1, tagUpdate);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var tag = await context.Tags.FindAsync(1);
            Assert.NotNull(tag);
            Assert.Equal("Patched Tag", tag.Name);

            // Cleanup
            await context.DisposeAsync();
        }

        [Fact]
        public async Task DeleteTag_AsAdmin_ShouldDeleteTag()
        {
            // Arrange
            var (context, controller) = await SetupTest("Admin");

            // Act
            var result = await controller.DeleteTag(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.Equal(2, context.Tags.Count());
            var tag = await context.Tags.FindAsync(1);
            Assert.Null(tag);

            // Cleanup
            await context.DisposeAsync();
        }
    }
}