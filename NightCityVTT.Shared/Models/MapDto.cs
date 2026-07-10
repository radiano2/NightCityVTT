namespace NightCityVTT.Shared.Models;

public class MapDto
{
    public string? Id        { get; set; }
    public string  Name      { get; set; } = "";
    public int     Width     { get; set; } = 20;
    public int     Height    { get; set; } = 20;
    public int     TileSize  { get; set; } = 40;
    public bool    IsActive  { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<TileCell>      Tiles      { get; set; } = new();
    public List<TokenState>    Tokens     { get; set; } = new();
    public List<TeamDto>       Teams      { get; set; } = new();
    public List<CombatLogEntry> CombatLog { get; set; } = new();
}

public class TileCell
{
    public int Col  { get; set; }
    public int Row  { get; set; }
    public int Type { get; set; } // 0=empty 1=floor 2=wall 3=door
}

public class TokenState
{
    public string Id          { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string CharacterId { get; set; } = "";
    public string Handle      { get; set; } = "";
    public int    Col         { get; set; }
    public int    Row         { get; set; }
    public string Color       { get; set; } = "#ff3a3a";
    public string TeamId      { get; set; } = "";
    public int    MoveRange        { get; set; } = 12; // MA * 2 (walk) filled at add-time
    public int    TotalDamageTaken { get; set; }
}

public class TeamDto
{
    public string Id           { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Name         { get; set; } = "";
    public string Color        { get; set; } = "#3a9fff";
    public bool   IsPlayerSide { get; set; }
}

public class CombatLogEntry
{
    public DateTime Timestamp       { get; set; } = DateTime.UtcNow;
    public string   AttackerHandle  { get; set; } = "";
    public string   TargetHandle    { get; set; } = "";
    public string   WeaponName      { get; set; } = "";
    public int      Roll            { get; set; }
    public int      TargetDN        { get; set; }
    public bool     IsHit           { get; set; }
    public bool     IsCritical      { get; set; }
    public bool     IsFumble        { get; set; }
    public string   Location        { get; set; } = "";
    public int      RawDamage       { get; set; }
    public int      FinalDamage     { get; set; }
    public string   WoundState      { get; set; } = "";
    public bool?    StunSavePassed  { get; set; }
    public bool?    DeathSavePassed { get; set; }
}
