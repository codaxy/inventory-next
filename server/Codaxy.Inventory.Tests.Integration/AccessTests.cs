using System.Net;
using System.Net.Http.Json;
using Codaxy.Inventory.App.Administration.Roles;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Me = Codaxy.Inventory.Web.Auth.Session.Me.Endpoint.Response;

namespace Codaxy.Inventory.Tests.Integration;

/// <summary>Who gets in, and what each built-in role may do once in.</summary>
public class AccessTests(InventoryApplication app) : IClassFixture<InventoryApplication>
{
    /// <summary>A list each section reads and a create it writes, with a body no create accepts.</summary>
    public static TheoryData<string> Sections =>
        new()
        {
            "/api/company/clients",
            "/api/electronic-devices/tags",
            "/api/licenses/software-services",
            "/api/furniture/types",
            "/api/informations/tags",
            "/api/infrastructure/virtual-machines",
        };

    [Theory]
    [MemberData(nameof(Sections))]
    public async Task A_supervisor_reads_every_section_and_writes_none(string url)
    {
        var client = await app.ClientAsync("supervisor@codaxy.com", BuiltInRoles.Supervisor);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"{url}/")).StatusCode);
        Assert.Equal(
            HttpStatusCode.Forbidden,
            (await client.PostAsJsonAsync($"{url}/", new { })).StatusCode
        );
    }

    [Theory]
    [MemberData(nameof(Sections))]
    public async Task An_admin_reads_and_writes_every_section(string url)
    {
        var client = await app.ClientAsync("admin@codaxy.com", BuiltInRoles.Admin);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"{url}/")).StatusCode);

        // Past the permission: refused only for the empty body.
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (await client.PostAsJsonAsync($"{url}/", new { })).StatusCode
        );
    }

    [Theory]
    [InlineData(BuiltInRoles.Supervisor, "/api/dashboard", HttpStatusCode.OK)]
    [InlineData(
        BuiltInRoles.Supervisor,
        "/api/administration/audit-log/",
        HttpStatusCode.Forbidden
    )]
    [InlineData(BuiltInRoles.Admin, "/api/administration/audit-log/", HttpStatusCode.OK)]
    [InlineData(BuiltInRoles.Supervisor, "/api/administration/users/", HttpStatusCode.Forbidden)]
    [InlineData(
        BuiltInRoles.Supervisor,
        "/api/administration/server-log/days",
        HttpStatusCode.Forbidden
    )]
    [InlineData(BuiltInRoles.Admin, "/api/administration/users/", HttpStatusCode.OK)]
    [InlineData(
        BuiltInRoles.Admin,
        "/api/administration/server-log/days",
        HttpStatusCode.Forbidden
    )]
    [InlineData(BuiltInRoles.SystemAdmin, "/api/administration/server-log/days", HttpStatusCode.OK)]
    public async Task The_rest_follows_the_role(string role, string url, HttpStatusCode expected)
    {
        var client = await app.ClientAsync($"{role.Replace(" ", "")}-rest@codaxy.com", role);

        Assert.Equal(expected, (await client.GetAsync(url)).StatusCode);
    }

    [Fact]
    public async Task Someone_who_is_not_a_user_proves_the_address_and_is_told_to_ask()
    {
        var client = app.Client();
        const string email = "stranger@codaxy.com";

        // Requesting a code answers alike for anyone in the domain, so it names nobody.
        var requested = await client.PostAsJsonAsync(
            "/api/auth/one-time-code/request",
            new { email }
        );
        Assert.Equal(HttpStatusCode.NoContent, requested.StatusCode);

        var verified = await client.PostAsJsonAsync(
            "/api/auth/one-time-code/verify",
            new { email, code = app.Emails.LastCodeFor(email) }
        );

        Assert.Equal(HttpStatusCode.Forbidden, verified.StatusCode);
        Assert.Contains("administrator", await verified.Content.ReadAsStringAsync());
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await client.GetAsync("/api/auth/me")).StatusCode
        );
    }

    [Fact]
    public async Task Me_names_the_user_as_added_with_the_role_and_its_permissions()
    {
        var client = await app.ClientAsync("named@codaxy.com", BuiltInRoles.Supervisor);

        var me = (await client.GetFromJsonAsync<Me>("/api/auth/me"))!;

        Assert.Equal("named", me.Name);
        Assert.Equal(BuiltInRoles.Supervisor, me.Role);
        Assert.Equal(["inventory.view"], me.Permissions);
    }

    [Fact]
    public async Task A_deactivated_user_is_signed_out_on_the_next_request()
    {
        const string email = "leaving@codaxy.com";
        var client = await app.ClientAsync(email);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/auth/me")).StatusCode);

        await app.UserAsync(email, active: false);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await client.GetAsync("/api/auth/me")).StatusCode
        );

        // And stays out after reactivation until signing in again: the cookie was removed.
        await app.UserAsync(email);
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await client.GetAsync("/api/auth/me")).StatusCode
        );
    }

    [Fact]
    public async Task A_changed_role_applies_on_the_next_request()
    {
        const string email = "demoted@codaxy.com";
        var client = await app.ClientAsync(email, BuiltInRoles.Admin);
        Assert.Equal(
            HttpStatusCode.OK,
            (await client.GetAsync("/api/administration/users/")).StatusCode
        );

        await app.UserAsync(email, BuiltInRoles.Supervisor);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            (await client.GetAsync("/api/administration/users/")).StatusCode
        );
    }

    [Fact]
    public async Task System_admin_holds_every_permission_in_the_catalogue()
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();

        var held = await context
            .RolePermissions.Where(p =>
                context.Roles.Any(r => r.Id == p.RoleId && r.Name == BuiltInRoles.SystemAdmin)
            )
            .Select(p => p.Permission)
            .ToListAsync();

        Assert.Equal(App.Shared.Access.Permissions.Keys.Order(), held.Order());
    }
}
