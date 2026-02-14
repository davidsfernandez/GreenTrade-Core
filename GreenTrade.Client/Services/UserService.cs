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
}
