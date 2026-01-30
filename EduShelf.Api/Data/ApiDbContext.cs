using EduShelf.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
using EduShelf.Api.Constants;

namespace EduShelf.Api.Data;

public class ApiDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    public ApiDbContext(DbContextOptions<ApiDbContext> options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentTag> DocumentTags { get; set; }
    public DbSet<DocumentShare> DocumentShares { get; set; }
    public DbSet<Favourite> Favourites { get; set; }
    public DbSet<AccessLog> AccessLogs { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<ChatSession> ChatSessions { get; set; }
    public DbSet<Flashcard> Flashcards { get; set; }
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<DocumentChunk> DocumentChunks { get; set; }
    public DbSet<FlashcardTag> FlashcardTags { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.Name)
            .IsUnique();

        modelBuilder.Entity<DocumentTag>()
            .HasKey(dt => new { dt.DocumentId, dt.TagId });

        modelBuilder.Entity<DocumentShare>()
            .HasIndex(ds => new { ds.DocumentId, ds.UserId })
            .IsUnique();

        modelBuilder.Entity<FlashcardTag>()
            .HasKey(ft => new { ft.FlashcardId, ft.TagId });

        modelBuilder.Entity<Favourite>()
            .HasKey(f => new { f.UserId, f.DocumentId });

        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);
 
        modelBuilder.Entity<Document>()
            .HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DocumentShare>()
            .HasOne(ds => ds.Document)
            .WithMany()
            .HasForeignKey(ds => ds.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DocumentShare>()
            .HasOne(ds => ds.User)
            .WithMany()
            .HasForeignKey(ds => ds.UserId)
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

        modelBuilder.Entity<ChatSession>()
            .HasOne(cs => cs.User)
            .WithMany()
            .HasForeignKey(cs => cs.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChatMessage>()
            .HasOne(cm => cm.ChatSession)
            .WithMany(cs => cs.ChatMessages)
            .HasForeignKey(cm => cm.ChatSessionId)
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
        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 1, Name = EduShelf.Api.Constants.Roles.Admin },
            new Role { RoleId = 2, Name = EduShelf.Api.Constants.Roles.Student }
        );

        modelBuilder.Entity<User>().HasData(
            new User
            {
                UserId = 1,
                Username = "Admin User",
                Email = "admin@edushelf.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("AdminPassword123!"),
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                UserId = 2,
                Username = "Student User",
                Email = "student@edushelf.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("StudentPassword123!"),
                CreatedAt = DateTime.UtcNow
            }
        );

        modelBuilder.Entity<UserRole>().HasData(
            new UserRole { UserId = 1, RoleId = 1 },
            new UserRole { UserId = 2, RoleId = 2 }
        );

        modelBuilder.Entity<Tag>().HasData(
            new Tag { Id = 1, Name = "Mathematics" },
            new Tag { Id = 2, Name = "Physics" },
            new Tag { Id = 3, Name = "Chemistry" },
            new Tag { Id = 4, Name = "Biology" },
            new Tag { Id = 5, Name = "History" },
            new Tag { Id = 6, Name = "Computer Science" }
        );

        modelBuilder.Entity<Document>().HasData(
            new Document { Id = 1, UserId = 1, Title = "Algebra Basics", Path = "/documents/algebra.pdf", FileType = "pdf", CreatedAt = DateTime.UtcNow },
            new Document { Id = 2, UserId = 2, Title = "Introduction to Physics", Path = "/documents/physics.pdf", FileType = "pdf", CreatedAt = DateTime.UtcNow }
        );

        modelBuilder.Entity<DocumentTag>().HasData(
            new DocumentTag { DocumentId = 1, TagId = 1 },
            new DocumentTag { DocumentId = 2, TagId = 2 }  
        );

        modelBuilder.Entity<Flashcard>().HasData(
            new Flashcard { Id = 1, UserId = 1, Question = "What is 2+2?", Answer = "4", CreatedAt = DateTime.UtcNow },
            new Flashcard { Id = 2, UserId = 1, Question = "What is x in x+5=10?", Answer = "5", CreatedAt = DateTime.UtcNow }
        );

        modelBuilder.Entity<FlashcardTag>().HasData(
            new FlashcardTag { FlashcardId = 1, TagId = 1 },
            new FlashcardTag { FlashcardId = 2, TagId = 1 }
        );

        modelBuilder.Entity<Quiz>().HasData(
            new Quiz { Id = 1, UserId = 2, Title = "Math Quiz", CreatedAt = DateTime.UtcNow }
        );

        modelBuilder.Entity<Question>().HasData(
            new Question { Id = 1, QuizId = 1, Text = "What is 2 + 2?" }
        );

        modelBuilder.Entity<Answer>().HasData(
            new Answer { Id = 1, QuestionId = 1, Text = "4", IsCorrect = true },
            new Answer { Id = 2, QuestionId = 1, Text = "3", IsCorrect = false },
            new Answer { Id = 3, QuestionId = 1, Text = "5", IsCorrect = false }
        );

        modelBuilder.Entity<DocumentChunk>()
            .HasIndex(dc => dc.Embedding)
            .HasMethod("hnsw")
            .HasOperators("vector_l2_ops");
    }
}