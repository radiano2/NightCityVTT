using System.Net.Http.Json;
using NightCityVTT.Shared.Models;

namespace NightCityVTT.Client.Services;

public class StoryApiService
{
    private readonly HttpClient _http;

    public StoryApiService(HttpClient http) => _http = http;

    public async Task<List<StoryDto>> GetAllAsync()
        => await _http.GetFromJsonAsync<List<StoryDto>>("api/story") ?? new();

    public async Task<StoryDto?> CreateAsync(StoryDto dto)
    {
        var res = await _http.PostAsJsonAsync("api/story", dto);
        return res.IsSuccessStatusCode
            ? await res.Content.ReadFromJsonAsync<StoryDto>()
            : null;
    }

    public async Task<StoryDto?> UpdateAsync(StoryDto dto)
    {
        var res = await _http.PutAsJsonAsync($"api/story/{dto.Id}", dto);
        return res.IsSuccessStatusCode
            ? await res.Content.ReadFromJsonAsync<StoryDto>()
            : null;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var res = await _http.DeleteAsync($"api/story/{id}");
        return res.IsSuccessStatusCode;
    }
}
