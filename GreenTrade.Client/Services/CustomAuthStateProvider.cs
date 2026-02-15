using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace GreenTrade.Client.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _jsRuntime;

    public CustomAuthStateProvider(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
        
        if (string.IsNullOrWhiteSpace(token))
        {
            token = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "authToken");
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        // Token validation and claims extraction
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt")));
    }

    public async Task MarkUserAsAuthenticated(string token, bool rememberMe = false)
    {
        var storage = rememberMe ? "localStorage" : "sessionStorage";
        await _jsRuntime.InvokeVoidAsync($"{storage}.setItem", "authToken", token);
        
        var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"))));
        NotifyAuthenticationStateChanged(authState);
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
        await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "authToken");
        var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        NotifyAuthenticationStateChanged(authState);
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
        
        var claims = new List<Claim>();
        
        if (keyValuePairs != null)
        {
            foreach (var kvp in keyValuePairs)
            {
                var value = kvp.Value.ToString() ?? string.Empty;
                
                // Map standard JWT claims to ClaimTypes
                switch (kvp.Key)
                {
                    case "unique_name":
                    case "name":
                        claims.Add(new Claim(ClaimTypes.Name, value));
                        break;
                    case "email":
                    case "emailaddress":
                        claims.Add(new Claim(ClaimTypes.Email, value));
                        break;
                    case "role":
                        claims.Add(new Claim(ClaimTypes.Role, value));
                        break;
                    case "nameid":
                    case "sub":
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, value));
                        break;
                    default:
                        claims.Add(new Claim(kvp.Key, value));
                        break;
                }
            }
        }
        
        return claims;
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
