using System.Net;
using System.Net.Http.Json;
using Codaxy.Inventory.App.Administration.Roles;
using Codaxy.Inventory.App.Administration.Users;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Access;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RoleOptions = Codaxy.Inventory.App.Administration.Users.Options.Endpoint.Response;
using UserItem = Codaxy.Inventory.App.Administration.Users.List.Endpoint.Item;

namespace Codaxy.Inventory.Tests.Integration;

/// <summary>The built-in roles, and a custom one holding what Admin does not: the server log.</summary>
public class UserApplication : InventoryApplication
{
    public const string LogReader = "Log reader";

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
        var role = new Role
        {
            Id = Guid.CreateVersion7(),
            Name = LogReader,
            Description = "Reads the server log.",
        };
        context.Roles.Add(role);
        context.RolePermissions.AddRange(
            new RolePermission
            {
                Id = Guid.CreateVersion7(),
                RoleId = role.Id,
                Permission = Permissions.ServerLogView,
            },
            new RolePermission
            {
                Id = Guid.CreateVersion7(),
                RoleId = role.Id,
                Permission = Permissions.UsersManage,
            }
        );
        await context.SaveChangesAsync();
    }

    public async Task<Guid> RoleAsync(string name)
    {
        using var scope = Services.CreateScope();
        return await scope
            .ServiceProvider.GetRequiredService<InventoryContext>()
            .Roles.Where(r => r.Name == name)
            .Select(r => r.Id)
            .SingleAsync();
    }
}

public class UserTests(UserApplication app) : IClassFixture<UserApplication>
{
    private const string Url = "/api/administration/users";

    private Task<HttpClient> Admin() => app.ClientAsync("admin@codaxy.com", BuiltInRoles.Admin);

    private Task<HttpClient> SystemAdmin() =>
        app.ClientAsync("root@codaxy.com", BuiltInRoles.SystemAdmin);

    [Fact]
    public async Task An_admin_adds_a_user_who_can_then_sign_in()
    {
        var created = await (await Admin()).PostAsJsonAsync(
            $"{Url}/",
            new
            {
                email = " New.Person@Codaxy.com ",
                name = "New Person",
                roleId = await app.RoleAsync(BuiltInRoles.Supervisor),
            }
        );
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var user = (await created.Content.ReadFromJsonAsync<UserDetail>())!;
        Assert.Equal("new.person@codaxy.com", user.Email);
        Assert.True(user.IsActive);

        await app.SignInAsync("new.person@codaxy.com");
    }

