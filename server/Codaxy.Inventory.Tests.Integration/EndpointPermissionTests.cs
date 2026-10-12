using Codaxy.Inventory.App.Shared.Access;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Codaxy.Inventory.Tests.Integration;

/// <summary>
/// A forgotten permission means "everyone signed in", which every other test passes. So every
/// <c>/api</c> endpoint names one from the catalogue, but for a short list that any caller may reach.
/// </summary>
public class EndpointPermissionTests(InventoryApplication app) : IClassFixture<InventoryApplication>
{
    private static readonly string[] Open =
    [
        "api/version",
        "api/auth/",
        "api/documents/settings",
        "api/{**path}",
    ];

    [Fact]
    public void Every_api_endpoint_requires_a_permission_from_the_catalogue()
    {
        var endpoints = app
            .Services.GetRequiredService<EndpointDataSource>()
            .Endpoints.OfType<RouteEndpoint>()
            .Where(e => e.RoutePattern.RawText?.TrimStart('/').StartsWith("api/") == true)
            .ToList();

        Assert.NotEmpty(endpoints);

        var missing = endpoints
            .Where(e => !Open.Any(o => e.RoutePattern.RawText!.TrimStart('/').StartsWith(o)))
            .Where(e =>
                PermissionPolicy.Of(e.Metadata) is not { } p || !Permissions.Keys.Contains(p)
            )
            .Select(e => $"{e.DisplayName}")
            .ToList();

        Assert.Empty(missing);
    }
}
