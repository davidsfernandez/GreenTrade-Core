using System.ComponentModel.DataAnnotations;

namespace GreenTrade.Shared.Models;

public class LotCertification
{
    public int CoffeeLotId { get; set; }
    public CoffeeLot? CoffeeLot { get; set; }

    public int CertificationId { get; set; }
    public Certification? Certification { get; set; }

    [MaxLength(50)]
    public string? CertificateNumber { get; set; }

    public DateTime? ValidUntil { get; set; }
}
