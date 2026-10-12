using Codaxy.Inventory.App.Administration.Users;
using Microsoft.AspNetCore.Authorization;

namespace Codaxy.Inventory.Web.Auth;

public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;

/// <summary>
/// Answers a permission's policy from the signed-in user's role, read from the database for the
/// request: never from the cookie, where a revoked permission would last as long as the session.
/// </summary>
public sealed class PermissionHandler(UserAccess access)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement
    )
    {
        if ((await access.CurrentAsync())?.Has(requirement.Permission) == true)
            context.Succeed(requirement);
    }
}
