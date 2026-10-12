using System.ComponentModel.DataAnnotations;
using Codaxy.Inventory.App.Administration.Roles;
using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Administration.Users;

/// <summary>What adding a user takes. The email is fixed afterwards.</summary>
public sealed record NewUserForm(
    [property:
        Required(ErrorMessage = "Give the user's email."),
        EmailAddress(ErrorMessage = "That is not an email address."),
        StringLength(250, ErrorMessage = "An email is at most 250 characters.")
    ]
        string? Email,
    [property:
        Required(ErrorMessage = "Give the user a name."),
        StringLength(200, ErrorMessage = "A name is at most 200 characters.")
    ]
        string? Name,
    [property: Required(ErrorMessage = "Choose a role.")] Guid? RoleId
);

/// <summary>What editing a user takes.</summary>
public sealed record UserForm(
    [property:
        Required(ErrorMessage = "Give the user a name."),
        StringLength(200, ErrorMessage = "A name is at most 200 characters.")
    ]
        string? Name,
    [property: Required(ErrorMessage = "Choose a role.")] Guid? RoleId
);

/// <summary>A user as its page shows it.</summary>
/// <param name="IsSelf">The reader's own user: only its name may change.</param>
/// <param name="CanManage">Whether the reader may change it at all, by the subset rule.</param>
public sealed record UserDetail(
    Guid Id,
    string Email,
    string Name,
    Guid RoleId,
    string Role,
    bool IsActive,
    DateTimeOffset CreatedAt,
    bool IsSelf,
    bool CanManage
);

internal static class Users
{
    public static IResult Conflict(string title) =>
        Results.Problem(title: title, statusCode: StatusCodes.Status409Conflict);

    public static IResult Forbidden(string title) =>
        Results.Problem(title: title, statusCode: StatusCodes.Status403Forbidden);

    public static IResult Problem(string field, string message) =>
        Results.ValidationProblem(new Dictionary<string, string[]> { [field] = [message] });

    /// <summary>A user with its role, tracked, for a change.</summary>
    public static Task<AppUser?> FindAsync(
        InventoryContext context,
        Guid id,
        CancellationToken cancellationToken
    ) =>
        context
            .AppUsers.Include(u => u.Role)
                .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    /// <summary>A role the manager may give, or the problem to answer with.</summary>
    public static async Task<(Role? Role, IResult? Problem)> AssignableAsync(
        InventoryContext context,
        Access manager,
        Guid roleId,
        CancellationToken cancellationToken
    )
    {
        var role = await context
            .Roles.Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

        if (role is null)
            return (null, Problem("roleId", "That role does not exist."));

        if (!manager.Covers(role))
            return (null, Problem("roleId", "You cannot give a role that grants more than yours."));

        return (role, null);
    }

    /// <summary>
    /// Whether taking System Admin away from <paramref name="user"/> — a demotion or a deactivation —
    /// would leave nobody active holding it. Takes no lock: two System Admins demoting each other at
    /// once can both pass.
    /// </summary>
    public static async Task<bool> IsLastSystemAdminAsync(
        InventoryContext context,
        AppUser user,
        CancellationToken cancellationToken
    ) =>
        user.IsActive
        && user.Role.IsSystemAdmin
        && !await context.AppUsers.AnyAsync(
            u => u.Id != user.Id && u.IsActive && u.Role.IsSystemAdmin,
            cancellationToken
        );

    public static async Task<UserDetail?> DetailAsync(
        InventoryContext context,
        Access reader,
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var user = await context
            .AppUsers.AsNoTracking()
            .Include(u => u.Role)
                .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        return user is null
            ? null
            : new UserDetail(
                user.Id,
                user.Email,
                user.Name,
                user.RoleId,
                user.Role.Name,
                user.IsActive,
                user.CreatedAt,
                user.Id == reader.UserId,
                reader.Covers(user.Role)
            );
    }
}
