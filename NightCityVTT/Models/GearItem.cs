using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NightCityVTT.Models;

public class GearItem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("category")]
    public string Category { get; set; } = string.Empty;

    [BsonElement("forRole")]
    public string ForRole { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("costEurobucks")]
    public int CostEurobucks { get; set; }

    [BsonElement("concealability")]
    public string Concealability { get; set; } = string.Empty;

    [BsonElement("availability")]
    public string Availability { get; set; } = string.Empty;

    [BsonElement("acc")]
    public string Acc { get; set; } = string.Empty;

    [BsonElement("damage")]
    public string Damage { get; set; } = string.Empty;

    [BsonElement("range")]
    public string Range { get; set; } = string.Empty;

    [BsonElement("capacity")]
    public string Capacity { get; set; } = string.Empty;

    [BsonElement("rof")]
    public string ROF { get; set; } = string.Empty;

    [BsonElement("sp")]
    public string SP { get; set; } = string.Empty;

    [BsonElement("ev")]
    public string EV { get; set; } = string.Empty;
}
