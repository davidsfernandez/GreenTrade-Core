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
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        // Find active alerts for this commodity
        var alerts = await context.PriceAlerts
            .Include(a => a.User)
            .Include(a => a.Commodity)
            .Where(a => a.IsActive && a.Commodity!.TickerSymbol == update.Ticker)
            .ToListAsync();

        var alertsToDeactivate = new List<PriceAlert>();

        foreach (var alert in alerts)
        {
            // Improved Logic: Detect CROSSING, not just equality.
            // But since we don't store "Alert Direction" (Above/Below) in the current simplified model,
            // we assume the user wants to be notified if the price hits the target range.
            // A robust system would store "Condition: GreaterThan / LessThan".
            // For this MVP, we trigger if the current price is "close enough" (0.5% margin) OR passed it?
            // Actually, "passed it" is hard without history state of the alert.
            // Let's stick to "close enough" or exact match logic but improved.
            
            // Let's assume an alert is "Price >= Target" if originally created when Price < Target.
            // But we don't have creation context.
            // Let's use a simple proximity check (within 0.5% range).
            var diff = Math.Abs(update.CurrentPrice - alert.TargetPrice);
            var percentageDiff = diff / alert.TargetPrice;

            if (percentageDiff <= 0.005m) // 0.5% margin
            {
                _logger.LogInformation($"Alert triggered for user {alert.UserId} on {update.Ticker}. Target: {alert.TargetPrice}, Current: {update.CurrentPrice}");

                // 1. SignalR Notification
                var message = $"Alerta 🔔: {alert.Commodity?.Name} ({update.Ticker}) atingiu R$ {update.CurrentPrice:N2} (Alvo: {alert.TargetPrice:N2})";
                await _hubContext.Clients.User(alert.UserId.ToString()).SendAsync("ReceiveAlert", message);

                // 2. Email Notification
                var emailBody = $@"
                    <h3>Alerta de Preço GreenTrade</h3>
                    <p>Olá, {alert.User?.FullName}!</p>
                    <p>O ativo <strong>{alert.Commodity?.Name} ({update.Ticker})</strong> atingiu seu preço alvo.</p>
                    <ul>
                        <li><strong>Preço Atual:</strong> R$ {update.CurrentPrice:N2}</li>
                        <li><strong>Seu Alvo:</strong> R$ {alert.TargetPrice:N2}</li>
                    </ul>
                    <p>Acesse a plataforma para negociar agora.</p>
                    <br/>
                    <a href='http://localhost:5000'>Ir para GreenTrade</a>
                ";
                
                try
                {
                    await emailService.SendEmailAsync(alert.User!.Email, $"Alerta: {alert.Commodity?.Name} atingiu o alvo", emailBody);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Failed to send alert email.");
                }

                // Deactivate alert
                alert.IsActive = false;
                alertsToDeactivate.Add(alert);
            }
        }

        if (alertsToDeactivate.Any())
        {
            await context.SaveChangesAsync();
        }
    }
}
