using Microsoft.AspNetCore.Authorization;

namespace Codaxy.Inventory.App.Shared.Access;

/// <summary>
/// The authorization policy for one permission, named <c>perm:&lt;key&gt;</c>. The host registers one
/// per entry of <see cref="Permissions.All"/>, so a key outside the catalogue has no policy and the
/// request fails rather than passing.
/// </summary>
public static class PermissionPolicy
{
    public const string Prefix = "perm:";

    public static string For(string permission) => Prefix + permission;

    /// <summary>The permission an endpoint requires, or null when it requires none.</summary>
    public static string? Of(EndpointMetadataCollection metadata) =>
        metadata
            .GetOrderedMetadata<IAuthorizeData>()
            .Select(a => a.Policy)
            .LastOrDefault(p => p?.StartsWith(Prefix, StringComparison.Ordinal) == true)
            ?[Prefix.Length..];

    /// <summary>
    /// A section's endpoints, behind its permissions: a <c>GET</c> reads and needs
    /// <paramref name="view"/>, any other method writes and needs <paramref name="edit"/>. Without
    /// an edit permission the section is read-only, and a write mapped into it anyway is refused to
    /// everyone.
    /// </summary>
    public static RouteGroupBuilder RequirePermissions(
        this RouteGroupBuilder group,
        string view,
        string? edit = null
    )
    {
        ((IEndpointConventionBuilder)group).Add(endpoint =>
        {
            var reads = endpoint
                .Metadata.OfType<IHttpMethodMetadata>()
                .SelectMany(m => m.HttpMethods)
                .All(m => m is "GET" or "HEAD");

            var permission = reads
                ? view
                : edit
                    ?? throw new InvalidOperationException(
                        $"{endpoint.DisplayName} writes in a section that has only {view}."
                    );

            endpoint.Metadata.Add(new AuthorizeAttribute(For(permission)));
        });

        return group;
    }
}
