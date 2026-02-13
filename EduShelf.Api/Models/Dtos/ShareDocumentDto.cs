using System.ComponentModel.DataAnnotations;

namespace EduShelf.Api.Models.Dtos;

public class ShareDocumentDto
{
    [Required]
    public string Email { get; set; } = string.Empty;
}
