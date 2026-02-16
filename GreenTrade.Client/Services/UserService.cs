using System.Net.Http.Json;
using GreenTrade.Shared.DTOs;

namespace GreenTrade.Client.Services;

/// <summary>
/// Service to handle user-related operations in the frontend.
/// </summary>
public interface IUserService
{
    Task<IEnumerable<UserDto>> GetUsers();
    Task<UserDto?> GetUserById(int id);
    Task<UserDto?> GetProfile();
    Task<bool> UpdateProfile(UpdateProfileRequest request);
    Task<ApiResponse<string>> ChangePassword(ChangePasswordRequest request);
}

public class UserService : IUserService
{
    private readonly HttpClient _httpClient;

    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<UserDto>> GetUsers()
    {
        try
        {
            var users = await _httpClient.GetFromJsonAsync<IEnumerable<UserDto>>("api/users");
            return users ?? Enumerable.Empty<UserDto>();
        }
        catch (Exception)
        {
            return Enumerable.Empty<UserDto>();
        }
    }

    public async Task<UserDto?> GetUserById(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<UserDto>($"api/users/{id}");
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<UserDto?> GetProfile()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<UserDto>("api/users/me");
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> UpdateProfile(UpdateProfileRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync("api/users/profile", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<ApiResponse<string>> ChangePassword(ChangePasswordRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/users/change-password", request);
        
        if (response.IsSuccessStatusCode)
        {
            return new ApiResponse<string> { Success = true, Message = "Senha alterada com sucesso" };
        }

        var error = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
        return error ?? new ApiResponse<string> { Success = false, Message = "Erro ao alterar senha" };
    }
}
