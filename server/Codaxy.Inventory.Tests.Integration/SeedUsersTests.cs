using Codaxy.Inventory.App.Administration.Roles;
using Codaxy.Inventory.App.Administration.Users;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Codaxy.Inventory.Tests.Integration;

public class SeedUsersApplication : InventoryApplication;

/// <summary><c>Auth:SeedUsers</c> and the start it refuses. Each test sets the users it needs.</summary>
public class SeedUsersTests(SeedUsersApplication app) : IClassFixture<SeedUsersApplication>
{
    private static UserOptions Seeding(params (string Email, string Role)[] users) =>
        new()
        {
            SeedUsers = users
                .Select(u => new UserOptions.SeedUser
                {
                    Email = u.Email,
                    Name = u.Email,
                    Role = u.Role,
                })
                .ToArray(),
        };

    private async Task ApplyAsync(UserOptions options)
    {
        using var scope = app.Services.CreateScope();
        await SeedUsers.ApplyAsync(
            scope.ServiceProvider.GetRequiredService<InventoryContext>(),
            options,
            TimeProvider.System
        );
    }

    private async Task DeactivateEveryoneAsync()
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
        await context.AppUsers.ExecuteUpdateAsync(u => u.SetProperty(x => x.IsActive, false));
    }

    [Fact]
    public async Task Refuses_a_start_with_no_active_system_admin()
    {
        await DeactivateEveryoneAsync();

        var refused = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ApplyAsync(Seeding(("helper@codaxy.com", BuiltInRoles.Admin)))
        );
        Assert.Contains("SeedUsers", refused.Message);
    }

    [Fact]
    public async Task Creates_a_missing_user_and_leaves_an_existing_one_as_it_is()
    {
        await DeactivateEveryoneAsync();
        await app.UserAsync("kept@codaxy.com", BuiltInRoles.Supervisor);

        await ApplyAsync(
            Seeding(
                ("First.Admin@codaxy.com ", BuiltInRoles.SystemAdmin),
                ("kept@codaxy.com", BuiltInRoles.Admin)
            )
        );

        using var scope = app.Services.CreateScope();
        var users = await scope
            .ServiceProvider.GetRequiredService<InventoryContext>()
            .AppUsers.Include(u => u.Role)
            .ToDictionaryAsync(u => u.Email);

        Assert.True(users["first.admin@codaxy.com"].IsActive);
        Assert.Equal(BuiltInRoles.SystemAdmin, users["first.admin@codaxy.com"].Role.Name);
        Assert.Equal(BuiltInRoles.Supervisor, users["kept@codaxy.com"].Role.Name);
    }

    [Fact]
    public async Task Refuses_a_role_the_database_does_not_have()
    {
        var refused = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ApplyAsync(Seeding(("someone@codaxy.com", "Owner")))
        );

        Assert.Contains("Owner", refused.Message);
    }

    [Fact]
    public async Task Seeding_the_roles_again_changes_nothing_but_system_admin()
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
        var supervisor = await context.Roles.SingleAsync(r => r.Name == BuiltInRoles.Supervisor);
        await context
            .RolePermissions.Where(p =>
                p.RoleId == supervisor.Id && p.Permission == "dashboard.view"
            )
            .ExecuteDeleteAsync();
        context.RolePermissions.Add(
            new RolePermission
            {
                Id = Guid.CreateVersion7(),
                RoleId = supervisor.Id,
                Permission = "gone.view",
            }
        );
        await context.SaveChangesAsync();

        await BuiltInRoles.SeedAsync(context, CancellationToken.None);

        var held = await context
            .RolePermissions.Where(p => p.RoleId == supervisor.Id)
            .Select(p => p.Permission)
            .ToListAsync();
        Assert.DoesNotContain("dashboard.view", held);
        Assert.DoesNotContain("gone.view", held);
    }
}
