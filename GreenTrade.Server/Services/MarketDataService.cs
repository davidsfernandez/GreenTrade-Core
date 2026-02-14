using GreenTrade.Server.Hubs;
using GreenTrade.Shared.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace GreenTrade.Server.Services;

/// <summary>
/// Background service that simulates real-time market price fluctuations.
/// Uses a Random Walk algorithm to generate realistic movements.
/// </summary>
public class MarketDataService : BackgroundService
{
    private readonly IHubContext<MarketHub> _hubContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MarketDataService> _logger;
    private readonly Random _random = new();

    // Initial dummy prices for simulation
    private readonly Dictionary<string, decimal> _prices = new()
    {
        { "KC", 185.50m },    // Coffee Arabica (Centavos/lb)
        { "C8", 145.20m },    // Coffee Conilon (Centavos/lb)
        { "USDBRL", 5.25m }   // US Dollar to BRL
    };

    public MarketDataService(IHubContext<MarketHub> hubContext, IServiceProvider serviceProvider, ILogger<MarketDataService> logger)
    {
        _hubContext = hubContext;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Market Data Simulation Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var ticker in _prices.Keys.ToList())
            {
                // Simulate Random Walk: variation between -0.5% and +0.5%
                decimal currentPrice = _prices[ticker];
                decimal variation = (decimal)(_random.NextDouble() * 0.01 - 0.005);
                decimal newPrice = currentPrice * (1 + variation);
                
                // Keep prices positive and within reasonable bounds
                if (newPrice < 0.1m) newPrice = 0.1m;
                
                _prices[ticker] = newPrice;

                var update = new PriceUpdateDto
                {
                    Ticker = ticker,
                    CurrentPrice = Math.Round(newPrice, 4),
                    ChangePercentage = Math.Round(variation * 100, 2),
                    Timestamp = DateTime.UtcNow
                };

                // Broadcast to SignalR Hub
                await _hubContext.Clients.Group("GlobalMarket").SendAsync("ReceivePriceUpdate", update, stoppingToken);
                await _hubContext.Clients.Group(ticker).SendAsync("ReceivePriceUpdate", update, stoppingToken);

                // Trigger Alert Check
                using (var scope = _serviceProvider.CreateScope())
                {
                    var alertService = scope.ServiceProvider.GetRequiredService<IPriceAlertService>();
                    await alertService.CheckAlertsAsync(update);
                }
            }

            // Wait for 3 seconds before next "tick"
            await Task.Delay(3000, stoppingToken);
        }

        _logger.LogInformation("Market Data Simulation Service is stopping.");
    }
}
