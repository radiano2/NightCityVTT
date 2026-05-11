using System.Net.Http.Json;
using NightCityVTT.Shared.Models;

namespace NightCityVTT.Client.Services;

public class MapApiService
{
    private readonly HttpClient _http;
    public MapApiService(HttpClient http) => _http = http;

    public async Task<List<MapDto>> GetAllAsync()
        => await _http.GetFromJsonAsync<List<MapDto>>("api/map") ?? new();

    public async Task<MapDto?> GetByIdAsync(string id)
    {
        try { return await _http.GetFromJsonAsync<MapDto>($"api/map/{id}"); }
        catch { return null; }
    }

    public async Task<MapDto?> GetActiveAsync()
    {
        try { return await _http.GetFromJsonAsync<MapDto>("api/map/active"); }
        catch { return null; }
    }

    public async Task<MapDto?> CreateAsync(MapDto dto)
    {
        var res = await _http.PostAsJsonAsync("api/map", dto);
        return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<MapDto>() : null;
    }

    public async Task<MapDto?> UpdateAsync(MapDto dto)
    {
        var res = await _http.PutAsJsonAsync($"api/map/{dto.Id}", dto);
        return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<MapDto>() : null;
    }

    public async Task DeleteAsync(string id)
        => await _http.DeleteAsync($"api/map/{id}");
}
