using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using NightCityVTT.Client.Auth;
using NightCityVTT.Client.Services;
using NightCityVTT.Shared.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Game services
builder.Services.AddScoped<SeedingApiService>();
builder.Services.AddScoped<CharacterApiService>();
builder.Services.AddScoped<SettingsApiService>();
builder.Services.AddScoped<GearApiService>();
builder.Services.AddScoped<StoryApiService>();
builder.Services.AddScoped<MapApiService>();
builder.Services.AddScoped<DiceEngine>();

// Auth
builder.Services.AddSingleton<UserSessionService>();
builder.Services.AddScoped<UserApiService>();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, NightCityAuthProvider>();

// MudBlazor
builder.Services.AddMudServices();

await builder.Build().RunAsync();
