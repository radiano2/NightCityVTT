namespace NightCityVTT.Shared.Models;

public class LoginRequest
{
    public string Nickname { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
