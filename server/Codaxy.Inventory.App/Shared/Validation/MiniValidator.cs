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
        List<ValidationResult> results = [];

        if (Validator.TryValidateObject(value, new ValidationContext(value), results, true))
        {
            problem = Results.Empty;
            return true;
        }

        problem = Results.ValidationProblem(
            results.ToDictionary(
                // Keyed as the JSON the client sent, so a message lands under the field it names.
                r => JsonNamingPolicy.CamelCase.ConvertName(r.MemberNames.FirstOrDefault() ?? ""),
                r => new[] { r.ErrorMessage ?? "Invalid." }
            )
        );
        return false;
    }
}
