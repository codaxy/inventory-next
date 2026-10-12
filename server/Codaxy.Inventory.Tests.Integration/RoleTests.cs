using System.Net;
using System.Net.Http.Json;
using Codaxy.Inventory.App.Administration.Roles;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Access;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RoleItem = Codaxy.Inventory.App.Administration.Roles.List.Endpoint.Item;

namespace Codaxy.Inventory.Tests.Integration;

/// <summary>
/// The built-in roles, and "Company keeper": manages roles and holds the inventory and nothing else —
/// not the audit log, the server log or users — so the subset rule has something to refuse.
/// </summary>
public class RoleApplication : InventoryApplication
{
    public const string Keeper = "Company keeper";

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await RoleAsync(
            Keeper,
            Permissions.RolesManage,
            Permissions.InventoryView,
            Permissions.InventoryEdit
        );
    }

    /// <summary>A custom role holding <paramref name="permissions"/>, made so.</summary>
    public async Task<Guid> RoleAsync(string name, params string[] permissions)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
        var role =
            await context.Roles.Include(r => r.Permissions).FirstOrDefaultAsync(r => r.Name == name)
            ?? context.Roles.Add(new Role { Id = Guid.CreateVersion7(), Name = name }).Entity;
        Roles.Grant(context, role, permissions);
        await context.SaveChangesAsync();
        return role.Id;
    }

    public async Task<Guid> IdOf(string name)
    {
        using var scope = Services.CreateScope();
        return await scope
            .ServiceProvider.GetRequiredService<InventoryContext>()
            .Roles.Where(r => r.Name == name)
            .Select(r => r.Id)
            .SingleAsync();
    }
}

public class RoleTests(RoleApplication app) : IClassFixture<RoleApplication>
{
    private const string Url = "/api/administration/roles";

    private Task<HttpClient> Root() => app.ClientAsync("root@codaxy.com", BuiltInRoles.SystemAdmin);

    private Task<HttpClient> Keeper() =>
        app.ClientAsync("keeper@codaxy.com", RoleApplication.Keeper);

    [Fact]
    public async Task A_system_admin_creates_a_role_listed_after_the_built_in_ones()
    {
        var created = await (await Root()).PostAsJsonAsync(
            $"{Url}/",
            new
            {
                name = "Auditor",
                description = "Reads the audit log.",
                permissions = new[] { Permissions.AuditLogView },
            }
        );
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var page = (await (await Root()).GetFromJsonAsync<Page<RoleItem>>($"{Url}/?pageSize=100"))!;
        var names = page.Items.Select(r => r.Name).ToList();
        Assert.Equal(
            [BuiltInRoles.SystemAdmin, BuiltInRoles.Admin, BuiltInRoles.Supervisor],
            names.Take(3)
        );
        Assert.Contains("Auditor", names);
    }

