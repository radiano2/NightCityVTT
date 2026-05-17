using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NightCityVTT.Models;
using NightCityVTT.Shared.Models;

namespace NightCityVTT.Controllers;

[ApiController]
[Route("api/map")]
public class MapController : ControllerBase
{
    private readonly IMongoCollection<MapDocument> _maps;

    public MapController(IMongoClient mongo)
        => _maps = mongo.GetDatabase("NightCityVTT").GetCollection<MapDocument>("Maps");

    [HttpGet]
    public async Task<List<MapDto>> GetAll()
        => (await _maps.Find(_ => true).ToListAsync()).Select(d => d.ToDto()).ToList();

    [HttpGet("{id}")]
    public async Task<ActionResult<MapDto>> GetById(string id)
    {
        var doc = await _maps.Find(m => m.Id == id).FirstOrDefaultAsync();
        return doc is null ? NotFound() : doc.ToDto();
    }

    [HttpGet("active")]
    public async Task<ActionResult<MapDto>> GetActive()
    {
        var doc = await _maps.Find(m => m.IsActive).FirstOrDefaultAsync();
        return doc is null ? NotFound() : doc.ToDto();
    }

    [HttpPost]
    public async Task<ActionResult<MapDto>> Create([FromBody] MapDto dto)
    {
        var doc = MapDocument.FromDto(dto);
        doc.Id = null;
        doc.CreatedAt = DateTime.UtcNow;
        await _maps.InsertOneAsync(doc);
        return doc.ToDto();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MapDto>> Update(string id, [FromBody] MapDto dto)
    {
        dto.Id = id;
        var doc = MapDocument.FromDto(dto);
        var result = await _maps.ReplaceOneAsync(m => m.Id == id, doc);
        return result.MatchedCount == 0 ? NotFound() : doc.ToDto();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _maps.DeleteOneAsync(m => m.Id == id);
        return NoContent();
    }
}
