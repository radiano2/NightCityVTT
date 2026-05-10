namespace NightCityVTT.Shared.Models;

public class RoleDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SpecialAbility { get; set; } = string.Empty;
    public string SpecialAbilityDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> PrimarySkills { get; set; } = new();
    public List<string> CareerSkills { get; set; } = new();
}