    [Theory]
    [InlineData(new[] { Permissions.InventoryEdit }, "inventory.view")]
    [InlineData(new[] { "company.own" }, "company.own")]
    public async Task Refuses_an_edit_without_its_view_and_an_unknown_permission(
        string[] permissions,
        string named
    )
    {
        var response = await (await Root()).PostAsJsonAsync(
            $"{Url}/",
            new { name = $"Broken {named}", permissions }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(named, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task A_name_is_taken_once_whatever_its_case()
    {
        var response = await (await Root()).PostAsJsonAsync(
            $"{Url}/",
            new { name = " supervisor ", permissions = Array.Empty<string>() }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Nobody_grants_what_they_do_not_hold()
    {
        var keeper = await Keeper();

        var beyond = await keeper.PostAsJsonAsync(
            $"{Url}/",
            new { name = "Auditing", permissions = new[] { Permissions.AuditLogView } }
        );
        Assert.Equal(HttpStatusCode.BadRequest, beyond.StatusCode);

        var within = await keeper.PostAsJsonAsync(
            $"{Url}/",
            new { name = "Company reader", permissions = new[] { Permissions.InventoryView } }
        );
        Assert.Equal(HttpStatusCode.Created, within.StatusCode);
    }

    [Fact]
    public async Task A_role_granting_more_than_the_manager_is_out_of_reach()
    {
        var keeper = await Keeper();
        var admin = await app.IdOf(BuiltInRoles.Admin);

        var update = await keeper.PutAsJsonAsync(
            $"{Url}/{admin}",
            new { name = BuiltInRoles.Admin, permissions = Array.Empty<string>() }
        );
        Assert.Equal(HttpStatusCode.Forbidden, update.StatusCode);

        var detail = await keeper.GetFromJsonAsync<RoleDetail>($"{Url}/{admin}");
        Assert.False(detail!.CanManage);
    }

    [Fact]
    public async Task Nobody_changes_the_role_they_hold()
    {
        var keeper = await Keeper();
        var own = await app.IdOf(RoleApplication.Keeper);

        var update = await keeper.PutAsJsonAsync(
            $"{Url}/{own}",
            new { name = RoleApplication.Keeper, permissions = new[] { Permissions.InventoryView } }
        );
        Assert.Equal(HttpStatusCode.Conflict, update.StatusCode);
        Assert.Equal(
            HttpStatusCode.Conflict,
            (await keeper.DeleteAsync($"{Url}/{own}")).StatusCode
        );
        Assert.True((await keeper.GetFromJsonAsync<RoleDetail>($"{Url}/{own}"))!.IsOwn);
    }

    [Fact]
    public async Task System_admin_changes_for_nobody()
    {
        var root = await Root();
        var id = await app.IdOf(BuiltInRoles.SystemAdmin);

        Assert.Equal(
            HttpStatusCode.Conflict,
            (
                await root.PutAsJsonAsync(
                    $"{Url}/{id}",
                    new { name = BuiltInRoles.SystemAdmin, permissions = Array.Empty<string>() }
                )
            ).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.Conflict,
            (await root.PostAsync($"{Url}/{id}/reset", null)).StatusCode
        );
        Assert.Equal(HttpStatusCode.Conflict, (await root.DeleteAsync($"{Url}/{id}")).StatusCode);
    }

    [Fact]
    public async Task A_built_in_role_keeps_its_name_and_resets_to_its_defaults()
    {
        var root = await Root();
        var id = await app.IdOf(BuiltInRoles.Supervisor);

        var renamed = await root.PutAsJsonAsync(
            $"{Url}/{id}",
            new { name = "Watcher", permissions = new[] { Permissions.InventoryView } }
        );
        Assert.Equal(HttpStatusCode.BadRequest, renamed.StatusCode);

        var narrowed = await root.PutAsJsonAsync(
            $"{Url}/{id}",
            new
            {
                name = BuiltInRoles.Supervisor,
                description = "Narrowed.",
                permissions = Array.Empty<string>(),
            }
        );
        Assert.Equal(HttpStatusCode.OK, narrowed.StatusCode);
        Assert.Empty((await narrowed.Content.ReadFromJsonAsync<RoleDetail>())!.Permissions);

        var reset = await root.PostAsync($"{Url}/{id}/reset", null);
        Assert.Equal(HttpStatusCode.OK, reset.StatusCode);
        var detail = (await reset.Content.ReadFromJsonAsync<RoleDetail>())!;
        Assert.Equal([Permissions.InventoryView], detail.Permissions);
        Assert.NotEqual("Narrowed.", detail.Description);
    }

    [Fact]
    public async Task A_custom_role_has_no_defaults_to_reset_to()
    {
        var id = await app.RoleAsync("Custom reset", Permissions.InventoryView);

        Assert.Equal(
            HttpStatusCode.Conflict,
            (await (await Root()).PostAsync($"{Url}/{id}/reset", null)).StatusCode
        );
    }

    [Fact]
    public async Task A_role_someone_holds_is_not_deleted_and_one_nobody_holds_is()
    {
        var root = await Root();
        var held = await app.RoleAsync("Held", Permissions.InventoryView);
        await app.UserAsync("holder@codaxy.com", "Held");
        var free = await app.RoleAsync("Free", Permissions.InventoryView);

        var refused = await root.DeleteAsync($"{Url}/{held}");
        Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
        Assert.Contains("1 user", await refused.Content.ReadAsStringAsync());

        Assert.Equal(
            HttpStatusCode.NoContent,
            (await root.DeleteAsync($"{Url}/{free}")).StatusCode
        );
        Assert.Equal(HttpStatusCode.NotFound, (await root.GetAsync($"{Url}/{free}")).StatusCode);
    }

    [Fact]
    public async Task A_built_in_role_is_not_deleted()
    {
        Assert.Equal(
            HttpStatusCode.Conflict,
            (
                await (await Root()).DeleteAsync($"{Url}/{await app.IdOf(BuiltInRoles.Admin)}")
            ).StatusCode
        );
    }

    [Fact]
    public async Task An_admin_cannot_reach_roles()
    {
        var admin = await app.ClientAsync("plain-admin@codaxy.com", BuiltInRoles.Admin);

        Assert.Equal(HttpStatusCode.Forbidden, (await admin.GetAsync($"{Url}/")).StatusCode);
    }

    [Fact]
    public async Task The_catalogue_says_what_the_reader_may_grant()
    {
        var entries = (
            await (
                await Keeper()
            ).GetFromJsonAsync<App.Administration.Roles.Catalogue.Endpoint.Entry[]>(
                $"{Url}/catalogue"
            )
        )!;

        Assert.True(entries.Single(e => e.Key == Permissions.InventoryEdit).Grantable);
        Assert.False(entries.Single(e => e.Key == Permissions.AuditLogView).Grantable);
        Assert.Equal(
            Permissions.InventoryView,
            entries.Single(e => e.Key == Permissions.InventoryEdit).Implies
        );
    }

    [Fact]
    public async Task A_granted_permission_is_audited()
    {
        var id = (
            await (
                await (await Root()).PostAsJsonAsync(
                    $"{Url}/",
                    new { name = "Audited", permissions = new[] { Permissions.InventoryView } }
                )
            ).Content.ReadFromJsonAsync<RoleDetail>()
        )!.Id;

        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
        var row = await context.RolePermissions.SingleAsync(p => p.RoleId == id);
        Assert.True(
            await context.AuditLogs.AnyAsync(a => a.EntityId == row.Id && a.ActionType == "Create")
        );
    }
}
