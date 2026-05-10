namespace NightCityVTT.Models;

public class SeedReport
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int SkillCount { get; set; }
    public int RoleCount { get; set; }
    public int GearCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> ValidationWarnings { get; set; } = new();
    public Dictionary<string, long> CollectionCounts { get; set; } = new();
}
