using GreenTrade.Server.Data;
using GreenTrade.Server.Hubs;
using GreenTrade.Server.Services.Providers;
using GreenTrade.Shared.DTOs;
using GreenTrade.Shared.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

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
                    var calculator = scope.ServiceProvider.GetRequiredService<IPriceCalculatorService>();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var settings = await context.MarketSettings.FirstOrDefaultAsync(stoppingToken);
                    var quotes = await provider.GetLatestQuotesAsync(_tickersToWatch);

                    // Get specific rates for composite calculation
                    quotes.TryGetValue("KC", out var arabica);
                    quotes.TryGetValue("USDBRL", out var dollar);

                    foreach (var quote in quotes.Values)
                    {
                        // Update History
                        var history = _priceHistory[quote.Ticker];
                        history.Add(quote.CurrentPrice);
                        if (history.Count > 100) history.RemoveAt(0);

                        // Broadcast Price
                        await _hubContext.Clients.Group("GlobalMarket").SendAsync("ReceivePriceUpdate", quote, stoppingToken);

                        // ADVANCED LOGIC: Sales Window (Janela de Venda)
                        if (quote.Ticker == "KC" && arabica != null && dollar != null && settings != null)
                        {
                            var currentBagPrice = calculator.CalculateCoffeeBagPrice(arabica.CurrentPrice, dollar.CurrentPrice, settings.CoffeeBasis);
                            
                            if (history.Count >= Period)
                            {
                                var rsi = TechnicalIndicatorsService.CalculateRSI(history, Period).Last();
                                
                                // Logic: High RSI + Good Price = Sell Window
                                if (rsi >= 70)
                                {
                                    var msg = $"[OPORTUNIDADE] Janela de Venda: Preço atingiu {currentBagPrice:C2} com RSI de {rsi:F1}. Momento ideal para fixação.";
                                    await _hubContext.Clients.Group("GlobalMarket").SendAsync("ReceiveAlert", msg, stoppingToken);
                                }
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
