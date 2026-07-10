using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NightCityVTT.Models;
using NightCityVTT.Shared.Models;

namespace NightCityVTT.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CharacterController : ControllerBase
{
    private readonly IMongoCollection<CharacterDocument> _chars;

    public CharacterController(IMongoClient mongo, IConfiguration config)
    {
        var db = mongo.GetDatabase(config["MongoDB:DatabaseName"] ?? "NightCityVTT");
        _chars = db.GetCollection<CharacterDocument>("Characters");
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? ownerId = null)
    {
        var filter = string.IsNullOrEmpty(ownerId)
            ? FilterDefinition<CharacterDocument>.Empty
            : Builders<CharacterDocument>.Filter.Eq(d => d.OwnerId, ownerId);

        var docs = await _chars.Find(filter)
            .SortByDescending(d => d.CreatedAt)
            .ToListAsync();
        return Ok(docs.Select(d => d.ToSheet()));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var doc = await _chars.Find(d => d.Id == id).FirstOrDefaultAsync();
        if (doc == null) return NotFound();
        return Ok(doc.ToSheet());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CharacterSheet sheet)
    {
        if (string.IsNullOrWhiteSpace(sheet.Handle))
            return BadRequest("Handle is required.");
        var doc = CharacterDocument.FromSheet(sheet);
        await _chars.InsertOneAsync(doc);
        return Ok(doc.ToSheet());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] CharacterSheet sheet)
    {
        if (id != sheet.Id) return BadRequest();
        var doc = CharacterDocument.FromSheet(sheet);
        doc.Id = id; // ensure the internal BSON ID matches
        var result = await _chars.ReplaceOneAsync(d => d.Id == id, doc);
        return result.ModifiedCount == 1 ? Ok(doc.ToSheet()) : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _chars.DeleteOneAsync(d => d.Id == id);
        return result.DeletedCount == 1 ? Ok() : NotFound();
    }
}
