using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using NightCityVTT.Client.Services;

namespace NightCityVTT.Client.Auth;

public class NightCityAuthProvider : AuthenticationStateProvider, IDisposable
{
    private readonly UserSessionService _session;

    public NightCityAuthProvider(UserSessionService session)
    {
        _session = session;
        _session.OnChange += Notify;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_session.CurrentUser is null)
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal()));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _session.CurrentUser.Id),
            new Claim(ClaimTypes.Name,           _session.CurrentUser.Nickname),
            new Claim(ClaimTypes.Role,           _session.CurrentUser.Role.ToString()),
        };
        var identity = new ClaimsIdentity(claims, "NightCityAuth");
        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    private void Notify() =>
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    public void Dispose() => _session.OnChange -= Notify;
}
