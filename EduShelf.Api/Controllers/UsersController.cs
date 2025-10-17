using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Models.Entities;
using EduShelf.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EduShelf.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ApiExplorerSettings(GroupName = "Users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/Users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _userService.GetUsersAsync();
            return Ok(users);
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _userService.GetUserAsync(id);
            return Ok(user);
        }

        // POST: api/Users
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<UserDto>> PostUser([FromBody] UserRegister userRegister)
        {
            var userDto = await _userService.RegisterUserAsync(userRegister);
            return CreatedAtAction(nameof(GetUser), new { id = userDto.UserId }, userDto);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] UserLogin login)
        {
            var loginResponse = await _userService.LoginAsync(login);
            return Ok(loginResponse);
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetMe()
        {
            var user = await _userService.GetMeAsync(User);
            return Ok(user);
        }

        [HttpPut("me/password")]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordChangeDto passwordChange)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid user identifier.");
            }
            await _userService.ChangePasswordAsync(userId, passwordChange);
            return NoContent();
        }

        [HttpGet("{id}/documents")]
        public async Task<ActionResult<IEnumerable<DocumentDto>>> GetUserDocuments(int id)
        {
            var documents = await _userService.GetUserDocumentsAsync(id);
            return Ok(documents);
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User userUpdate)
        {
            await _userService.UpdateUserAsync(id, userUpdate, User);
            return NoContent();
        }

        // PATCH: api/Users/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchUser(int id, [FromBody] UserUpdateDto userUpdate)
        {
            await _userService.PartialUpdateUserAsync(id, userUpdate, User);
            return NoContent();
        }
        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }

        [HttpGet("{id}/roles")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Role>>> GetUserRoles(int id)
        {
            var roles = await _userService.GetUserRolesAsync(id);
            return Ok(roles);
        }

        [HttpPost("{id}/roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddUserRole(int id, [FromBody] UserRoleDto userRoleDto)
        {
            await _userService.AddUserRoleAsync(id, userRoleDto);
            return Ok();
        }

        [HttpDelete("{id}/roles/{roleId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveUserRole(int id, int roleId)
        {
            await _userService.RemoveUserRoleAsync(id, roleId);
            return NoContent();
        }
    }
}