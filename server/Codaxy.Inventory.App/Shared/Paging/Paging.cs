using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Shared.Paging;

/// <summary>A validated request for one page: 1-based, and a size the server agreed to.</summary>
public readonly record struct PageWindow(int Page, int Size)
{
    public int Skip => (Page - 1) * Size;
}

/// <summary>One page of a list, and how many rows the whole filtered list holds.</summary>
public sealed record Page<T>(IReadOnlyList<T> Items, int Total);

/// <summary>
/// The paging convention every list follows: <c>page</c> from 1 and <c>pageSize</c> up to
/// <see cref="MaxPageSize"/> in the query, <c>{ items, total }</c> in the answer. A page past the last
/// is empty, not an error, so a list that shrank under a reader still answers.
/// </summary>
public static class Paging
{
    public const int DefaultPageSize = 25;
    public const int MaxPageSize = 100;

    /// <summary>The window asked for, or the validation problem to answer with instead.</summary>
    public static IResult? Read(int? page, int? pageSize, out PageWindow window)
    {
        window = new(page ?? 1, pageSize ?? DefaultPageSize);
        Dictionary<string, string[]> errors = [];

        if (window.Page < 1)
            errors["page"] = ["The page must be 1 or more."];

        if (window.Size is < 1 or > MaxPageSize)
            errors["pageSize"] = [$"The page size must be between 1 and {MaxPageSize}."];

        return errors.Count == 0 ? null : Results.ValidationProblem(errors);
    }

    /// <summary>
    /// Counts, then reads the window. <paramref name="ordered"/> must end in a unique key, or rows
    /// sharing a sort value can appear on two pages or none.
    /// </summary>
    public static async Task<Page<T>> ToPageAsync<T>(
        this IQueryable<T> ordered,
        PageWindow window,
        CancellationToken cancellationToken
    )
    {
        var total = await ordered.CountAsync(cancellationToken);

        var items =
            total <= window.Skip
                ? []
                : await ordered.Skip(window.Skip).Take(window.Size).ToListAsync(cancellationToken);

        return new(items, total);
    }
}