    [Fact]
    public async Task An_email_is_added_once()
    {
        await app.UserAsync("twice@codaxy.com");

        var response = await (await Admin()).PostAsJsonAsync(
            $"{Url}/",
            new
            {
                email = "TWICE@codaxy.com",
                name = "Twice",
                roleId = await app.RoleAsync(BuiltInRoles.Supervisor),
            }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("already exists", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task An_admin_gives_neither_system_admin_nor_a_role_holding_what_admin_lacks()
    {
        var admin = await Admin();

        foreach (var role in new[] { BuiltInRoles.SystemAdmin, UserApplication.LogReader })
        {
            var response = await admin.PostAsJsonAsync(
                $"{Url}/",
                new
                {
                    email = "climber@codaxy.com",
                    name = "Climber",
                    roleId = await app.RoleAsync(role),
                }
            );
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        var options = (await admin.GetFromJsonAsync<RoleOptions>($"{Url}/options"))!;
        Assert.Equal(
            [BuiltInRoles.Admin, BuiltInRoles.Supervisor],
            options.Roles.Select(r => r.Name)
        );
    }

    [Fact]
    public async Task A_system_admin_is_offered_every_role_in_authority_order()
    {
        var options = (
            await (await SystemAdmin()).GetFromJsonAsync<RoleOptions>($"{Url}/options")
        )!;

        Assert.Equal(
            [
                BuiltInRoles.SystemAdmin,
                BuiltInRoles.Admin,
                BuiltInRoles.Supervisor,
                UserApplication.LogReader,
            ],
            options.Roles.Select(r => r.Name)
        );
    }

    [Fact]
    public async Task An_admin_cannot_touch_a_user_whose_role_grants_more()
    {
        var admin = await Admin();
        var root = await app.UserAsync("other-root@codaxy.com", BuiltInRoles.SystemAdmin);
        var reader = await app.UserAsync("log-reader@codaxy.com", UserApplication.LogReader);

        foreach (var user in new[] { root, reader })
        {
            Assert.Equal(
                HttpStatusCode.Forbidden,
                (await admin.PostAsync($"{Url}/{user.Id}/deactivate", null)).StatusCode
            );
            Assert.Equal(
                HttpStatusCode.Forbidden,
                (
                    await admin.PutAsJsonAsync(
                        $"{Url}/{user.Id}",
                        new { name = "Renamed", roleId = user.RoleId }
                    )
                ).StatusCode
            );

            var detail = await admin.GetFromJsonAsync<UserDetail>($"{Url}/{user.Id}");
            Assert.False(detail!.CanManage);
        }
    }

    [Fact]
    public async Task Nobody_changes_their_own_role_or_activity_but_may_rename_themselves()
    {
        var admin = await Admin();
        var self = await app.UserAsync("admin@codaxy.com");

        var promoted = await admin.PutAsJsonAsync(
            $"{Url}/{self.Id}",
            new { name = "Me", roleId = await app.RoleAsync(BuiltInRoles.Supervisor) }
        );
        Assert.Equal(HttpStatusCode.Conflict, promoted.StatusCode);

        var deactivated = await admin.PostAsync($"{Url}/{self.Id}/deactivate", null);
        Assert.Equal(HttpStatusCode.Conflict, deactivated.StatusCode);

        var renamed = await admin.PutAsJsonAsync(
            $"{Url}/{self.Id}",
            new { name = "Admin Person", roleId = self.RoleId }
        );
        Assert.Equal(HttpStatusCode.OK, renamed.StatusCode);
        Assert.True((await renamed.Content.ReadFromJsonAsync<UserDetail>())!.IsSelf);
    }

    [Fact]
    public async Task A_user_is_deactivated_and_reactivated_once_each()
    {
        var admin = await Admin();
        var user = await app.UserAsync("toggled@codaxy.com", BuiltInRoles.Supervisor);

        Assert.Equal(
            HttpStatusCode.OK,
            (await admin.PostAsync($"{Url}/{user.Id}/deactivate", null)).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.Conflict,
            (await admin.PostAsync($"{Url}/{user.Id}/deactivate", null)).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.Forbidden,
            (await app.VerifyAsync(app.Client(), "toggled@codaxy.com")).StatusCode
        );

        Assert.Equal(
            HttpStatusCode.OK,
            (await admin.PostAsync($"{Url}/{user.Id}/reactivate", null)).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.Conflict,
            (await admin.PostAsync($"{Url}/{user.Id}/reactivate", null)).StatusCode
        );
        await app.SignInAsync("toggled@codaxy.com");
    }

    [Fact]
    public async Task A_role_is_changed_to_one_the_manager_holds()
    {
        var admin = await Admin();
        var user = await app.UserAsync("moved@codaxy.com", BuiltInRoles.Supervisor);

        var response = await admin.PutAsJsonAsync(
            $"{Url}/{user.Id}",
            new { name = "Moved", roleId = await app.RoleAsync(BuiltInRoles.Admin) }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var detail = (await response.Content.ReadFromJsonAsync<UserDetail>())!;
        Assert.Equal(BuiltInRoles.Admin, detail.Role);
        Assert.Equal("Moved", detail.Name);
    }

    [Fact]
    public async Task The_last_active_system_admin_keeps_the_role()
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();

        foreach (var other in await context.AppUsers.Where(u => u.Role.IsSystemAdmin).ToListAsync())
            other.IsActive = false;
        await context.SaveChangesAsync();

        var last = await app.UserAsync("last-root@codaxy.com", BuiltInRoles.SystemAdmin);
        var tracked = await context.AppUsers.Include(u => u.Role).SingleAsync(u => u.Id == last.Id);

        Assert.True(await Users.IsLastSystemAdminAsync(context, tracked, CancellationToken.None));

        await app.UserAsync("second-root@codaxy.com", BuiltInRoles.SystemAdmin);
        Assert.False(await Users.IsLastSystemAdminAsync(context, tracked, CancellationToken.None));
    }

    [Fact]
    public async Task Lists_users_by_name_and_searches_their_role()
    {
        await app.UserAsync("supervising@codaxy.com", BuiltInRoles.Supervisor);

        var page = (
            await (await Admin()).GetFromJsonAsync<Page<UserItem>>($"{Url}/?q=supervisor")
        )!;

        Assert.All(page.Items, u => Assert.Equal(BuiltInRoles.Supervisor, u.Role));
        Assert.Contains(page.Items, u => u.Email == "supervising@codaxy.com");
    }

    [Fact]
    public async Task A_supervisor_cannot_reach_users_at_all()
    {
        var client = await app.ClientAsync("watcher@codaxy.com", BuiltInRoles.Supervisor);

        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync($"{Url}/")).StatusCode);
    }
}
