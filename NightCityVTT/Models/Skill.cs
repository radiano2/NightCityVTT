using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NightCityVTT.Models;

public class Skill
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("linkedAttribute")]
    public string LinkedAttribute { get; set; } = string.Empty;

    [BsonElement("category")]
    public string Category { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("ipMultiplier")]
    public int IpMultiplier { get; set; } = 1;
}
