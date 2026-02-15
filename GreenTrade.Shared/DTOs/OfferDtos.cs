using System.ComponentModel.DataAnnotations;

namespace GreenTrade.Shared.DTOs;

public class OfferDto
{
    public int Id { get; set; }
    public int CoffeeLotId { get; set; }
    public string CoffeeLotDescription { get; set; } = string.Empty; // e.g. "100 bags Arabica"
    public int BuyerId { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public decimal PricePerBag { get; set; }
    public int Quantity { get; set; }
    public decimal TotalAmount => PricePerBag * Quantity;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateOfferDto
{
    [Required]
    public int CoffeeLotId { get; set; }

    [Required]
    [Range(0.01, 1000000, ErrorMessage = "Preço inválido")]
    public decimal PricePerBag { get; set; }
    
    [MaxLength(500)]
    public string? Remarks { get; set; }
}

public class UpdateOfferStatusDto
{
    [Required]
    public bool IsAccepted { get; set; } // True = Accept, False = Reject
}
