using System.Security.Claims;
using EduShelf.Api.Constants;
using EduShelf.Api.Data;
using EduShelf.Api.Exceptions;
using EduShelf.Api.Models.Dtos;
using EduShelf.Api.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Configuration;

namespace EduShelf.Api.Services;

public class AuthService : IAuthService
{
    private readonly ApiDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public AuthService(ApiDbContext context, IHttpContextAccessor httpContextAccessor, IEmailService emailService, IConfiguration configuration)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _emailService = emailService;
        _configuration = configuration;
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

    public async Task<UserDto> LoginAsync(UserLogin login)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == login.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (!user.IsEmailConfirmed)
        {
            throw new UnauthorizedAccessException("Email not confirmed.");
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username)
        };

        foreach (var userRole in user.UserRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
        }

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(120)
        };

        if (_httpContextAccessor.HttpContext != null)
        {
            await _httpContextAccessor.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
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
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRegister.Password),
            IsEmailConfirmed = false,
            EmailConfirmationToken = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)),
            EmailConfirmationTokenExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        var studentRole = await _context.Roles.SingleOrDefaultAsync(r => r.Name == Roles.Student);
        if (studentRole == null)
        {
            throw new Exception($"Default role '{Roles.Student}' not found.");
        }

        user.UserRoles.Add(new UserRole { Role = studentRole });

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        try
        {
            var appUrl = _configuration["AppUrl"] ?? "http://localhost:5173";
            var confirmationLink = $"{appUrl}/confirm-email?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(user.EmailConfirmationToken)}";
            var emailBody = $@"
                <h2>Welcome to EduShelf!</h2>
                <p>Please confirm your email by clicking the link below:</p>
                <p><a href='{confirmationLink}'>Confirm Email</a></p>
                <p>Or copy and paste this link into your browser:</p>
                <p>{confirmationLink}</p>";

            await _emailService.SendEmailAsync(user.Email, "Confirm your email", emailBody);
        }
        catch (Exception ex)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            throw new Exception($"Registration failed: Could not send confirmation email. Error: {ex.Message}");
        }

        return new UserDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email
        };
    }

    public async Task LogoutAsync()
    {
        if (_httpContextAccessor.HttpContext != null)
        {
            await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }

    public async Task ChangePasswordAsync(int userId, PasswordChangeDto passwordChange)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordChange.NewPassword);
        await _context.SaveChangesAsync();
    }

    public async Task<UserDto> GetMeAsync()
    {
        var userId = GetCurrentUserId();
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

    public async Task ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == confirmEmailDto.Email);
        if (user == null)
        {
             throw new NotFoundException("User not found.");
        }

        if (user.IsEmailConfirmed)
        {
             return;
        }

        if (user.EmailConfirmationToken != confirmEmailDto.Token)
        {
             throw new BadRequestException("Invalid token.");
        }

        if (user.EmailConfirmationTokenExpiresAt < DateTime.UtcNow)
        {
             throw new BadRequestException("Token expired.");
        }

        user.IsEmailConfirmed = true;
        user.EmailConfirmationToken = null;
        user.EmailConfirmationTokenExpiresAt = null;
        await _context.SaveChangesAsync();
    }
}
