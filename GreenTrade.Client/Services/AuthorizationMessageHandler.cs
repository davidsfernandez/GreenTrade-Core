using System.Net.Http.Headers;
using Microsoft.JSInterop;

namespace GreenTrade.Client.Services;

public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly IJSRuntime _jsRuntime;

    public AuthorizationMessageHandler(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
        
        if (string.IsNullOrWhiteSpace(token))
        {
            token = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "authToken");
        }

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            // Console.WriteLine($"[AuthHandler] Token found and attached to {request.RequestUri}");
        }
        else
        {
            // Console.WriteLine($"[AuthHandler] No token found for {request.RequestUri}");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
