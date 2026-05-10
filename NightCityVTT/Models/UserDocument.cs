using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NightCityVTT.Shared.Models;

namespace NightCityVTT.Models;

public class UserDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string   Nickname     { get; set; } = string.Empty;
    public string   PasswordHash { get; set; } = string.Empty;
    public UserRole Role         { get; set; }
    public DateTime CreatedAt    { get; set; } = DateTime.UtcNow;

    public UserDto ToDto() => new() { Id = Id!, Nickname = Nickname, Role = Role };
}
