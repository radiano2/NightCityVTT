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

    public record TransferItemRequest(string InstanceId, string RecipientCharacterId, int? TakeEurobucks);

    [HttpPost("{id}/transfer-item")]
    public async Task<IActionResult> TransferItem(string id, [FromBody] TransferItemRequest req)
    {
        var donor = await _chars.Find(d => d.Id == id).FirstOrDefaultAsync();
        if (donor is null) return NotFound("Donor not found");

        var recipient = await _chars.Find(d => d.Id == req.RecipientCharacterId).FirstOrDefaultAsync();
        if (recipient is null) return NotFound("Recipient not found");

        // Transfer Eurobucks
        if (req.TakeEurobucks.HasValue && req.TakeEurobucks.Value > 0)
        {
            int amount = Math.Min(req.TakeEurobucks.Value, donor.Eurobucks);
            donor.Eurobucks    -= amount;
            recipient.Eurobucks += amount;
        }

        // Transfer gear item
        if (!string.IsNullOrEmpty(req.InstanceId))
        {
            var item = donor.Inventory.FirstOrDefault(i => i.InstanceId == req.InstanceId);
            if (item is null) return NotFound("Item not found in donor inventory");
            donor.Inventory.Remove(item);
            // Clear from equipped slots if present
            var equippedSlot = donor.EquippedGearIds.FirstOrDefault(kv => kv.Value == req.InstanceId);
            if (equippedSlot.Key is not null) donor.EquippedGearIds.Remove(equippedSlot.Key);
            item.InstanceId = Guid.NewGuid().ToString("N")[..8]; // fresh ID for recipient
            recipient.Inventory.Add(item);
        }

        await _chars.ReplaceOneAsync(d => d.Id == id, donor);
        await _chars.ReplaceOneAsync(d => d.Id == req.RecipientCharacterId, recipient);
        return Ok();
    }
}
