using System.ComponentModel.DataAnnotations;

namespace GreenTrade.Shared.Models;

public class Warehouse
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? CNPJ { get; set; }

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [MaxLength(2)]
    public string State { get; set; } = string.Empty; // MG, SP, ES...

    [MaxLength(500)]
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;
}
