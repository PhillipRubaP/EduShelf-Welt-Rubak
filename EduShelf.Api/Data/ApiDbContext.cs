using EduShelf.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduShelf.Api.Data;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentTag> DocumentTags { get; set; }
    public DbSet<Favourite> Favourites { get; set; }
    public DbSet<AccessLog> AccessLogs { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<Flashcard> Flashcards { get; set; }
    public DbSet<Quiz> Quizzes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure unique constraint for Email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.Name)
            .IsUnique();

        modelBuilder.Entity<DocumentTag>()
            .HasKey(dt => new { dt.DocumentId, dt.TagId });

        modelBuilder.Entity<Favourite>()
            .HasKey(f => new { f.UserId, f.DocumentId });

        // Seed initial data
        modelBuilder.Entity<User>().HasData(
            new User
            {
                UserId = 1,
                Username = "Admin User",
                Email = "admin@edushelf.com",
                // You should use a proper password hasher in a real application
                // This is a placeholder and is not secure.
                PasswordHash = "placeholder_hash",
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            }
        );

        modelBuilder.Entity<Tag>().HasData(
            new Tag { Id = 1, Name = "Mathematics" },
            new Tag { Id = 2, Name = "Physics" },
            new Tag { Id = 3, Name = "Chemistry" },
            new Tag { Id = 4, Name = "Biology" },
            new Tag { Id = 5, Name = "History" },
            new Tag { Id = 6, Name = "Computer Science" }
        );
    }
}