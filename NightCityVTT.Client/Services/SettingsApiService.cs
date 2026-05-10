using System.Net.Http.Json;
using NightCityVTT.Shared.Models;

namespace NightCityVTT.Client.Services;

public class SettingsApiService
{
    private readonly HttpClient _http;

    public SettingsApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<SystemSettingsDto> GetSettingsAsync()
    {
        try
        {
            var result = await _http.GetFromJsonAsync<SystemSettingsDto>("api/settings");
            return result ?? new SystemSettingsDto();
        }
        catch
        {
            return new SystemSettingsDto();
        }
    }

    public async Task<bool> UpdateSettingsAsync(SystemSettingsDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/settings", dto);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
