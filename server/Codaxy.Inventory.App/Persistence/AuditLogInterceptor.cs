using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Codaxy.Inventory.App.Persistence;

/// <summary>
/// One <see cref="AuditLog"/> row for every entity a save adds, modifies or deletes. The format is
/// the original application's, because both write into one log: the entity's class name, its id, the
/// user's address (<see cref="SystemUser"/> without one), <c>Create</c>/<c>Update</c>/<c>Delete</c>, and the
/// complete property set before and after as indented JSON in EF's property order. Rows of one save
/// share a transaction id.
///
/// The user is read when the save happens, not when the interceptor was made — the original captured
/// the request in its constructor and attributed writes by the request that created it.
/// </summary>
public sealed class AuditLogInterceptor(ICurrentUser user, TimeProvider clock)
    : SaveChangesInterceptor
{
    public const string SystemUser = "system";

    /// <summary>Non-ASCII written as it is, as the original's writer does; indented as it is.</summary>
    private static readonly JsonWriterOptions Writer = new()
    {
        Indented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    )
    {
        Record(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        Record(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private void Record(DbContext? context)
    {
        if (context is null)
            return;

        var entries = context
            .ChangeTracker.Entries<IIdentifiableReadOnly<Guid>>()
            .Where(e =>
                e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
                && e.Entity is not AuditLog
            )
            .ToList();

        if (entries.Count == 0)
            return;

        var transaction =
            context.Database.CurrentTransaction?.TransactionId ?? Guid.CreateVersion7();
        var email = user.Email ?? SystemUser;
        var now = clock.GetUtcNow();

        context
            .Set<AuditLog>()
            .AddRange(
                entries.Select(entry => new AuditLog
                {
                    Id = Guid.CreateVersion7(),
                    Table = entry.Metadata.ClrType.Name,
                    EntityId = entry.Entity.Id,
                    Email = email,
                    TransactionId = transaction,
                    TimeCreated = now,
                    ActionType = entry.State switch
                    {
                        EntityState.Added => "Create",
                        EntityState.Modified => "Update",
                        _ => "Delete",
                    },
                    OldValuesJson =
                        entry.State == EntityState.Added ? null : Json(entry, original: true),
                    NewValuesJson =
                        entry.State == EntityState.Deleted ? null : Json(entry, original: false),
                })
            );
    }

    /// <summary>Every property, in EF's order — the key first, then by name — changed or not.</summary>
    internal static string Json(EntityEntry entry, bool original)
    {
        using var stream = new MemoryStream();

        using (var writer = new Utf8JsonWriter(stream, Writer))
        {
            writer.WriteStartObject();

            foreach (var property in entry.Metadata.GetProperties())
            {
                var value = original
                    ? entry.Property(property.Name).OriginalValue
                    : entry.Property(property.Name).CurrentValue;

                writer.WritePropertyName(property.Name);
                JsonSerializer.Serialize(writer, value, value?.GetType() ?? typeof(object));
            }

            writer.WriteEndObject();
        }

        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }
}
