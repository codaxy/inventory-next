using Codaxy.Inventory.App.Administration.AuditLogs;
using Codaxy.Inventory.App.ElectronicDevices.Tags;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

namespace Codaxy.Inventory.Tests.Unit;

/// <summary>
/// The audit JSON, checked against the format the original application writes, on a context that
/// never connects: EF builds its model without a database.
/// </summary>
public class AuditLogInterceptorTests
{
    private sealed class User(string? email) : ICurrentUser
    {
        public string? Email { get; } = email;
    }

    private static InventoryContext Context() =>
        new(new DbContextOptionsBuilder<InventoryContext>().UseNpgsql("Host=unused").Options);

    private static readonly Asset Laptop = new()
    {
        Id = Guid.Parse("019cb3b9-5a49-4aa0-8703-b40ccfcb58bc"),
        Name = "lap-ura-sk",
        Description = "izbačena dva 16GB RAM-a\nMS Group",
        InventoryNumber = 100893,
        PurchaseDate = new DateOnly(2026, 3, 1),
        PurchaseValue = 2053.09m,
        LastModified = new DateTimeOffset(2026, 9, 24, 12, 17, 20, TimeSpan.Zero),
    };

    [Fact]
    public void Properties_come_key_first_then_by_name_as_the_original_writes_them()
    {
        using var context = Context();
        var json = AuditLogInterceptor.Json(context.Entry(Laptop), original: false);

        var names = System
            .Text.Json.JsonDocument.Parse(json)
            .RootElement.EnumerateObject()
            .Select(p => p.Name)
            .ToList();

        Assert.Equal("Id", names[0]);
        Assert.Equal(names.Skip(1).OrderBy(n => n, StringComparer.Ordinal), names.Skip(1));
    }

    [Fact]
    public void Values_are_written_as_the_original_writes_them()
    {
        using var context = Context();
        var json = AuditLogInterceptor.Json(context.Entry(Laptop), original: false);

        Assert.Contains("\"PurchaseDate\": \"2026-03-01\"", json);
        Assert.Contains("\"PurchaseValue\": 2053.09", json);
        Assert.Contains("\"InventoryNumber\": 100893", json);
        Assert.Contains("\"LastModified\": \"2026-09-24T12:17:20+00:00\"", json);
        Assert.Contains("\"AssetSubstatusId\": null", json);
        Assert.Contains("\"Id\": \"019cb3b9-5a49-4aa0-8703-b40ccfcb58bc\"", json);
        Assert.Contains("izbačena dva 16GB RAM-a\\nMS Group", json);
        Assert.Contains("\n  \"Name\": \"lap-ura-sk\"", json);
    }

    [Fact]
    public void The_screen_reads_what_the_writer_writes()
    {
        using var context = Context();
        var values = AuditValues.Parse(
            AuditLogInterceptor.Json(context.Entry(Laptop), original: false)
        );

        Assert.Equal("lap-ura-sk", values.GetString("Name"));
        Assert.Equal(100893, values.GetInt("InventoryNumber"));
        Assert.Equal(ValueKind.Date, values.Get("PurchaseDate")!.Kind);
        Assert.Equal(ValueKind.Instant, values.Get("LastModified")!.Kind);
    }

    [Fact]
    public void A_save_records_create_update_and_delete_with_the_user_and_one_transaction()
    {
        using var context = Context();
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 9, 26, 10, 0, 0, TimeSpan.Zero));
        var interceptor = new AuditLogInterceptor(new User("ana@codaxy.com"), clock);

        var created = new ElectronicDeviceTag { Id = Guid.CreateVersion7(), Name = "New" };
        var changed = new ElectronicDeviceTag { Id = Guid.CreateVersion7(), Name = "Old" };
        var removed = new ElectronicDeviceTag { Id = Guid.CreateVersion7(), Name = "Gone" };

        context.Add(created);
        context.Attach(changed);
        changed.Name = "Renamed";
        context.Attach(removed);
        context.Remove(removed);

        interceptor.SavingChanges(
            new Microsoft.EntityFrameworkCore.Diagnostics.DbContextEventData(null!, null!, context),
            default
        );

        var logs = context
            .ChangeTracker.Entries<AuditLog>()
            .Select(e => e.Entity)
            .ToDictionary(l => l.EntityId);

        Assert.Equal(
            ("Create", null),
            (logs[created.Id].ActionType, logs[created.Id].OldValuesJson)
        );
        Assert.Equal("Update", logs[changed.Id].ActionType);
        Assert.Contains("\"Name\": \"Old\"", logs[changed.Id].OldValuesJson);
        Assert.Contains("\"Name\": \"Renamed\"", logs[changed.Id].NewValuesJson);
        Assert.Equal(
            ("Delete", null),
            (logs[removed.Id].ActionType, logs[removed.Id].NewValuesJson)
        );
        Assert.All(logs.Values, l => Assert.Equal("ana@codaxy.com", l.Email));
        Assert.All(logs.Values, l => Assert.Equal("ElectronicDeviceTag", l.Table));
        Assert.All(logs.Values, l => Assert.Equal(clock.GetUtcNow(), l.TimeCreated));
        Assert.Single(logs.Values.Select(l => l.TransactionId).Distinct());
    }

    [Fact]
    public void Without_a_user_the_writer_is_the_system()
    {
        using var context = Context();
        var interceptor = new AuditLogInterceptor(new User(null), TimeProvider.System);

        context.Add(new ElectronicDeviceTag { Id = Guid.CreateVersion7(), Name = "x" });
        interceptor.SavingChanges(
            new Microsoft.EntityFrameworkCore.Diagnostics.DbContextEventData(null!, null!, context),
            default
        );

        Assert.Equal(
            AuditLogInterceptor.SystemUser,
            context.ChangeTracker.Entries<AuditLog>().Single().Entity.Email
        );
    }

    [Fact]
    public void The_log_does_not_log_itself()
    {
        using var context = Context();
        var interceptor = new AuditLogInterceptor(new User(null), TimeProvider.System);

        context.Add(new AuditLog { Id = Guid.CreateVersion7(), Table = "x" });
        interceptor.SavingChanges(
            new Microsoft.EntityFrameworkCore.Diagnostics.DbContextEventData(null!, null!, context),
            default
        );

        Assert.Single(context.ChangeTracker.Entries<AuditLog>());
    }
}
