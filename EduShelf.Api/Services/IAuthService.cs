using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Models.Entities;

namespace EduShelf.Api.Services;

public interface IAuthService
{
    Task<UserDto> LoginAsync(UserLogin login);
    Task<UserDto> RegisterUserAsync(UserRegister userRegister);
    Task LogoutAsync();
    Task ChangePasswordAsync(int userId, PasswordChangeDto passwordChange);
    Task<UserDto> GetMeAsync();
    Task ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto);
}
