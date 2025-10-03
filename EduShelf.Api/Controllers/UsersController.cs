using EduShelf.Api.Data;
using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EduShelf.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ApiExplorerSettings(GroupName = "Users")]
    public class UsersController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly IConfiguration _configuration;

        public UsersController(ApiDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/Users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            return await _context.Users
                .Select(u => new UserDto { UserId = u.UserId, Username = u.Username, Email = u.Email })
                .ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var userDto = new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email
            };

            return userDto;
        }

        // POST: api/Users
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<UserDto>> PostUser([FromBody] UserRegister userRegister)
        {
            if (await _context.Users.AnyAsync(u => u.Username == userRegister.Username))
            {
                return Conflict("Username already exists.");
            }

            if (await _context.Users.AnyAsync(u => u.Email == userRegister.Email))
            {
                return Conflict("Email already exists.");
            }

            var user = new User
            {
                Username = userRegister.Username,
                Email = userRegister.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRegister.Password)
            };

            var schuelerRole = await _context.Roles.SingleOrDefaultAsync(r => r.Name == "Schüler");
            if (schuelerRole == null)
            {
                return StatusCode(500, "Default role not found.");
            }

            user.UserRoles.Add(new UserRole { Role = schuelerRole });

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userDto = new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, userDto);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] UserLogin login)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == login.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
            {
                return Unauthorized();
            }

            var token = GenerateJwtToken(user);

            var userDto = new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email
            };

            return Ok(new LoginResponseDto { Token = token, User = userDto });
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var userRole in user.UserRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetMe()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid user identifier.");
            }

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var userDto = new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email
            };

            return userDto;
        }

        [HttpPut("me/password")]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordChangeDto passwordChange)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid user identifier.");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (!BCrypt.Net.BCrypt.Verify(passwordChange.OldPassword, user.PasswordHash))
            {
                return BadRequest("Invalid old password.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordChange.NewPassword);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}/documents")]
        public async Task<ActionResult<IEnumerable<DocumentDto>>> GetUserDocuments(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var documents = await _context.Documents
                .Where(d => d.UserId == id)
                .Select(d => new DocumentDto
                {
                    Id = d.Id,
                    Title = d.Title,
                    FileType = d.FileType,
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync();

            return Ok(documents);
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User userUpdate)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (currentUserId != id.ToString() && !isAdmin)
            {
                return Forbid();
            }

            if (id != userUpdate.UserId)
            {
                return BadRequest();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Username = userUpdate.Username;
            user.Email = userUpdate.Email;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }

        [HttpGet("{id}/roles")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Role>>> GetUserRoles(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user.UserRoles.Select(ur => ur.Role));
        }

        [HttpPost("{id}/roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddUserRole(int id, [FromBody] UserRoleDto userRoleDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var role = await _context.Roles.FindAsync(userRoleDto.RoleId);
            if (role == null)
            {
                return NotFound("Role not found.");
            }

            var userRoleExists = await _context.UserRoles.AnyAsync(ur => ur.UserId == id && ur.RoleId == userRoleDto.RoleId);
            if (userRoleExists)
            {
                return Conflict("User already has this role.");
            }

            var userRole = new UserRole
            {
                UserId = id,
                RoleId = userRoleDto.RoleId
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}/roles/{roleId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveUserRole(int id, int roleId)
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == id && ur.RoleId == roleId);

            if (userRole == null)
            {
                return NotFound("User role not found.");
            }

            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class PasswordChangeDto
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}