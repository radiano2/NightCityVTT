using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NightCityVTT.Models;

public class SystemSettingsDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    
    public string Theme { get; set; } = "";
    public int AnimationIntervalMs { get; set; } = 5000;
    public bool CrtFilterEnabled { get; set; } = false;
}
