using System.ComponentModel.DataAnnotations;
using GreenTrade.Shared.Enums;

namespace GreenTrade.Shared.Models;

public class CoffeeLot
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; } // Owner
    public User? User { get; set; }

    [Required]
    public int CommodityId { get; set; } // Arabica or Conilon
    public Commodity? Commodity { get; set; }

    [Required]
    [Range(1, 100000)]
    public int Quantity { get; set; } // In 60kg Bags

    [Required]
    [MaxLength(20)]
    public string CropYear { get; set; } = string.Empty; // e.g. "2023/2024"

    // Quality Specs
    public CoffeeQuality Quality { get; set; }
    
    [MaxLength(50)]
    public string ScreenSize { get; set; } = string.Empty; // Peneira (e.g. "16 up")
    
    [Range(0, 100)]
    public int Defects { get; set; } // Number of defects (COB)

    // Logistics
    // [Required]
    // [MaxLength(100)]
    // public string WarehouseName { get; set; } = string.Empty;
    
    // [Required]
    // [MaxLength(100)]
    // public string WarehouseCity { get; set; } = string.Empty;

    public int? WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    // Commercial
    public decimal? AskingPrice { get; set; } // Preço Pedido (por saca)
    public LotStatus Status { get; set; } = LotStatus.Draft;

    // Certifications (Simple approach: stored as comma-separated IDs or bitmask, 
    // but for EF Core simpler to have a separate collection or just a string for MVP. 
    // Let's use a simple collection of Enums if possible, or just a helper string).
    // For MVP/EF Core simplicity without creating a many-to-many join table manually right now:
    // public List<CoffeeCertification> Certifications { get; set; } = new();
    
    public List<LotCertification> LotCertifications { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
