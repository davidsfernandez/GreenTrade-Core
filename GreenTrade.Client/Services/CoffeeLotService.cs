using System.Net.Http.Json;
using GreenTrade.Shared.DTOs;

namespace GreenTrade.Client.Services;

public class CoffeeLotService
{
    private readonly HttpClient _http;

    public CoffeeLotService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<CoffeeLotDto>> GetMarketplaceLots()
    {
        return await _http.GetFromJsonAsync<List<CoffeeLotDto>>("api/coffeelots") ?? new List<CoffeeLotDto>();
    }

    public async Task<List<CoffeeLotDto>> GetMyLots()
    {
        return await _http.GetFromJsonAsync<List<CoffeeLotDto>>("api/coffeelots/my") ?? new List<CoffeeLotDto>();
    }

    public async Task CreateLot(CreateCoffeeLotDto lot)
    {
        await _http.PostAsJsonAsync("api/coffeelots", lot);
    }

    public async Task DeleteLot(int id)
    {
        await _http.DeleteAsync($"api/coffeelots/{id}");
    }

    public async Task<List<GreenTrade.Shared.Models.Warehouse>> GetWarehouses()
    {
        return await _http.GetFromJsonAsync<List<GreenTrade.Shared.Models.Warehouse>>("api/references/warehouses") ?? new();
    }

    public async Task<List<GreenTrade.Shared.Models.Certification>> GetCertifications()
    {
        return await _http.GetFromJsonAsync<List<GreenTrade.Shared.Models.Certification>>("api/references/certifications") ?? new();
    }
}
