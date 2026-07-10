using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NightCityVTT.Shared.Models;

namespace NightCityVTT.Models;

public class ScenarioDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id          { get; set; }
    public string  Name        { get; set; } = "";
    public string  Description { get; set; } = "";
    public string? MapId       { get; set; }
    public string? StoryId     { get; set; }
    public string  Status      { get; set; } = "Active";
    public List<ScenarioTeam> Teams { get; set; } = new();
    public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;

    public ScenarioDto ToDto() => new()
    {
        Id = Id, Name = Name, Description = Description,
        MapId = MapId, StoryId = StoryId, Status = Status,
        Teams = Teams, CreatedAt = CreatedAt
    };

    public static ScenarioDocument FromDto(ScenarioDto d) => new()
    {
        Id = d.Id, Name = d.Name, Description = d.Description,
        MapId = d.MapId, StoryId = d.StoryId, Status = d.Status,
        Teams = d.Teams, CreatedAt = d.CreatedAt == default ? DateTime.UtcNow : d.CreatedAt
    };
}
