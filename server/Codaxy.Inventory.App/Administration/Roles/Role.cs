using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Access;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codaxy.Inventory.App.Administration.Roles;

/// <summary>A named set of permissions. A user holds exactly one.</summary>
public class Role : IIdentifiable<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = "";

    /// <summary>
    /// Grants every permission, including ones added later. Only System Admin carries it, and
    /// nothing about that role changes.
    /// </summary>
    public bool IsSystemAdmin { get; set; }

    /// <summary>Seeded and found by its name, which therefore never changes.</summary>
    public bool IsBuiltIn { get; set; }

    public List<RolePermission> Permissions { get; set; } = [];

    /// <summary>What the role grants: the catalogue for System Admin, its rows for any other.</summary>
    public IReadOnlySet<string> Grants() =>
        IsSystemAdmin
            ? Shared.Access.Permissions.Keys
            : Permissions.Select(p => p.Permission).ToHashSet();
}

/// <summary>
/// One permission held by one role. An id of its own, unlike a link table's composite key, so the
/// audit log records a permission granted or revoked.
/// </summary>
public class RolePermission : IIdentifiable<Guid>
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public string Permission { get; set; } = null!;
}

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.Property(r => r.Name).HasMaxLength(100);
        builder.Property(r => r.Description).HasMaxLength(500);
        builder.HasIndex(r => r.Name).IsUnique();
        builder.HasMany(r => r.Permissions).WithOne().HasForeignKey(p => p.RoleId);
    }
}

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.Property(p => p.Permission).HasMaxLength(100);
        builder.HasIndex(p => new { p.RoleId, p.Permission }).IsUnique();
    }
}
