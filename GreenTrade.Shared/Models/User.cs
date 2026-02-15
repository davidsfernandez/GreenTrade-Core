using System.ComponentModel.DataAnnotations;
using GreenTrade.Shared.Enums;

namespace GreenTrade.Shared.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<PriceAlert> PriceAlerts { get; set; } = new();

    // Password Recovery Fields
    public string? PasswordResetToken { get; set; }
    public DateTime? ResetTokenExpires { get; set; }
}
