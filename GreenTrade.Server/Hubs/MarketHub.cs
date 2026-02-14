using Microsoft.AspNetCore.SignalR;

namespace GreenTrade.Server.Hubs;

/// <summary>
/// SignalR Hub for real-time market data updates.
/// </summary>
public class MarketHub : Hub
{
    private const string GlobalMarketGroup = "GlobalMarket";

    public override async Task OnConnectedAsync()
    {
        // By default, add all connected clients to the global market group
        await Groups.AddToGroupAsync(Context.ConnectionId, GlobalMarketGroup);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Allows clients to subscribe to updates for a specific commodity.
    /// </summary>
    public async Task SubscribeToCommodity(string ticker)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, ticker);
    }

    /// <summary>
    /// Allows clients to unsubscribe from updates for a specific commodity.
    /// </summary>
    public async Task UnsubscribeFromCommodity(string ticker)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ticker);
    }
}
