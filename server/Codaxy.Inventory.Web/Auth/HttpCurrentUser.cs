using System.Security.Claims;
using Codaxy.Inventory.App.Persistence;

namespace Codaxy.Inventory.Web.Auth;

/// <summary>The signed-in user of the request in progress, read each time it is asked.</summary>
public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public string? Email =>
        accessor.HttpContext?.User is { Identity.IsAuthenticated: true } user
            ? user.FindFirstValue(SignIn.EmailClaim)
            : null;
}
