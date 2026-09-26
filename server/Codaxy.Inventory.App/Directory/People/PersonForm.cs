using System.ComponentModel.DataAnnotations;
using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Directory.People;

/// <summary>What creating and editing a person take.</summary>
public sealed record PersonForm(
    [property:
        Required(ErrorMessage = "Give the person a name."),
        StringLength(200, ErrorMessage = "A name is at most 200 characters.")
    ]
        string? Name,
    [property:
        Required(ErrorMessage = "Give the person an email address."),
        StringLength(200, ErrorMessage = "An email address is at most 200 characters."),
        EmailAddress(ErrorMessage = "That is not an email address.")
    ]
        string? Email
);

public sealed record PersonDetail(Guid Id, string Name, string Email);

internal static class People
{
    /// <summary>
    /// No other person holds the name or the email, whatever the case — the original refused a
    /// duplicate name. Both sides trimmed: stored values the original wrote can carry spaces. Checked
    /// here, not by an index: the schema is frozen, so two saves at once can still both pass.
    /// </summary>
    public static async Task<IResult?> CheckUniqueAsync(
        InventoryContext context,
        PersonForm form,
        Guid? except,
        CancellationToken cancellationToken
    )
    {
        var name = form.Name!.Trim().ToLowerInvariant();
        var email = form.Email!.Trim().ToLowerInvariant();
        var others = context.Persons.Where(p => p.Id != except);

        var errors = new Dictionary<string, string[]>();
        if (await others.AnyAsync(p => p.Name.Trim().ToLower() == name, cancellationToken))
            errors["name"] = ["A person with this name already exists."];
        if (await others.AnyAsync(p => p.Email.Trim().ToLower() == email, cancellationToken))
            errors["email"] = ["A person with this email address already exists."];

        return errors.Count > 0 ? Results.ValidationProblem(errors) : null;
    }

    public static void Apply(Person person, PersonForm form)
    {
        person.Name = form.Name!.Trim();
        person.Email = form.Email!.Trim();
    }

    public static Task<PersonDetail?> DetailAsync(
        InventoryContext context,
        Guid id,
        CancellationToken cancellationToken
    ) =>
        context
            .Persons.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PersonDetail(p.Id, p.Name, p.Email))
            .FirstOrDefaultAsync(cancellationToken);
}
