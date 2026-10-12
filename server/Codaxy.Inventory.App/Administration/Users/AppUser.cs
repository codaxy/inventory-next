using Codaxy.Inventory.App.Administration.Roles;
using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codaxy.Inventory.App.Administration.Users;

/// <summary>
/// Someone who may sign in, added by an administrator. Not <c>User</c>: its table would be
/// <c>user</c>, which PostgreSQL reserves, so hand-written SQL against it fails unquoted.
/// </summary>
public class AppUser : IIdentifiable<Guid>
{
    public Guid Id { get; set; }

    /// <summary>Trimmed and lower-case; fixed once the user is added.</summary>
    public string Email { get; set; } = null!;

    /// <summary>Typed by whoever adds the user, never taken from a provider.</summary>
    public string Name { get; set; } = null!;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    /// <summary>A user is deactivated, never deleted.</summary>
    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public static string Normalize(string email) => email.Trim().ToLowerInvariant();
}

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.Property(u => u.Email).HasMaxLength(250);
        builder.Property(u => u.Name).HasMaxLength(200);
        builder.HasIndex(u => u.Email).IsUnique();
        builder
            .HasOne(u => u.Role)
            .WithMany()
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
