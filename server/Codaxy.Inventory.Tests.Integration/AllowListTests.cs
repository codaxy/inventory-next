using System.Net;
using System.Net.Http.Json;
using Codaxy.Inventory.App.Administration.Roles;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Me = Codaxy.Inventory.Web.Auth.Session.Me.Endpoint.Response;

namespace Codaxy.Inventory.Tests.Integration;

public class AllowListApplication : InventoryApplication
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting("Auth:AllowedUsers:0", "listed@codaxy.com");
        builder.UseSetting("Auth:AllowedUsers:1", "denied@codaxy.com");
        builder.UseSetting("Auth:AllowedUsers:2", "added@codaxy.com");
        builder.UseSetting("Auth:DeniedUsers:0", "denied@codaxy.com");
    }
}

/// <summary>
/// <c>Auth:AllowedUsers</c> until it goes: an address on it that is not a user signs in as Admin, and
/// one that is a user follows its row.
/// </summary>
public class AllowListTests(AllowListApplication app) : IClassFixture<AllowListApplication>
{
    [Fact]
    public async Task A_listed_address_that_is_not_a_user_signs_in_as_admin()
    {
        var client = await app.SignInAsync("listed@codaxy.com");

        var me = (await client.GetFromJsonAsync<Me>("/api/auth/me"))!;
        Assert.Equal(BuiltInRoles.Admin, me.Role);
        Assert.Equal("listed@codaxy.com", me.Name);
    }

    [Fact]
    public async Task A_denied_address_is_refused_although_listed()
    {
        var response = await app.VerifyAsync(app.Client(), "denied@codaxy.com");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task A_listed_address_once_added_follows_its_user()
    {
        await app.UserAsync("added@codaxy.com", BuiltInRoles.Supervisor);
        var client = await app.SignInAsync("added@codaxy.com");

        var me = (await client.GetFromJsonAsync<Me>("/api/auth/me"))!;
        Assert.Equal(BuiltInRoles.Supervisor, me.Role);
    }

    [Fact]
    public async Task The_users_screen_lists_the_listed_addresses_not_added_yet()
    {
        await app.UserAsync("added@codaxy.com", BuiltInRoles.Supervisor);
        var client = await app.ClientAsync("manager@codaxy.com", BuiltInRoles.Admin);

        var configured = await client.GetFromJsonAsync<string[]>(
            "/api/administration/users/configured"
        );

        Assert.Equal(["listed@codaxy.com"], configured);
    }
}
