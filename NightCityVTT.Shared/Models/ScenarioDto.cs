namespace NightCityVTT.Shared.Models;

public class ScenarioDto
{
    public string? Id          { get; set; }
    public string  Name        { get; set; } = "";
    public string  Description { get; set; } = "";
    public string? MapId       { get; set; }
    public string? StoryId     { get; set; }
    public string  Status      { get; set; } = "Active"; // Active | Completed | Archived
    public List<ScenarioTeam> Teams { get; set; } = new();
    public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
}

public class ScenarioTeam
{
    public string Id           { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Name         { get; set; } = "";
    public string Color        { get; set; } = "#3a9fff";
    public bool   IsPlayerSide { get; set; }
    public List<string> MemberCharacterIds { get; set; } = new();
}
