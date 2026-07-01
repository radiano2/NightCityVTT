using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using NightCityVTT.Client.Services;
using NightCityVTT.Shared.Services;
using MongoDB.Driver;
using NightCityVTT.Client.Pages;
using NightCityVTT.Components;
using NightCityVTT.Hubs;
using NightCityVTT.Services;

var builder = WebApplication.CreateBuilder(args);

// MongoDB
var mongoConnectionString = builder.Configuration["MongoDB:ConnectionString"] ?? "mongodb://localhost:27017";
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));
builder.Services.AddScoped<DatabaseSeeder>();

// Controllers (for seed API)
builder.Services.AddControllers();

// SignalR
builder.Services.AddSignalR();

// Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

// Shared/Client services registered on server for pre-rendering
builder.Services.AddScoped(sp => 
{
    // When pre-rendering on the server, we need to know where the app is hosted
    // to point the HttpClient to itself for API calls.
    var navManager = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navManager.BaseUri) };
});
builder.Services.AddScoped<SeedingApiService>();
builder.Services.AddScoped<CharacterApiService>();
builder.Services.AddScoped<SettingsApiService>();
builder.Services.AddScoped<GearApiService>();
builder.Services.AddScoped<StoryApiService>();
builder.Services.AddScoped<MapApiService>();
builder.Services.AddScoped<ScenarioApiService>();
builder.Services.AddScoped<DiceEngine>();
builder.Services.AddMudServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();
app.MapHub<GameHub>("/hubs/game");

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(NightCityVTT.Client._Imports).Assembly);

app.Run();
