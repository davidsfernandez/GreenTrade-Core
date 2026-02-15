using System.ComponentModel.DataAnnotations;
using GreenTrade.Shared.Enums;

namespace GreenTrade.Shared.Models;

public class Offer
{
    public int Id { get; set; }

    [Required]
    public int CoffeeLotId { get; set; }
    public CoffeeLot? CoffeeLot { get; set; }

    [Required]
    public int BuyerId { get; set; } // The user making the offer
    public User? Buyer { get; set; }

    [Required]
    [Range(0, 1000000)]
    public decimal PricePerBag { get; set; } // The offered price

    [Required]
    public int Quantity { get; set; } // Usually matches lot quantity, but could be partial (future)

    public OfferStatus Status { get; set; } = OfferStatus.Pending;

    [MaxLength(500)]
    public string? Remarks { get; set; } // Message from buyer

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }
}
