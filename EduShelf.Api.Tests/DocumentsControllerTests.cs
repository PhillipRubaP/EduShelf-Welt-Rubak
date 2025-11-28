using Xunit;
using EduShelf.Api.Controllers;
using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using EduShelf.Api.Services;
using System;
using EduShelf.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EduShelf.Api.Tests
{
    public class DocumentsControllerTests
    {
        private readonly Mock<IndexingService> _mockIndexingService;
        private readonly Mock<IImageProcessingService> _mockImageProcessingService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
        private readonly Mock<ILogger<IndexingService>> _mockLogger;
        private readonly DocumentsController _controller;
        private readonly ApiDbContext _context;

        public DocumentsControllerTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(c => c["FileStorage:UploadPath"]).Returns("Uploads");

            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new TestApiDbContext(options, _mockConfiguration.Object);

            _mockImageProcessingService = new Mock<IImageProcessingService>();
            _mockScopeFactory = new Mock<IServiceScopeFactory>();
            _mockLogger = new Mock<ILogger<IndexingService>>();

            // Pass mocked dependencies to IndexingService constructor
            _mockIndexingService = new Mock<IndexingService>(
                _mockScopeFactory.Object, 
                _mockLogger.Object, 
                _mockImageProcessingService.Object, 
                _mockConfiguration.Object);
            
            _controller = new DocumentsController(_context, _mockIndexingService.Object, _mockImageProcessingService.Object, _mockConfiguration.Object);
        }

        private void SetupUser(int userId, string role = "User")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task GetDocuments_ShouldReturnDocuments_ForUser()
        {
            // Arrange
            SetupUser(1);
            var doc1 = new Document { Id = 1, UserId = 1, Title = "Doc 1", FileType = "pdf", CreatedAt = DateTime.UtcNow, Path = "path1" };
            var doc2 = new Document { Id = 2, UserId = 2, Title = "Doc 2", FileType = "pdf", CreatedAt = DateTime.UtcNow, Path = "path2" };
            _context.Documents.AddRange(doc1, doc2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetDocuments();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<DocumentDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var documents = Assert.IsAssignableFrom<IEnumerable<DocumentDto>>(okResult.Value);
            Assert.Single(documents); // Should only see user 1's document
            Assert.Contains(documents, d => d.Title == "Doc 1");
        }

        [Fact]
        public async Task DeleteDocument_ShouldRemoveDocument_WhenUserOwnsIt()
        {
            // Arrange
            SetupUser(1);
            var doc = new Document { Id = 1, UserId = 1, Title = "Doc 1", FileType = "pdf", CreatedAt = DateTime.UtcNow, Path = "testfile.pdf" };
            _context.Documents.Add(doc);
            await _context.SaveChangesAsync();

            // Create a dummy file to simulate existence
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            Directory.CreateDirectory(uploadPath);
            var filePath = Path.Combine(uploadPath, "testfile.pdf");
            await System.IO.File.WriteAllTextAsync(filePath, "dummy content");

            try 
            {
                // Act
                var result = await _controller.DeleteDocument(1);

                // Assert
                Assert.IsType<NoContentResult>(result);
                var deletedDoc = await _context.Documents.FindAsync(1);
                Assert.Null(deletedDoc);
                Assert.False(System.IO.File.Exists(filePath));
            }
            finally
            {
                // Cleanup
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                if (Directory.Exists(uploadPath)) Directory.Delete(uploadPath, true);
            }
        }

        [Fact]
        public async Task DeleteDocument_ShouldReturnForbid_WhenUserDoesNotOwnIt()
        {
            // Arrange
            SetupUser(2); // Different user
            var doc = new Document { Id = 1, UserId = 1, Title = "Doc 1", FileType = "pdf", CreatedAt = DateTime.UtcNow, Path = "path1" };
            _context.Documents.Add(doc);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteDocument(1);

            // Assert
            Assert.IsType<ForbidResult>(result);
            var existingDoc = await _context.Documents.FindAsync(1);
            Assert.NotNull(existingDoc);
        }
    }
}
