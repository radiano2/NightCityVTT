using System.Net.Http.Json;
using NightCityVTT.Shared.Models;

namespace NightCityVTT.Client.Services;

public class ScenarioApiService
{
    private readonly HttpClient _http;
    public ScenarioApiService(HttpClient http) => _http = http;

    public async Task<List<ScenarioDto>> GetAllAsync()
        => await _http.GetFromJsonAsync<List<ScenarioDto>>("api/scenario") ?? new();

    public async Task<ScenarioDto?> GetByIdAsync(string id)
    {
        try { return await _http.GetFromJsonAsync<ScenarioDto>($"api/scenario/{id}"); }
        catch { return null; }
    }

    public async Task<ScenarioDto?> CreateAsync(ScenarioDto dto)
    {
        var res = await _http.PostAsJsonAsync("api/scenario", dto);
        return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<ScenarioDto>() : null;
    }

    public async Task<ScenarioDto?> UpdateAsync(ScenarioDto dto)
    {
        var res = await _http.PutAsJsonAsync($"api/scenario/{dto.Id}", dto);
        return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<ScenarioDto>() : null;
    }

    public async Task DeleteAsync(string id)
        => await _http.DeleteAsync($"api/scenario/{id}");

    public async Task<string?> LaunchAsync(string id)
    {
        var res = await _http.PostAsync($"api/scenario/{id}/launch", null);
        if (!res.IsSuccessStatusCode) return null;
        var raw = await res.Content.ReadAsStringAsync();
        return raw.Trim('"');
    }

    public async Task<bool> TransferItemAsync(string donorCharId, string instanceId, string recipientCharId, int? takeEurobucks = null)
    {
        var res = await _http.PostAsJsonAsync($"api/character/{donorCharId}/transfer-item",
            new { InstanceId = instanceId, RecipientCharacterId = recipientCharId, TakeEurobucks = takeEurobucks });
        return res.IsSuccessStatusCode;
    }
}
