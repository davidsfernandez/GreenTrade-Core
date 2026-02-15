using System.Net.Http.Json;
using GreenTrade.Shared.DTOs;

namespace GreenTrade.Client.Services;

public class OfferService
{
    private readonly HttpClient _http;

    public OfferService(HttpClient http)
    {
        _http = http;
    }

    public async Task CreateOffer(CreateOfferDto offer)
    {
        await _http.PostAsJsonAsync("api/offers", offer);
    }

    public async Task<List<OfferDto>> GetMySentOffers()
    {
        return await _http.GetFromJsonAsync<List<OfferDto>>("api/offers/my-sent") ?? new List<OfferDto>();
    }

    public async Task<List<OfferDto>> GetReceivedOffers()
    {
        return await _http.GetFromJsonAsync<List<OfferDto>>("api/offers/received") ?? new List<OfferDto>();
    }

    public async Task RespondToOffer(int offerId, bool accepted)
    {
        var response = new UpdateOfferStatusDto { IsAccepted = accepted };
        await _http.PostAsJsonAsync($"api/offers/{offerId}/respond", response);
    }
}
