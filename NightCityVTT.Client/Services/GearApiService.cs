using System.Net.Http.Json;
using NightCityVTT.Shared.Models;

namespace NightCityVTT.Client.Services;

public class GearApiService
{
    private readonly HttpClient _http;

    public GearApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<GearItemDto>> GetAllAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<GearItemDto>>("api/gear") ?? new();
        }
        catch
        {
            return new();
        }
    }
}
