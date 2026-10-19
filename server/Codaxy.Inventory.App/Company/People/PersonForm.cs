using System.ComponentModel.DataAnnotations;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Access;
using Codaxy.Inventory.App.Shared.Settings;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Company.People;

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
        string? Email,
    bool CanSignIn = true
);

/// <param name="SignInOpen">Whether employees may sign in at all: the page shows the flag by it.</param>
public sealed record PersonDetail(
    Guid Id,
    string Name,
    string Email,
    bool CanSignIn,
    bool SignInOpen
);

internal static class People
{
    /// <summary>
    /// No other person holds the name or the email, whatever the case. Both sides trimmed: older rows
    /// carry stray spaces. Checked here so the form says which field: the email is also held unique by
    /// an index on its key, the name by nothing yet, so two saves at once can still share a name.
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
        if (await others.AnyAsync(p => p.EmailKey == email, cancellationToken))
            errors["email"] = ["A person with this email address already exists."];

        return errors.Count > 0 ? Results.ValidationProblem(errors) : null;
    }

    public static void Apply(Person person, PersonForm form)
    {
        person.Name = form.Name!.Trim();
        person.Email = form.Email!.Trim();
        person.CanSignIn = form.CanSignIn;
    }

    public static async Task<PersonDetail?> DetailAsync(
        InventoryContext context,
        SettingStore settings,
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var open = (await settings.LoadAsync(cancellationToken)).Get(SignInSettings.People);
        return await context
            .Persons.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PersonDetail(p.Id, p.Name, p.Email, p.CanSignIn, open))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
