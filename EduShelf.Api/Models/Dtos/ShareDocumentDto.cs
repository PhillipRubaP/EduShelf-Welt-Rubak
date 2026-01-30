using System.ComponentModel.DataAnnotations;

namespace EduShelf.Api.Models.Dtos;

public class ShareDocumentDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
