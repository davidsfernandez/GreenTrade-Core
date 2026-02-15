using GreenTrade.Server.Data;
using GreenTrade.Server.Services;
using GreenTrade.Shared.DTOs;
using GreenTrade.Shared.Enums;
using GreenTrade.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenTrade.Server.Controllers;

/// <summary>
/// Controller for handling user authentication and registration.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;

    public AuthController(AppDbContext context, ITokenService tokenService, IEmailService emailService)
    {
        _context = context;
        _tokenService = tokenService;
        _emailService = emailService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new LoginResponse { Success = false, Message = "Invalid credentials" });
        }

        var token = _tokenService.CreateToken(user);

        return Ok(new LoginResponse
        {
            Success = true,
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString()
            }
        });
    }

    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest(new LoginResponse { Success = false, Message = "Email already registered" });
        }

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Buyer // Default role
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _tokenService.CreateToken(user);

        return Ok(new LoginResponse
        {
            Success = true,
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString()
            }
        });
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email)) return Unauthorized();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return NotFound();

        return Ok(new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString()
        });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
        {
            // Don't reveal that the user doesn't exist
            return Ok(new { message = "If the email is registered, a reset link will be sent." });
        }

        // Generate Token
        var token = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
        user.PasswordResetToken = token;
        user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);
        await _context.SaveChangesAsync();

        // Send Email
        var resetLink = $"{Request.Scheme}://{Request.Host}/reset-password?token={token}";
        var emailBody = $@"
            <h3>Recuperação de Senha - GreenTrade</h3>
            <p>Você solicitou a redefinição de sua senha.</p>
            <p>Clique no link abaixo para criar uma nova senha:</p>
            <a href='{resetLink}'>Redefinir Minha Senha</a>
            <p>Se você não solicitou isso, ignore este email.</p>
            <p><small>Este link expira em 1 hora.</small></p>
        ";

        await _emailService.SendEmailAsync(user.Email, "Recuperação de Senha GreenTrade", emailBody);

        return Ok(new { message = "If the email is registered, a reset link will be sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token);

        if (user == null || user.ResetTokenExpires < DateTime.UtcNow)
        {
            return BadRequest("Invalid or expired token.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.PasswordResetToken = null;
        user.ResetTokenExpires = null;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Password reset successfully." });
    }
}
