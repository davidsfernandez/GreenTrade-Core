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

    public AuthController(AppDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
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
}
