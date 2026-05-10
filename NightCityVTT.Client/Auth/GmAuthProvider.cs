using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace NightCityVTT.Client.Auth;

/// <summary>
/// Development stub: returns an authenticated GM user.
/// Replace with a real auth provider (e.g. OIDC, JWT) before production.
/// </summary>
public class GmAuthProvider : AuthenticationStateProvider
{
    private readonly ClaimsPrincipal _gmUser = new(new ClaimsIdentity(
    [
        new Claim(ClaimTypes.Name, "GameMaster"),
        new Claim(ClaimTypes.Role, "GM"),
    ], authenticationType: "NightCityAuth"));

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => Task.FromResult(new AuthenticationState(_gmUser));
}
