using System.Text.Json;
using Microsoft.JSInterop;
using NightCityVTT.Shared.Models;

namespace NightCityVTT.Client.Services;

public class UserSessionService
{
    private readonly IJSRuntime _js;
    private UserDto? _currentUser;
    private bool _initialized;

    public UserSessionService(IJSRuntime js) => _js = js;

    public UserDto? CurrentUser => _currentUser;
    public bool IsLoggedIn      => _currentUser != null;

    public event Action? OnChange;

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;
        try
        {
            var json = await _js.InvokeAsync<string?>("localStorage.getItem", "nc-session");
            if (!string.IsNullOrEmpty(json))
            {
                _currentUser = JsonSerializer.Deserialize<UserDto>(json);
                OnChange?.Invoke();
            }
        }
        catch { }
    }

    public async Task SetUserAsync(UserDto user)
    {
        _currentUser = user;
        await _js.InvokeVoidAsync("localStorage.setItem", "nc-session",
            JsonSerializer.Serialize(user));
        OnChange?.Invoke();
    }

    public async Task ClearAsync()
    {
        _currentUser = null;
        await _js.InvokeVoidAsync("localStorage.removeItem", "nc-session");
        OnChange?.Invoke();
    }
}
