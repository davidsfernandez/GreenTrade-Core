using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenTrade.Shared.Models;

public class PriceAlert
{
    public int Id { get; set; }

    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    public int CommodityId { get; set; }
    [ForeignKey(nameof(CommodityId))]
    public Commodity? Commodity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TargetPrice { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
