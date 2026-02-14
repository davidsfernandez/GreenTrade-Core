using GreenTrade.Server.Data;
using GreenTrade.Server.Hubs;
using GreenTrade.Shared.DTOs;
using GreenTrade.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GreenTrade.Server.Services;

/// <summary>
/// Service that monitors prices and triggers alerts when thresholds are crossed.
/// </summary>
public interface IPriceAlertService
{
    Task CheckAlertsAsync(PriceUpdateDto update);
}

public class PriceAlertService : IPriceAlertService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<MarketHub> _hubContext;
    private readonly ILogger<PriceAlertService> _logger;

    public PriceAlertService(IServiceProvider serviceProvider, IHubContext<MarketHub> hubContext, ILogger<PriceAlertService> logger)
    {
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task CheckAlertsAsync(PriceUpdateDto update)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Find active alerts for this commodity
        var triggeredAlerts = await context.PriceAlerts
            .Include(a => a.User)
            .Include(a => a.Commodity)
            .Where(a => a.IsActive && a.Commodity!.TickerSymbol == update.Ticker)
            .ToListAsync();

        foreach (var alert in triggeredAlerts)
        {
            // Simple logic: if TargetPrice is reached or crossed
            bool isTriggered = false;
            
            // This is a simplified check. In a real app, we'd check if it's an "Above" or "Below" alert.
            // For now, if price is within 0.1% of target, trigger it.
            if (Math.Abs(update.CurrentPrice - alert.TargetPrice) / alert.TargetPrice < 0.001m)
            {
                isTriggered = true;
            }

            if (isTriggered)
            {
                _logger.LogInformation($"Alert triggered for user {alert.UserId} on {update.Ticker}");

                // Send notification via SignalR to specific user
                // We'll use the user's email as the group name (or we could use their ID)
                await _hubContext.Clients.User(alert.UserId.ToString()).SendAsync("ReceiveNotification", new
                {
                    Type = "PriceAlert",
                    Message = $"Alerta: {alert.Commodity?.Name} atingiu o preço de {update.CurrentPrice}!",
                    Ticker = update.Ticker
                });

                // Deactivate alert after trigger
                alert.IsActive = false;
            }
        }

        if (triggeredAlerts.Any(a => !a.IsActive))
        {
            await context.SaveChangesAsync();
        }
    }
}
