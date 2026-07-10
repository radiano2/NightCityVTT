using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NightCityVTT.Models;
using NightCityVTT.Shared.Models;

namespace NightCityVTT.Controllers;

[ApiController]
[Route("api/scenario")]
public class ScenarioController : ControllerBase
{
    private readonly IMongoCollection<ScenarioDocument> _scenarios;
    private readonly IMongoCollection<MapDocument>      _maps;
    private readonly IMongoCollection<CharacterDocument> _chars;

    public ScenarioController(IMongoClient mongo, IConfiguration config)
    {
        var db = mongo.GetDatabase(config["MongoDB:DatabaseName"] ?? "NightCityVTT");
        _scenarios = db.GetCollection<ScenarioDocument>("Scenarios");
        _maps      = db.GetCollection<MapDocument>("Maps");
        _chars     = db.GetCollection<CharacterDocument>("Characters");
    }

    [HttpGet]
    public async Task<List<ScenarioDto>> GetAll()
        => (await _scenarios.Find(_ => true).SortByDescending(s => s.CreatedAt).ToListAsync())
           .Select(d => d.ToDto()).ToList();

    [HttpGet("{id}")]
    public async Task<ActionResult<ScenarioDto>> GetById(string id)
    {
        var doc = await _scenarios.Find(s => s.Id == id).FirstOrDefaultAsync();
        return doc is null ? NotFound() : doc.ToDto();
    }

    [HttpPost]
    public async Task<ActionResult<ScenarioDto>> Create([FromBody] ScenarioDto dto)
    {
        var doc = ScenarioDocument.FromDto(dto);
        doc.Id = null;
        doc.CreatedAt = DateTime.UtcNow;
        await _scenarios.InsertOneAsync(doc);
        return doc.ToDto();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ScenarioDto>> Update(string id, [FromBody] ScenarioDto dto)
    {
        dto.Id = id;
        var doc = ScenarioDocument.FromDto(dto);
        var result = await _scenarios.ReplaceOneAsync(s => s.Id == id, doc);
        return result.MatchedCount == 0 ? NotFound() : doc.ToDto();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _scenarios.DeleteOneAsync(s => s.Id == id);
        return NoContent();
    }

    // Launch scenario: populate the linked map with tokens from each team's characters
    [HttpPost("{id}/launch")]
    public async Task<ActionResult<string>> Launch(string id)
    {
        var scenario = await _scenarios.Find(s => s.Id == id).FirstOrDefaultAsync();
        if (scenario is null) return NotFound("Scenario not found");
        if (string.IsNullOrEmpty(scenario.MapId)) return BadRequest("Scenario has no linked map");

        var map = await _maps.Find(m => m.Id == scenario.MapId).FirstOrDefaultAsync();
        if (map is null) return NotFound("Linked map not found");

        // Sync teams from scenario → map
        map.Teams = scenario.Teams.Select(st => new TeamDto
        {
            Id = st.Id, Name = st.Name, Color = st.Color, IsPlayerSide = st.IsPlayerSide
        }).ToList();

        // Ensure defaults if no teams defined
        if (!map.Teams.Any())
        {
            map.Teams.Add(new TeamDto { Name = "Players",  Color = "#3a9fff", IsPlayerSide = true  });
            map.Teams.Add(new TeamDto { Name = "Hostiles", Color = "#ff3a3a", IsPlayerSide = false });
        }

        // Remove tokens that belong to characters in the scenario (will re-add)
        var allMemberIds = scenario.Teams.SelectMany(t => t.MemberCharacterIds).ToHashSet();
        map.Tokens.RemoveAll(t => allMemberIds.Contains(t.CharacterId));

        // Add a token for each character in each team
        var tokenColors = new[] { "#ff3a3a","#3a9fff","#3aff6e","#ffb73a","#ff3aff","#3affff","#ff8c3a","#b83aff" };
        int colorIdx = 0;
        int col = 1, row = 1;
        foreach (var team in scenario.Teams)
        {
            foreach (var charId in team.MemberCharacterIds)
            {
                var charDoc = await _chars.Find(c => c.Id == charId).FirstOrDefaultAsync();
                if (charDoc is null) continue;

                // find a free cell (simple left-to-right placement)
                while (map.Tokens.Any(t => t.Col == col && t.Row == row))
                {
                    col++;
                    if (col >= map.Width) { col = 1; row++; }
                }

                map.Tokens.Add(new TokenState
                {
                    CharacterId = charId,
                    Handle      = charDoc.Handle,
                    Col         = col,
                    Row         = row,
                    Color       = tokenColors[colorIdx++ % tokenColors.Length],
                    TeamId      = team.Id,
                    MoveRange   = charDoc.MA * 2
                });
                col++;
            }
        }

        await _maps.ReplaceOneAsync(m => m.Id == map.Id, map);
        scenario.Status = "Active";
        await _scenarios.ReplaceOneAsync(s => s.Id == id, scenario);

        return Ok($"/maps/{scenario.MapId}");
    }
}
