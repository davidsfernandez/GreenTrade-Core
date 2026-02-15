using GreenTrade.Shared.DTOs;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Components;

namespace GreenTrade.Client.Services;

/// <summary>
/// Service to manage SignalR connection and price updates in the frontend.
/// </summary>
public class MarketDataClientService : IAsyncDisposable
{
    private readonly NavigationManager _navigationManager;
    private HubConnection? _hubConnection;
    
    public event Action<PriceUpdateDto>? OnPriceUpdate;
    public event Action<string>? OnAlertReceived;

    public HubConnectionState State => _hubConnection?.State ?? HubConnectionState.Disconnected;

    public MarketDataClientService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public async Task StartAsync()
    {
        if (_hubConnection is not null && _hubConnection.State == HubConnectionState.Connected)
            return;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_navigationManager.ToAbsoluteUri("/hubs/market"))
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<PriceUpdateDto>("ReceivePriceUpdate", (update) =>
        {
            OnPriceUpdate?.Invoke(update);
        });

        _hubConnection.On<string>("ReceiveAlert", (message) =>
        {
            OnAlertReceived?.Invoke(message);
        });

        await _hubConnection.StartAsync();
    }

    public async Task StopAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.StopAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
