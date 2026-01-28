namespace EduShelf.Api.Models.Dtos
{
    public class UserDto
    {
        public int UserId { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
    }
}