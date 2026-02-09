using Xunit;
using EduShelf.Api.Controllers;
using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using EduShelf.Api.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace EduShelf.Api.Tests
{
    public class DocumentsControllerTests
    {
        private readonly Mock<IDocumentService> _mockDocumentService;
        private readonly DocumentsController _controller;

        public DocumentsControllerTests()
        {
            _mockDocumentService = new Mock<IDocumentService>();
            _controller = new DocumentsController(_mockDocumentService.Object);
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
        public async Task GetDocuments_ShouldReturnDocuments_FromService()
        {
            // Arrange
            SetupUser(1);
            var expectedDocs = new PagedResult<DocumentDto>
            {
                Items = new List<DocumentDto>
                {
                    new DocumentDto { Id = 1, Title = "Doc 1", FileType = "application/pdf", Tags = new List<TagDto>() },
                    new DocumentDto { Id = 2, Title = "Doc 2", FileType = "application/pdf", Tags = new List<TagDto>() }
                },
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 10
            };

            _mockDocumentService.Setup(s => s.GetDocumentsAsync(1, "User", 1, 10))
                .ReturnsAsync(expectedDocs);

            // Act
            var result = await _controller.GetDocuments(1, 10);

            // Assert
            var actionResult = Assert.IsType<ActionResult<PagedResult<DocumentDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedResult = Assert.IsType<PagedResult<DocumentDto>>(okResult.Value);
            Assert.Equal(2, returnedResult.TotalCount);
            Assert.Equal(2, returnedResult.Items.Count());
        }

        [Fact]
        public async Task PostDocument_ShouldUploadDocument_AndReturnCreated()
        {
            // Arrange
            SetupUser(1);
            var fileMock = new Mock<IFormFile>();
            var content = "Fake Content";
            var fileName = "test.txt";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;
            fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(ms.Length);
            fileMock.Setup(_ => _.ContentType).Returns("text/plain");

            var tags = new List<string> { "tag1" };
            var expectedDto = new DocumentDto { 
                Id = 1, 
                Title = fileName, 
                FileType = "text/plain", 
                Tags = new List<TagDto>() 
            };

            _mockDocumentService.Setup(s => s.UploadDocumentAsync(It.IsAny<Stream>(), fileName, "text/plain", 1, tags))
                .ReturnsAsync(expectedDto);

            // Act
            var result = await _controller.PostDocument(fileMock.Object, tags);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnDto = Assert.IsType<DocumentDto>(createdAtActionResult.Value);
            Assert.Equal(expectedDto.Id, returnDto.Id);
        }

        [Fact]
        public async Task DeleteDocument_ShouldCallServiceDelete()
        {
            // Arrange
            SetupUser(1);
            
            // Act
            var result = await _controller.DeleteDocument(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockDocumentService.Verify(s => s.DeleteDocumentAsync(1, 1, "User"), Times.Once);
        }
    }
}
