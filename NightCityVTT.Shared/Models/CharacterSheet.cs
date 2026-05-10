namespace NightCityVTT.Shared.Models;

public class CharacterSheet
{
    public string? Id      { get; set; }
    public string  OwnerId { get; set; } = string.Empty;

    // Identification
    public string Handle { get; set; } = string.Empty;
    public string Name   { get; set; } = string.Empty;

    // Personal Style
    public string Clothes     { get; set; } = string.Empty;
    public string Hairstyle   { get; set; } = string.Empty;
    public string Affectation { get; set; } = string.Empty;

    // Origins
    public string EthnicOrigin { get; set; } = string.Empty;

    // Family
    public string FamilyRanking { get; set; } = string.Empty;
    public string FamilyTragedy { get; set; } = string.Empty;

    // Motivations
    public string PersonalityTrait { get; set; } = string.Empty;
    public string ValueMost        { get; set; } = string.Empty;
    public string FeelAboutPeople  { get; set; } = string.Empty;

    // Edge Runner stats (wizard-created characters)
    public string Role            { get; set; } = string.Empty;
    public string SpecialAbility  { get; set; } = string.Empty;
    public int    INT  { get; set; }
    public int    REF  { get; set; }
    public int    TECH { get; set; }
    public int    COOL { get; set; }
    public int    LK   { get; set; }
    public int    ATT  { get; set; }
    public int    MA   { get; set; }
    public int    EMP  { get; set; }
    public int    BT   { get; set; }
    public bool   IsElite    { get; set; }
    public string SkillsJson { get; set; } = string.Empty; // JSON: {skill:points}
    public int    Humanity   { get; set; }
    public int    Mobility   { get; set; }
    public int    Resilience { get; set; }
    public string CreatedBy  { get; set; } = string.Empty; // "" | "heroldowie"

    public int Eurobucks { get; set; }
    public List<GearItemDto> Inventory { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
