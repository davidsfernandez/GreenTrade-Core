using System.Text.Json;
using GreenTrade.Shared.DTOs;

namespace GreenTrade.Server.Services.Providers;

public class YahooFinanceProvider : IMarketDataProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<YahooFinanceProvider> _logger;

    // Mapping from Internal Ticker -> Yahoo Ticker
    // Note: B3 real-time data is often restricted. We try 'ICF=F' (Coffee Futures). 
    // If not available, we might need to calculate it or use a proxy.
    private readonly Dictionary<string, string> _symbolMap = new()
    {
        { "KC", "KC=F" },      // Coffee Arabica (NY)
        { "C8", "RM=F" },      // Robusta Coffee (Liffe) - Proxy for Conilon
        { "USDBRL", "BRL=X" }, // USD to BRL
        { "B3", "ICF=F" }      // B3 Arabica Coffee (proxy)
    };

    public YahooFinanceProvider(HttpClient httpClient, ILogger<YahooFinanceProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Dictionary<string, PriceUpdateDto>> GetLatestQuotesAsync(IEnumerable<string> symbols)
    {
        var results = new Dictionary<string, PriceUpdateDto>();
        var yahooTickers = new List<string>();

        foreach (var sym in symbols)
        {
            if (_symbolMap.ContainsKey(sym))
            {
                yahooTickers.Add(_symbolMap[sym]);
            }
        }

        if (!yahooTickers.Any()) return results;

        var url = $"https://query1.finance.yahoo.com/v7/finance/quote?symbols={string.Join(",", yahooTickers)}";

        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            
            if (doc.RootElement.TryGetProperty("quoteResponse", out var quoteResponse) && 
                quoteResponse.TryGetProperty("result", out var resultArr))
            {
                foreach (var item in resultArr.EnumerateArray())
                {
                    var symbol = item.GetProperty("symbol").GetString();
                    
                    decimal price = 0;
                    if (item.TryGetProperty("regularMarketPrice", out var priceProp))
                        price = priceProp.GetDecimal();
                    
                    decimal changePercent = 0;
                    if (item.TryGetProperty("regularMarketChangePercent", out var changeProp))
                        changePercent = changeProp.GetDecimal();

                    // Reverse Map
                    var internalSymbol = _symbolMap.FirstOrDefault(x => x.Value == symbol).Key;
                    
                    if (internalSymbol != null && price > 0)
                    {
                        results[internalSymbol] = new PriceUpdateDto
                        {
                            Ticker = internalSymbol,
                            CurrentPrice = price,
                            ChangePercentage = Math.Round(changePercent, 2),
                            Timestamp = DateTime.UtcNow
                        };
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch data from Yahoo Finance.");
        }

        return results;
    }
}
