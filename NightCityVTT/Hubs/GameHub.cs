using Microsoft.AspNetCore.SignalR;
using NightCityVTT.Shared.Models;

namespace NightCityVTT.Hubs;

public class GameHub : Hub
{
    public async Task BroadcastRoll(RollResult result, string player)
        => await Clients.Others.SendAsync("ReceiveRoll", result, player);
}
