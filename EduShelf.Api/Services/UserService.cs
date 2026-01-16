using EduShelf.Api.Data;
using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EduShelf.Api.Exceptions;

using EduShelf.Api.Exceptions;
using EduShelf.Api.Constants;
{
    public class UserService : IUserService
    {
        private readonly ApiDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(ApiDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetCurrentUserId()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated or user ID is invalid.");
            }
            return userId;
        }

        private bool IsCurrentUserAdmin()
        {
            return _httpContextAccessor.HttpContext?.User.IsInRole(Roles.Admin) ?? false;
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

        public async Task UpdateUserAsync(int id, User userUpdate)
        {
            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();

            if (currentUserId != id && !isAdmin)
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

        public async Task PartialUpdateUserAsync(int id, UserUpdateDto userUpdate)
        {
            var currentUserId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();

            if (currentUserId != id && !isAdmin)
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
    }

    public class ForbidException : Exception
    {
        public ForbidException(string message) : base(message) { }
    }
}