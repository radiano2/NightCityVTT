using System.Net.Http.Json;
using NightCityVTT.Shared.Models;

namespace NightCityVTT.Client.Services;

public class CharacterApiService
{
    private readonly HttpClient _http;

    public CharacterApiService(HttpClient http) => _http = http;

    public async Task<List<CharacterSheet>> GetAllAsync(string? ownerId = null)
    {
        var url = string.IsNullOrEmpty(ownerId)
            ? "api/character"
            : $"api/character?ownerId={Uri.EscapeDataString(ownerId)}";
        return await _http.GetFromJsonAsync<List<CharacterSheet>>(url) ?? new();
    }

    public async Task<CharacterSheet?> CreateAsync(CharacterSheet sheet)
    {
        var res = await _http.PostAsJsonAsync("api/character", sheet);
        return res.IsSuccessStatusCode
            ? await res.Content.ReadFromJsonAsync<CharacterSheet>()
            : null;
    }

    public async Task<CharacterSheet?> UpdateAsync(CharacterSheet sheet)
    {
        var res = await _http.PutAsJsonAsync($"api/character/{sheet.Id}", sheet);
        return res.IsSuccessStatusCode
            ? await res.Content.ReadFromJsonAsync<CharacterSheet>()
            : null;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var res = await _http.DeleteAsync($"api/character/{id}");
        return res.IsSuccessStatusCode;
    }
}
