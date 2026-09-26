using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Codaxy.Inventory.App.Shared.Validation;

/// <summary>
/// Minimal endpoints do not validate a body the way `[ApiController]` does, and one helper is
/// cheaper than a package.
/// </summary>
public static class MiniValidator
{
    public static bool IsValid<T>(T value, out IResult problem)
        where T : notnull
    {
        var errors = Errors(value);
        problem = errors.Count == 0 ? Results.Empty : Results.ValidationProblem(errors);
        return errors.Count == 0;
    }

    /// <summary>
    /// The attributes' messages by field, keyed as the JSON the client sent, so a message lands under
    /// the field it names; for an endpoint that adds rules of its own before answering.
    /// </summary>
    public static Dictionary<string, string[]> Errors<T>(T value)
        where T : notnull
    {
        List<ValidationResult> results = [];
        Validator.TryValidateObject(value, new ValidationContext(value), results, true);

        return results
            .GroupBy(r =>
                JsonNamingPolicy.CamelCase.ConvertName(r.MemberNames.FirstOrDefault() ?? "")
            )
            .ToDictionary(g => g.Key, g => g.Select(r => r.ErrorMessage ?? "Invalid.").ToArray());
    }
}
