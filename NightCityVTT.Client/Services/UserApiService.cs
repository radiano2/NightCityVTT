using System.Net.Http.Json;
using NightCityVTT.Shared.Models;

namespace NightCityVTT.Client.Services;

public class UserApiService
{
    private readonly HttpClient _http;

    public UserApiService(HttpClient http) => _http = http;

    public async Task<bool> AnyExistAsync()
        => await _http.GetFromJsonAsync<bool>("api/user/exists");

    public async Task<List<UserDto>> GetAllAsync()
        => await _http.GetFromJsonAsync<List<UserDto>>("api/user") ?? new();

    public async Task<(UserDto? user, string? error)> RegisterAsync(RegisterRequest req)
    {
        var res = await _http.PostAsJsonAsync("api/user/register", req);
        if (res.IsSuccessStatusCode)
            return (await res.Content.ReadFromJsonAsync<UserDto>(), null);
        var msg = await res.Content.ReadAsStringAsync();
        return (null, msg);
    }

    public async Task<(UserDto? user, string? error)> LoginAsync(LoginRequest req)
    {
        var res = await _http.PostAsJsonAsync("api/user/login", req);
        if (res.IsSuccessStatusCode)
            return (await res.Content.ReadFromJsonAsync<UserDto>(), null);
        return (null, "Invalid credentials.");
    }
}
