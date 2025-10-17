using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Models.Entities;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EduShelf.Api.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetUsersAsync();
        Task<UserDto> GetUserAsync(int id);
        Task<UserDto> RegisterUserAsync(UserRegister userRegister);
        Task<LoginResponseDto> LoginAsync(UserLogin login);
        Task<UserDto> GetMeAsync(ClaimsPrincipal user);
        Task ChangePasswordAsync(int userId, PasswordChangeDto passwordChange);
        Task<IEnumerable<DocumentDto>> GetUserDocumentsAsync(int id);
        Task UpdateUserAsync(int id, User userUpdate, ClaimsPrincipal user);
        Task PartialUpdateUserAsync(int id, UserUpdateDto userUpdate, ClaimsPrincipal user);
        Task DeleteUserAsync(int id);
        Task<IEnumerable<Role>> GetUserRolesAsync(int id);
        Task AddUserRoleAsync(int id, UserRoleDto userRoleDto);
        Task RemoveUserRoleAsync(int id, int roleId);
    }

    public class UserUpdateDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
    }

    public class PasswordChangeDto
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}