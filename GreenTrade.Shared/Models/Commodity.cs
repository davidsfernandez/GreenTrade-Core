using System.ComponentModel.DataAnnotations;

namespace GreenTrade.Shared.Models;

public class Commodity
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string TickerSymbol { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string UnitOfMeasure { get; set; } = string.Empty;

    public List<PriceAlert> PriceAlerts { get; set; } = new();
}
