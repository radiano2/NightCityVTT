using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NightCityVTT.Models;
using NightCityVTT.Shared.Models;

namespace NightCityVTT.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly IMongoCollection<SystemSettingsDocument> _settingsCollection;

    public SettingsController(IMongoClient mongoClient)
    {
        var db = mongoClient.GetDatabase("NightCityVTT");
        _settingsCollection = db.GetCollection<SystemSettingsDocument>("Settings");
    }

    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var settings = await _settingsCollection.Find(_ => true).FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new SystemSettingsDocument();
            await _settingsCollection.InsertOneAsync(settings);
        }
        
        return Ok(new SystemSettingsDto 
        { 
            Theme = settings.Theme, 
            AnimationIntervalMs = settings.AnimationIntervalMs,
            CrtFilterEnabled = settings.CrtFilterEnabled
        });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateSettings([FromBody] SystemSettingsDto dto)
    {
        var settings = await _settingsCollection.Find(_ => true).FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new SystemSettingsDocument 
            { 
                Theme = dto.Theme, 
                AnimationIntervalMs = dto.AnimationIntervalMs,
                CrtFilterEnabled = dto.CrtFilterEnabled
            };
            await _settingsCollection.InsertOneAsync(settings);
        }
        else
        {
            var update = Builders<SystemSettingsDocument>.Update
                .Set(s => s.Theme, dto.Theme)
                .Set(s => s.AnimationIntervalMs, dto.AnimationIntervalMs)
                .Set(s => s.CrtFilterEnabled, dto.CrtFilterEnabled);
            await _settingsCollection.UpdateOneAsync(s => s.Id == settings.Id, update);
        }
        
        return Ok();
    }
}
