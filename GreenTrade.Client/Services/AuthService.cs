using System.Net.Http.Json;
using GreenTrade.Shared.DTOs;
using Microsoft.AspNetCore.Components.Authorization;

namespace GreenTrade.Client.Services;

public interface IAuthService
{
    Task<LoginResponse> Login(LoginRequest request);
    Task<LoginResponse> Register(RegisterRequest request);
    Task Logout();
    Task<bool> ForgotPassword(string email);
    Task<bool> ResetPassword(ResetPasswordRequest request);
}

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthService(HttpClient httpClient, AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _authStateProvider = authStateProvider;
    }

    public async Task<LoginResponse> Login(LoginRequest request)
    {
        var result = await _httpClient.PostAsJsonAsync("api/auth/login", request);
        var response = await result.Content.ReadFromJsonAsync<LoginResponse>();

        if (response != null && response.Success)
        {
            await ((CustomAuthStateProvider)_authStateProvider).MarkUserAsAuthenticated(response.Token);
        }

        return response ?? new LoginResponse { Success = false, Message = "Unknown error" };
    }

    public async Task<LoginResponse> Register(RegisterRequest request)
    {
        var result = await _httpClient.PostAsJsonAsync("api/auth/register", request);
        var response = await result.Content.ReadFromJsonAsync<LoginResponse>();

        if (response != null && response.Success)
        {
            await ((CustomAuthStateProvider)_authStateProvider).MarkUserAsAuthenticated(response.Token);
        }

        return response ?? new LoginResponse { Success = false, Message = "Unknown error" };
    }

    public async Task Logout()
    {
        await ((CustomAuthStateProvider)_authStateProvider).MarkUserAsLoggedOut();
    }

    public async Task<bool> ForgotPassword(string email)
    {
        var result = await _httpClient.PostAsJsonAsync("api/auth/forgot-password", new ForgotPasswordRequest { Email = email });
        return result.IsSuccessStatusCode;
    }

    public async Task<bool> ResetPassword(ResetPasswordRequest request)
    {
        var result = await _httpClient.PostAsJsonAsync("api/auth/reset-password", request);
        return result.IsSuccessStatusCode;
    }
}
