using System.ComponentModel.DataAnnotations;
using GreenTrade.Shared.Enums;

namespace GreenTrade.Shared.DTOs;

public class CoffeeLotDto
{
    public int Id { get; set; }
    public string CommodityName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string CropYear { get; set; } = string.Empty;
    public string Quality { get; set; } = string.Empty; // Enum as string
    public string WarehouseName { get; set; } = string.Empty;
    public string WarehouseCity { get; set; } = string.Empty;
    public decimal? AskingPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateCoffeeLotDto
{
    [Required]
    public int CommodityId { get; set; }

    [Required]
    [Range(1, 100000, ErrorMessage = "Quantidade deve ser maior que 0")]
    public int Quantity { get; set; }

    [Required]
    public string CropYear { get; set; } = string.Empty;

    [Required]
    public CoffeeQuality Quality { get; set; }

    public string ScreenSize { get; set; } = string.Empty;
    public int Defects { get; set; }

    [Required]
    public int WarehouseId { get; set; }
    
    // [Required]
    // public string WarehouseName { get; set; } = string.Empty;
    
    // [Required]
    // public string WarehouseCity { get; set; } = string.Empty;

    public decimal? AskingPrice { get; set; }
    public string Description { get; set; } = string.Empty;

    public List<int> CertificationIds { get; set; } = new();
}
