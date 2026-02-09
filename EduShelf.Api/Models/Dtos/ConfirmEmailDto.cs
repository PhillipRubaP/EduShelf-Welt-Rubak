using System.ComponentModel.DataAnnotations;

namespace EduShelf.Api.Models.Dtos;

public class ConfirmEmailDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;
}
