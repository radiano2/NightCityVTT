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
    public List<TileCell>   Tiles  { get; set; } = new();
    public List<TokenState> Tokens { get; set; } = new();
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
    public int    MoveRange   { get; set; } = 12; // MA * 2 (walk) filled at add-time
}
