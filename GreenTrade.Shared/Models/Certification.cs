using System.ComponentModel.DataAnnotations;

namespace GreenTrade.Shared.Models;

public class Certification
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty; // e.g. "Rainforest Alliance"

    [MaxLength(100)]
    public string? Organization { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}
