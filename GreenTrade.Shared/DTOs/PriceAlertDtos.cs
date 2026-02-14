using System.ComponentModel.DataAnnotations;

namespace GreenTrade.Shared.DTOs;

public class CreatePriceAlertDto
{
    [Required]
    public int CommodityId { get; set; }

    [Required]
    [Range(0.01, 10000)]
    public decimal TargetPrice { get; set; }
}

public class PriceAlertDto
{
    public int Id { get; set; }
    public string CommodityName { get; set; } = string.Empty;
    public string Ticker { get; set; } = string.Empty;
    public decimal TargetPrice { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
