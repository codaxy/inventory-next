using System.ComponentModel.DataAnnotations;

namespace Codaxy.Inventory.Web.Auth;

/// <summary>
/// Minimal endpoints do not validate a body the way `[ApiController]` does, and one helper is
/// cheaper than a package.
/// </summary>
internal static class MiniValidator
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
                r => r.MemberNames.FirstOrDefault() ?? "",
                r => new[] { r.ErrorMessage ?? "Invalid." }
            )
        );
        return false;
    }
}
