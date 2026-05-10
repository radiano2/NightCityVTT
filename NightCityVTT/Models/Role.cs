using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NightCityVTT.Models;

public class Role
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("specialAbility")]
    public string SpecialAbility { get; set; } = string.Empty;

    [BsonElement("specialAbilityDescription")]
    public string SpecialAbilityDescription { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("primarySkills")]
    public List<string> PrimarySkills { get; set; } = new();

    [BsonElement("careerSkills")]
    public List<string> CareerSkills { get; set; } = new();
}
