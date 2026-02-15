using GreenTrade.Shared.DTOs;

namespace GreenTrade.Server.Services.Providers;

public interface IMarketDataProvider
{
    Task<Dictionary<string, PriceUpdateDto>> GetLatestQuotesAsync(IEnumerable<string> symbols);
    Task<IEnumerable<ChartDataDto>> GetHistoryAsync(string symbol, string interval, string range);
}

public class ChartDataDto
{
    public long Time { get; set; }
    public decimal Value { get; set; }
}
