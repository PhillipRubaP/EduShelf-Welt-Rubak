using Xunit;
using EduShelf.Api.Services;
using EduShelf.Api.Models.Entities;
using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Data;
using EduShelf.Api.Exceptions;
using EduShelf.Api.Constants;
using EduShelf.Api.Services.FileStorage;
using EduShelf.Api.Services.Background;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System;
using Microsoft.Extensions.Configuration;

namespace EduShelf.Api.Tests
{
    public class DocumentServiceTests
    {
        private readonly Mock<IBackgroundJobQueue> _mockQueue;
        private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
        private readonly Mock<IImageProcessingService> _mockImageProcessing;
        private readonly Mock<IFileStorageService> _mockFileStorage;
        private readonly Mock<IConfiguration> _mockConfiguration;

        public DocumentServiceTests()
        {
            _mockQueue = new Mock<IBackgroundJobQueue>();
            _mockScopeFactory = new Mock<IServiceScopeFactory>();
            _mockImageProcessing = new Mock<IImageProcessingService>();
            _mockFileStorage = new Mock<IFileStorageService>();
            _mockConfiguration = new Mock<IConfiguration>();
        }

        private ApiDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new TestApiDbContext(options, _mockConfiguration.Object);
        }

        private DocumentService CreateService(ApiDbContext context)
        {
            return new DocumentService(
                context, 
                _mockQueue.Object, 
                _mockScopeFactory.Object, 
                _mockImageProcessing.Object, 
                _mockFileStorage.Object);
        }

        [Fact]
        public async Task UploadDocumentAsync_ValidFile_ShouldUploadAndCreateEntity()
        {
            // Arrange
            using var context = CreateContext();
            var service = CreateService(context);
            
            var fileStream = new MemoryStream(new byte[] { 1, 2, 3 });
            var fileName = "test.pdf";
            var contentType = "application/pdf";
            var userId = 1;
            var tags = new List<string> { "science" };

            _mockFileStorage.Setup(f => f.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), contentType))
                .ReturnsAsync("uploaded/file/path.pdf");

            // Act
            var result = await service.UploadDocumentAsync(fileStream, fileName, contentType, userId, tags);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test", result.Title);
            Assert.Equal("pdf", result.FileType);
            Assert.Single(result.Tags);
            Assert.Equal("science", result.Tags[0].Name);
            
            Assert.Single(context.Documents);
            _mockFileStorage.Verify(x => x.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), contentType), Times.Once);
            _mockQueue.Verify(x => x.QueueBackgroundWorkItemAsync(It.IsAny<Func<System.Threading.CancellationToken, ValueTask>>()), Times.Once);
        }

        [Fact]
        public async Task UploadDocumentAsync_InvalidExtension_ShouldThrowBadRequest()
        {
            // Arrange
            using var context = CreateContext();
            var service = CreateService(context);
            var fileStream = new MemoryStream();

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => 
                service.UploadDocumentAsync(fileStream, "test.exe", "application/octet-stream", 1, null));
        }

        [Fact]
        public async Task GetDocumentAsync_AsOwner_ShouldReturnDocument()
        {
            // Arrange
            using var context = CreateContext();
            var document = new Document { Id = 1, Title = "My Doc", UserId = 1, FileType = "pdf", Path = "path/to/doc", CreatedAt = DateTime.UtcNow };
            context.Documents.Add(document);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            var result = await service.GetDocumentAsync(1, 1, Roles.Student);

            // Assert
            Assert.Equal("My Doc", result.Title);
        }

        [Fact]
        public async Task GetDocumentAsync_AsOtherUser_ShouldThrowForbid()
        {
            // Arrange
            using var context = CreateContext();
            var document = new Document { Id = 1, Title = "My Doc", UserId = 1, FileType = "pdf", Path = "path/to/doc", CreatedAt = DateTime.UtcNow };
            context.Documents.Add(document);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act & Assert
            await Assert.ThrowsAsync<ForbidException>(() => service.GetDocumentAsync(1, 2, Roles.Student));
        }

         [Fact]
        public async Task DeleteDocumentAsync_AsOwner_ShouldRemoveEntityAndFile()
        {
            // Arrange
            using var context = CreateContext();
            var document = new Document { Id = 1, Title = "My Doc", UserId = 1, FileType = "pdf", Path = "123.pdf", CreatedAt = DateTime.UtcNow };
            context.Documents.Add(document);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Act
            await service.DeleteDocumentAsync(1, 1, Roles.Student);

            // Assert
            Assert.Empty(context.Documents);
            _mockFileStorage.Verify(x => x.DeleteFileAsync("123.pdf"), Times.Once);
        }
    }
}
