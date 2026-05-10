namespace NightCityVTT.Shared.Models;

public enum UserRole { Player, GameMaster }

public class UserDto
{
    public string Id       { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public UserRole Role   { get; set; }
}
