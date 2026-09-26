using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Shared.Validation;

/// <summary>
/// A value no other record holds, whatever its case, both sides trimmed — the original saved values
/// with stray spaces. Checked here, not by an index: the schema is frozen, so two saves at once can
/// still both pass.
/// </summary>
public static class Unique
{
    /// <summary>The validation problem when <paramref name="others"/> already hold the value, or none.</summary>
    public static async Task<IResult?> CheckAsync<T>(
        IQueryable<T> others,
        Expression<Func<T, string>> of,
        string value,
        string field,
        string message,
        CancellationToken cancellationToken
    )
    {
        var lower = value.Trim().ToLowerInvariant();
        var taken = await others
            .Select(of)
            .AnyAsync(v => v.Trim().ToLower() == lower, cancellationToken);

        return taken
            ? Results.ValidationProblem(new Dictionary<string, string[]> { [field] = [message] })
            : null;
    }
}
