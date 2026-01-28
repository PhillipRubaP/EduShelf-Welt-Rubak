namespace EduShelf.Api.Models.Entities
{
    public class Intent
    {
        public required string Type { get; set; }
        public string? DocumentName { get; set; }
    }
}