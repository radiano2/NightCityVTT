using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NightCityVTT.Shared.Models;

namespace NightCityVTT.Models;

public class MapDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string?  Id        { get; set; }
    public string   Name      { get; set; } = "";
    public int      Width     { get; set; } = 20;
    public int      Height    { get; set; } = 20;
    public int      TileSize  { get; set; } = 40;
    public bool     IsActive  { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<TileCell>   Tiles  { get; set; } = new();
    public List<TokenState> Tokens { get; set; } = new();

    public MapDto ToDto() => new()
    {
        Id = Id, Name = Name, Width = Width, Height = Height, TileSize = TileSize,
        IsActive = IsActive, CreatedAt = CreatedAt, Tiles = Tiles, Tokens = Tokens
    };

    public static MapDocument FromDto(MapDto d) => new()
    {
        Id = d.Id, Name = d.Name, Width = d.Width, Height = d.Height, TileSize = d.TileSize,
        IsActive = d.IsActive, CreatedAt = d.CreatedAt, Tiles = d.Tiles, Tokens = d.Tokens
    };
}
