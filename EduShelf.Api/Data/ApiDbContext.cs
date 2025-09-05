using EduShelf.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;

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
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<DocumentChunk> DocumentChunks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");
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

        modelBuilder.Entity<Document>()
            .HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Favourite>()
            .HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AccessLog>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChatMessage>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Flashcard>()
            .HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Quiz>()
            .HasOne(q => q.User)
            .WithMany()
            .HasForeignKey(q => q.UserId)
            .OnDelete(DeleteBehavior.Cascade);

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

        modelBuilder.Entity<User>().HasData(
            new User
            {
                UserId = 2,
                Username = "Student User",
                Email = "student@edushelf.com",
                PasswordHash = "placeholder_hash", // Use a proper password hasher
                Role = "Student",
                CreatedAt = DateTime.UtcNow
            }
        );

        modelBuilder.Entity<Document>().HasData(
            new Document { Id = 1, UserId = 1, Title = "Algebra Basics", Path = "/documents/algebra.pdf", FileType = "pdf", CreatedAt = DateTime.UtcNow },
            new Document { Id = 2, UserId = 2, Title = "Introduction to Physics", Path = "/documents/physics.pdf", FileType = "pdf", CreatedAt = DateTime.UtcNow }
        );

        modelBuilder.Entity<DocumentTag>().HasData(
            new DocumentTag { DocumentId = 1, TagId = 1 }, // Algebra -> Mathematics
            new DocumentTag { DocumentId = 2, TagId = 2 }  // Physics Intro -> Physics
        );

        modelBuilder.Entity<Flashcard>().HasData(
            new Flashcard { Id = 1, UserId = 1, DocumentId = 1, Question = "What is 2+2?", Answer = "4", CreatedAt = DateTime.UtcNow },
            new Flashcard { Id = 2, UserId = 1, DocumentId = 1, Question = "What is x in x+5=10?", Answer = "5", CreatedAt = DateTime.UtcNow }
        );

        modelBuilder.Entity<Quiz>().HasData(
            new Quiz { Id = 1, UserId = 2, DocumentId = 2, CreatedAt = DateTime.UtcNow }
        );

        modelBuilder.Entity<DocumentChunk>()
            .HasIndex(dc => dc.Embedding)
            .HasMethod("hnsw")
            .HasOperators("vector_l2_ops");
    }
}