using GreenTrade.Shared.DTOs;

namespace GreenTrade.Server.Services.Providers;

public interface IMarketDataProvider
{
    Task<Dictionary<string, PriceUpdateDto>> GetLatestQuotesAsync(IEnumerable<string> symbols);
    Task<IEnumerable<ChartDataDto>> GetHistoryAsync(string symbol, string interval, string range);
}
