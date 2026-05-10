namespace NightCityVTT.Shared.Models;

public class SkillDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LinkedAttribute { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int IpMultiplier { get; set; } = 1;
}
