namespace GreenTrade.Shared.DTOs;

/// <summary>
/// Data Transfer Object for real-time price updates.
/// </summary>
public class PriceUpdateDto
{
    public string Ticker { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public decimal ChangePercentage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ChartDataDto
{
    public long Time { get; set; }
    public decimal Value { get; set; }
}
