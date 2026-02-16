using GreenTrade.Server.Data;
using GreenTrade.Shared.DTOs;
using GreenTrade.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenTrade.Server.Controllers;

/// <summary>
/// Controller for managing users.
/// Protected by JWT authorization.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _context.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role.ToString()
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString()
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

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email)) return Unauthorized();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return NotFound();

        user.FullName = request.FullName;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<string>.SuccessResponse(string.Empty, "Perfil atualizado com sucesso"));
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email)) return Unauthorized();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return BadRequest(ApiResponse<string>.ErrorResponse("Senha atual incorreta"));
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<string>.SuccessResponse(string.Empty, "Senha alterada com sucesso"));
    }
}
