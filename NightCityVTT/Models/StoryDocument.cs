using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NightCityVTT.Shared.Models;

namespace NightCityVTT.Models;

public class StoryDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public List<StoryStageDto> Stages { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public StoryDto ToDto() => new()
    {
        Id = Id,
        Title = Title,
        AuthorId = AuthorId,
        Stages = Stages,
        CreatedAt = CreatedAt
    };

    public static StoryDocument FromDto(StoryDto dto) => new()
    {
        Id = dto.Id,
        Title = dto.Title,
        AuthorId = dto.AuthorId,
        Stages = dto.Stages,
        CreatedAt = dto.CreatedAt == default ? DateTime.UtcNow : dto.CreatedAt
    };
}
