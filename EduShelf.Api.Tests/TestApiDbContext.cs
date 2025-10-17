using EduShelf.Api.Data;
using EduShelf.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EduShelf.Api.Tests
{
    public class TestApiDbContext : ApiDbContext
    {
        public TestApiDbContext(DbContextOptions<ApiDbContext> options, IConfiguration configuration)
            : base(options, configuration)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Exclude the DocumentChunk entity to avoid issues with the Vector type in the in-memory database
            modelBuilder.Ignore<DocumentChunk>();
        }
    }
}