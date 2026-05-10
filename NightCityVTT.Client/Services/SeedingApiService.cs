using System.Net.Http.Json;

namespace NightCityVTT.Client.Services;

public class SeedReport
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int SkillCount { get; set; }
    public int RoleCount { get; set; }
    public int GearCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> ValidationWarnings { get; set; } = new();
    public Dictionary<string, long> CollectionCounts { get; set; } = new();
}

public class SeedingApiService
{
    private readonly HttpClient _http;

    public SeedingApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<SeedReport?> RunSeedAsync()
    {
        var response = await _http.PostAsync("api/seed/run", null);
        return await response.Content.ReadFromJsonAsync<SeedReport>();
    }

    public async Task<SeedReport?> ValidateAsync()
    {
        return await _http.GetFromJsonAsync<SeedReport>("api/seed/validate");
    }
}
