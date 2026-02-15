using System.Net.Http.Json;
using GreenTrade.Shared.DTOs;
using GreenTrade.Shared.Models;

namespace GreenTrade.Client.Services;

public class PriceAlertService
{
    private readonly HttpClient _http;

    public PriceAlertService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<PriceAlertDto>> GetMyAlerts()
    {
        return await _http.GetFromJsonAsync<List<PriceAlertDto>>("api/pricealerts") ?? new List<PriceAlertDto>();
    }

    public async Task<List<Commodity>> GetCommodities()
    {
        return await _http.GetFromJsonAsync<List<Commodity>>("api/pricealerts/commodities") ?? new List<Commodity>();
    }

    public async Task CreateAlert(CreatePriceAlertDto alert)
    {
        await _http.PostAsJsonAsync("api/pricealerts", alert);
    }

    public async Task DeleteAlert(int id)
    {
        await _http.DeleteAsync($"api/pricealerts/{id}");
    }
}
