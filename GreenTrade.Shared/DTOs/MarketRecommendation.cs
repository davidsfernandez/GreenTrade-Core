namespace GreenTrade.Shared.DTOs;

public enum RecommendationType
{
    Neutral,
    Buy,
    StrongBuy,
    Sell,
    StrongSell
}

public class MarketRecommendation
{
    public RecommendationType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal RSI { get; set; }
    public string Color { get; set; } = "secondary";
}
