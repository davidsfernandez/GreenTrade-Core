using GreenTrade.Server.Hubs;
using GreenTrade.Server.Services.Providers;
using GreenTrade.Shared.DTOs;
using GreenTrade.Shared.Services;
using Microsoft.AspNetCore.SignalR;

namespace GreenTrade.Server.Services;

/// <summary>
/// Background service that fetches real-time market data via a provider.
/// Also calculates "Sales Windows" (market opportunities).
/// </summary>
public class MarketDataService : BackgroundService
{
    private readonly IHubContext<MarketHub> _hubContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MarketDataService> _logger;

    private readonly List<string> _tickersToWatch = new() { "KC", "C8", "USDBRL", "B3" };
    private readonly Dictionary<string, List<decimal>> _priceHistory = new();
    private const int Period = 14;

    public MarketDataService(
        IHubContext<MarketHub> hubContext, 
        IServiceProvider serviceProvider, 
        ILogger<MarketDataService> logger)
    {
        _hubContext = hubContext;
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        foreach (var ticker in _tickersToWatch)
        {
            _priceHistory[ticker] = new List<decimal>();
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Market Data Service (Yahoo) is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var provider = scope.ServiceProvider.GetRequiredService<IMarketDataProvider>();
                    var alertService = scope.ServiceProvider.GetRequiredService<IPriceAlertService>();

                    var quotes = await provider.GetLatestQuotesAsync(_tickersToWatch);

                    foreach (var quote in quotes.Values)
                    {
                        // Update History
                        var history = _priceHistory[quote.Ticker];
                        history.Add(quote.CurrentPrice);
                        if (history.Count > 100) history.RemoveAt(0);

                        // Broadcast Price
                        await _hubContext.Clients.Group("GlobalMarket").SendAsync("ReceivePriceUpdate", quote, stoppingToken);
                        await _hubContext.Clients.Group(quote.Ticker).SendAsync("ReceivePriceUpdate", quote, stoppingToken);

                        // Check "Janela de Venda" logic
                        if (history.Count >= Period)
                        {
                            var rsiValues = TechnicalIndicatorsService.CalculateRSI(history, Period);
                            var currentRSI = rsiValues.Last();

                            if (currentRSI >= 70)
                            {
                                await _hubContext.Clients.Group("GlobalMarket").SendAsync("ReceiveAlert", $"[OPORTUNIDADE] Janela de Venda para {quote.Ticker}: RSI Alto ({currentRSI.ToString("F2")})", stoppingToken);
                            }
                            else if (currentRSI <= 30 && currentRSI > 0)
                            {
                                await _hubContext.Clients.Group("GlobalMarket").SendAsync("ReceiveAlert", $"[OPORTUNIDADE] Janela de Compra para {quote.Ticker}: RSI Baixo ({currentRSI.ToString("F2")})", stoppingToken);
                            }
                        }

                        // Check User Alerts
                        await alertService.CheckAlertsAsync(quote);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching market data.");
            }

            // Yahoo Rate Limit safe: 15 seconds
            await Task.Delay(15000, stoppingToken);
        }
    }
}
