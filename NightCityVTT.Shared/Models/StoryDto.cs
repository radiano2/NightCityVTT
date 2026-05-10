namespace NightCityVTT.Shared.Models;

public class StoryStageDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Optional Skill Check
    public bool HasSkillCheck { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public int TargetDifficulty { get; set; } = 15; // Standard Average difficulty
}

public class StoryDto
{
    public string? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public List<StoryStageDto> Stages { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
