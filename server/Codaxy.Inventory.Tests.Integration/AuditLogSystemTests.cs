using System.Net.Http.Json;
using Codaxy.Inventory.App.Administration.AuditLogs;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Codaxy.Inventory.Tests.Integration;

/// <summary>
/// A role seeded at startup and later renamed by a person, and one save of a person's elsewhere: the
/// seeding is a system save, the rest are not.
/// </summary>
public class SystemSavesApplication : InventoryApplication
{
    public static readonly Guid Seeded = Guid.CreateVersion7();
    public static readonly Guid Edited = Guid.CreateVersion7();

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();

        // Only the rows below: the built-in roles' seeding was audited too.
        await context.AuditLogs.ExecuteDeleteAsync();

        var t0 = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
        context.AuditLogs.AddRange(
            Row("Role", Seeded, "Create", AuditLogInterceptor.SystemUser, t0),
            Row("Role", Seeded, "Update", "ana@codaxy.com", t0.AddMinutes(1)),
            Row("Client", Edited, "Create", "bob@codaxy.com", t0.AddMinutes(2))
        );
        await context.SaveChangesAsync();
    }

    private static AuditLog Row(
        string table,
        Guid entity,
        string action,
        string email,
        DateTimeOffset time
    ) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            Table = table,
            EntityId = entity,
            ActionType = action,
            Email = email,
            TimeCreated = time,
            TransactionId = Guid.CreateVersion7(),
            NewValuesJson = "{}",
        };
}

/// <summary>The log leaves system saves out unless asked, or unless it is one record's history.</summary>
public class AuditLogSystemTests(SystemSavesApplication app) : IClassFixture<SystemSavesApplication>
{
    private const string Url = "/api/administration/audit-log";

    private async Task<IReadOnlyList<string>> Emails(string query)
    {
        var client = await app.ClientAsync("reader@codaxy.com");
        var page = (await client.GetFromJsonAsync<Page<Entry>>($"{Url}/?{query}"))!;

        return page.Items.Select(e => e.Email).ToList();
    }

    [Fact]
    public async Task Leaves_system_saves_out_by_default()
    {
        Assert.DoesNotContain(AuditLogInterceptor.SystemUser, await Emails(""));
        Assert.Equal(2, (await Emails("")).Count);
    }

    [Fact]
    public async Task Shows_them_when_asked()
    {
        Assert.Contains(AuditLogInterceptor.SystemUser, await Emails("system=true"));
    }

    [Fact]
    public async Task The_user_filter_only_narrows()
    {
        Assert.Empty(await Emails("email=system"));
        Assert.Equal([AuditLogInterceptor.SystemUser], await Emails("email=system&system=true"));
    }

    [Fact]
    public async Task A_records_history_is_whole()
    {
        Assert.Equal(2, (await Emails($"entityId={SystemSavesApplication.Seeded}")).Count);
        Assert.Equal(2, (await Emails($"q={SystemSavesApplication.Seeded}")).Count);
    }
}
