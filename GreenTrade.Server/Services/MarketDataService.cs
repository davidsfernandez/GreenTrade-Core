using GreenTrade.Server.Hubs;
using GreenTrade.Server.Services.Providers;
using GreenTrade.Shared.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace GreenTrade.Server.Services;

/// <summary>
/// Background service that fetches real-time market data via a provider.
/// </summary>
public class MarketDataService : BackgroundService
{
    private readonly IHubContext<MarketHub> _hubContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MarketDataService> _logger;

    private readonly List<string> _tickersToWatch = new() { "KC", "C8", "USDBRL", "B3" };

    public MarketDataService(
        IHubContext<MarketHub> hubContext, 
        IServiceProvider serviceProvider, 
        ILogger<MarketDataService> logger)
    {
        _hubContext = hubContext;
        _serviceProvider = serviceProvider;
        _logger = logger;
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
                        // Broadcast
                        await _hubContext.Clients.Group("GlobalMarket").SendAsync("ReceivePriceUpdate", quote, stoppingToken);
                        await _hubContext.Clients.Group(quote.Ticker).SendAsync("ReceivePriceUpdate", quote, stoppingToken);

                        // Check Alerts
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
