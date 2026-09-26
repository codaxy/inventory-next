using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Codaxy.Inventory.Web.Auth;

/// <summary>
/// One place builds the principal, so every provider produces the same claims and a screen never has
/// to ask which one a person used.
/// </summary>
public static class SignIn
{
    public const string EmailClaim = ClaimTypes.Email;
    public const string NameClaim = ClaimTypes.Name;

    public static Task SignInAsync(this HttpContext context, string email, string? displayName)
    {
        var identity = new ClaimsIdentity(
            [
                new Claim(EmailClaim, email),
                new Claim(NameClaim, string.IsNullOrWhiteSpace(displayName) ? email : displayName),
            ],
            CookieAuthenticationDefaults.AuthenticationScheme,
            NameClaim,
            roleType: null
        );

        return context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true }
        );
    }
}
