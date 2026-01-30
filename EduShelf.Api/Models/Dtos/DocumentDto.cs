namespace EduShelf.Api.Models.Dtos;

public class DocumentDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string FileType { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TagDto> Tags { get; set; } = new();
    public int UserId { get; set; }
    public bool IsShared { get; set; }
    public string? OwnerName { get; set; }
}