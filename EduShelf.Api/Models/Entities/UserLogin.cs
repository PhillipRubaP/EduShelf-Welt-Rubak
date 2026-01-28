namespace EduShelf.Api.Models.Entities
{
    public class UserLogin
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}