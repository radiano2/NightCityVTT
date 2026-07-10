namespace NightCityVTT.Shared.Models;

public class SystemSettingsDto
{
    public string Theme { get; set; } = "";
    public int AnimationIntervalMs { get; set; } = 5000;
    public bool CrtFilterEnabled { get; set; } = true;
}
