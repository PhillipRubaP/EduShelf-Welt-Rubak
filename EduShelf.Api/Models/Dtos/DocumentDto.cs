namespace EduShelf.Api.Models.Dtos;

public class DocumentDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string FileType { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TagDto> Tags { get; set; }
    public int UserId { get; set; }
}