using Microsoft.AspNetCore.SignalR;
using NightCityVTT.Shared.Models;

namespace NightCityVTT.Hubs;

public class GameHub : Hub
{
    // ── Dice ──────────────────────────────────────────────────────────────

    public async Task BroadcastRoll(RollResult result, string player)
        => await Clients.Others.SendAsync("ReceiveRoll", result, player);

    // ── Map room management ───────────────────────────────────────────────

    public async Task JoinMap(string mapId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"map_{mapId}");

    public async Task LeaveMap(string mapId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"map_{mapId}");

    // ── Map events — broadcast to others in the same map room ────────────

    public async Task MoveToken(string mapId, string tokenId, int col, int row)
        => await Clients.OthersInGroup($"map_{mapId}").SendAsync("TokenMoved", tokenId, col, row);

    public async Task UpdateMapTiles(string mapId, List<TileCell> tiles)
        => await Clients.OthersInGroup($"map_{mapId}").SendAsync("MapTilesUpdated", tiles);

    public async Task AddToken(string mapId, TokenState token)
        => await Clients.OthersInGroup($"map_{mapId}").SendAsync("TokenAdded", token);

    public async Task RemoveToken(string mapId, string tokenId)
        => await Clients.OthersInGroup($"map_{mapId}").SendAsync("TokenRemoved", tokenId);
}
