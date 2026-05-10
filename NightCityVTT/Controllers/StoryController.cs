using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NightCityVTT.Models;
using NightCityVTT.Shared.Models;

namespace NightCityVTT.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoryController : ControllerBase
{
    private readonly IMongoCollection<StoryDocument> _stories;

    public StoryController(IMongoClient mongo, IConfiguration config)
    {
        var db = mongo.GetDatabase(config["MongoDB:DatabaseName"] ?? "NightCityVTT");
        _stories = db.GetCollection<StoryDocument>("Stories");
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var docs = await _stories.Find(_ => true).ToListAsync();
        return Ok(docs.Select(d => d.ToDto()));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title)) return BadRequest("Title is required.");
        
        var doc = StoryDocument.FromDto(dto);
        await _stories.InsertOneAsync(doc);
        return Ok(doc.ToDto());
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] StoryDto dto)
    {
        if (id != dto.Id) return BadRequest();
        var doc = StoryDocument.FromDto(dto);
        doc.Id = id;
        
        var result = await _stories.ReplaceOneAsync(d => d.Id == id, doc);
        return result.ModifiedCount == 1 ? Ok(doc.ToDto()) : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _stories.DeleteOneAsync(d => d.Id == id);
        return result.DeletedCount == 1 ? Ok() : NotFound();
    }
}
