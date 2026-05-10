namespace NightCityVTT.Shared.Models;

public class RegisterRequest
{
    public string   Nickname { get; set; } = string.Empty;
    public string   Password { get; set; } = string.Empty;
    public UserRole Role     { get; set; }
}
