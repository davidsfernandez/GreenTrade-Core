using System.ComponentModel.DataAnnotations;

namespace GreenTrade.Shared.Models;

/// <summary>
/// Global configuration settings for market calculations.
/// Implemented as a Singleton-like record in the database.
/// </summary>
public class MarketSettings
{
    [Key]
    public int Id { get; set; } = 1; // Always 1 for singleton record

    public decimal CoffeeBasis { get; set; } = 0m; // Default basis in BRL
    
    public decimal ServiceFeePercentage { get; set; } = 0.5m; // Example fee

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
