namespace NightCityVTT.Shared.Models;

public class RollResult
{
    public string Label { get; set; } = string.Empty;
    public int Total { get; set; }
    public List<int> Dice { get; set; } = new();
    public bool IsCriticalSuccess { get; set; }
    public bool IsFumble { get; set; }
    public List<string> Log { get; set; } = new();
}
