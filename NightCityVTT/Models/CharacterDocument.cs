using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NightCityVTT.Shared.Models;

namespace NightCityVTT.Models;

public class CharacterDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id      { get; set; }
    public string  OwnerId { get; set; } = string.Empty;

    public string Handle       { get; set; } = string.Empty;
    public string Name         { get; set; } = string.Empty;
    public string Clothes      { get; set; } = string.Empty;
    public string Hairstyle    { get; set; } = string.Empty;
    public string Affectation  { get; set; } = string.Empty;
    public string EthnicOrigin { get; set; } = string.Empty;
    public string FamilyRanking{ get; set; } = string.Empty;
    public string FamilyTragedy{ get; set; } = string.Empty;
    public string PersonalityTrait { get; set; } = string.Empty;
    public string ValueMost        { get; set; } = string.Empty;
    public string FeelAboutPeople  { get; set; } = string.Empty;

    // Edge Runner stat fields
    public string Role           { get; set; } = string.Empty;
    public string SpecialAbility { get; set; } = string.Empty;
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
    public bool   IsTestAsset { get; set; }
    public string SkillsJson { get; set; } = string.Empty;
    public int    Humanity   { get; set; }
    public int    Mobility   { get; set; }
    public int    Resilience { get; set; }
    public string CreatedBy  { get; set; } = string.Empty;
    public int Eurobucks { get; set; }
    public List<GearItemDto> Inventory { get; set; } = new();
    public Dictionary<string, string> EquippedGearIds { get; set; } = new();
    public Dictionary<string, int> GridItemPositions { get; set; } = new();
    public List<string> InstalledCyberwareIds { get; set; } = new();
    public Dictionary<string, int> HumanityPaidByInstanceId { get; set; } = new();

    public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;

    public CharacterSheet ToSheet() => new()
    {
        Id = Id, Handle = Handle, Name = Name,
        Clothes = Clothes, Hairstyle = Hairstyle, Affectation = Affectation,
        EthnicOrigin = EthnicOrigin,
        FamilyRanking = FamilyRanking, FamilyTragedy = FamilyTragedy,
        PersonalityTrait = PersonalityTrait, ValueMost = ValueMost,
        FeelAboutPeople = FeelAboutPeople,
        Role = Role, SpecialAbility = SpecialAbility,
        INT = INT, REF = REF, TECH = TECH, COOL = COOL, LK = LK,
        ATT = ATT, MA = MA, EMP = EMP, BT = BT,
        IsElite = IsElite, IsTestAsset = IsTestAsset, SkillsJson = SkillsJson,
        Humanity = Humanity, Mobility = Mobility, Resilience = Resilience,
        CreatedBy = CreatedBy, Eurobucks = Eurobucks, Inventory = Inventory,
        EquippedGearIds = EquippedGearIds, GridItemPositions = GridItemPositions,
        InstalledCyberwareIds = InstalledCyberwareIds,
        HumanityPaidByInstanceId = HumanityPaidByInstanceId,
        CreatedAt = CreatedAt
    };

    public static CharacterDocument FromSheet(CharacterSheet s) => new()
    {
        Id = s.Id,
        Handle = s.Handle, Name = s.Name,
        Clothes = s.Clothes, Hairstyle = s.Hairstyle, Affectation = s.Affectation,
        EthnicOrigin = s.EthnicOrigin,
        FamilyRanking = s.FamilyRanking, FamilyTragedy = s.FamilyTragedy,
        PersonalityTrait = s.PersonalityTrait, ValueMost = s.ValueMost,
        FeelAboutPeople = s.FeelAboutPeople,
        Role = s.Role, SpecialAbility = s.SpecialAbility,
        INT = s.INT, REF = s.REF, TECH = s.TECH, COOL = s.COOL, LK = s.LK,
        ATT = s.ATT, MA = s.MA, EMP = s.EMP, BT = s.BT,
        IsElite = s.IsElite, IsTestAsset = s.IsTestAsset, SkillsJson = s.SkillsJson,
        Humanity = s.Humanity, Mobility = s.Mobility, Resilience = s.Resilience,
        CreatedBy = s.CreatedBy, Eurobucks = s.Eurobucks, Inventory = s.Inventory,
        EquippedGearIds = s.EquippedGearIds, GridItemPositions = s.GridItemPositions,
        InstalledCyberwareIds = s.InstalledCyberwareIds,
        HumanityPaidByInstanceId = s.HumanityPaidByInstanceId,
        CreatedAt = s.CreatedAt == default ? DateTime.UtcNow : s.CreatedAt
    };
}
