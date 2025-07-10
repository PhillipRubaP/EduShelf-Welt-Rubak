using System.ComponentModel.DataAnnotations;

namespace EduShelf.Api.Models.Entities;

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "Schüler";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}