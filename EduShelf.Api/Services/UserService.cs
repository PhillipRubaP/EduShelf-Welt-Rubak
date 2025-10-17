using EduShelf.Api.Data;
using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Models.Entities;
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
using EduShelf.Api.Exceptions;

namespace EduShelf.Api.Services
{
    public class UserService : IUserService
    {
        private readonly ApiDbContext _context;
        private readonly IConfiguration _configuration;

        public UserService(ApiDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IEnumerable<UserDto>> GetUsersAsync()
        {
            return await _context.Users
                .Select(u => new UserDto { UserId = u.UserId, Username = u.Username, Email = u.Email })
                .ToListAsync();
        }

        public async Task<UserDto> GetUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            return new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email
            };
        }

        public async Task<UserDto> RegisterUserAsync(UserRegister userRegister)
        {
            if (await _context.Users.AnyAsync(u => u.Username == userRegister.Username))
            {
                throw new BadRequestException("Username already exists.");
            }

            if (await _context.Users.AnyAsync(u => u.Email == userRegister.Email))
            {
                throw new BadRequestException("Email already exists.");
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
                throw new Exception("Default role not found.");
            }

            user.UserRoles.Add(new UserRole { Role = schuelerRole });

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email
            };
        }

        public async Task<LoginResponseDto> LoginAsync(UserLogin login)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == login.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            var token = GenerateJwtToken(user);

            var userDto = new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email
            };

            return new LoginResponseDto { Token = token, User = userDto };
        }

        public async Task<UserDto> GetMeAsync(ClaimsPrincipal userPrincipal)
        {
            var userIdString = userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid user identifier.");
            }

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            return new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email
            };
        }

        public async Task ChangePasswordAsync(int userId, PasswordChangeDto passwordChange)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            if (!BCrypt.Net.BCrypt.Verify(passwordChange.OldPassword, user.PasswordHash))
            {
                throw new BadRequestException("Invalid old password.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordChange.NewPassword);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<DocumentDto>> GetUserDocumentsAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            return await _context.Documents
                .Where(d => d.UserId == id)
                .Select(d => new DocumentDto
                {
                    Id = d.Id,
                    Title = d.Title,
                    FileType = d.FileType,
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync();
        }

        public async Task UpdateUserAsync(int id, User userUpdate, ClaimsPrincipal userPrincipal)
        {
            var currentUserId = userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = userPrincipal.IsInRole("Admin");

            if (currentUserId != id.ToString() && !isAdmin)
            {
                throw new ForbidException("You are not authorized to update this user.");
            }

            if (id != userUpdate.UserId)
            {
                throw new BadRequestException("User ID mismatch.");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            user.Username = userUpdate.Username;
            user.Email = userUpdate.Email;

            await _context.SaveChangesAsync();
        }

        public async Task PartialUpdateUserAsync(int id, UserUpdateDto userUpdate, ClaimsPrincipal userPrincipal)
        {
            var currentUserId = userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = userPrincipal.IsInRole("Admin");

            if (currentUserId != id.ToString() && !isAdmin)
            {
                throw new ForbidException("You are not authorized to update this user.");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            if (!string.IsNullOrEmpty(userUpdate.Username))
            {
                user.Username = userUpdate.Username;
            }

            if (!string.IsNullOrEmpty(userUpdate.Email))
            {
                user.Email = userUpdate.Email;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Role>> GetUserRolesAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            return user.UserRoles.Select(ur => ur.Role);
        }

        public async Task AddUserRoleAsync(int id, UserRoleDto userRoleDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            var role = await _context.Roles.FindAsync(userRoleDto.RoleId);
            if (role == null)
            {
                throw new NotFoundException("Role not found.");
            }

            var userRoleExists = await _context.UserRoles.AnyAsync(ur => ur.UserId == id && ur.RoleId == userRoleDto.RoleId);
            if (userRoleExists)
            {
                throw new BadRequestException("User already has this role.");
            }

            var userRole = new UserRole
            {
                UserId = id,
                RoleId = userRoleDto.RoleId
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveUserRoleAsync(int id, int roleId)
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == id && ur.RoleId == roleId);

            if (userRole == null)
            {
                throw new NotFoundException("User role not found.");
            }

            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();
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
    }

    public class ForbidException : Exception
    {
        public ForbidException(string message) : base(message) { }
    }
}