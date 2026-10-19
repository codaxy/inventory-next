using System.Text.RegularExpressions;
using Codaxy.Inventory.App.Shared.Settings;

namespace Codaxy.Inventory.App.Shared.ExpiryAlerts;

/// <summary>How long before a date an alert goes out: a number of days, weeks or months.</summary>
public sealed record AlertLead(int Amount, string Unit)
{
    public static readonly string[] Units = ["days", "weeks", "months"];

    /// <summary>The last date the lead reaches from <paramref name="today"/>: due on or before it.</summary>
    public DateOnly Until(DateOnly today) =>
        Unit switch
        {
            "days" => today.AddDays(Amount),
            "weeks" => today.AddDays(7 * Amount),
            _ => today.AddMonths(Amount),
        };

    /// <summary>"2 months", "1 week".</summary>
    public override string ToString() => $"{Amount} {(Amount == 1 ? Unit[..^1] : Unit)}";
}

/// <summary>Who expiry alerts go to, and how early.</summary>
public static partial class ExpiryAlertSettings
{
    /// <summary>
    /// The addresses every alert goes to, in one email; none, nothing is sent. Stored trimmed,
    /// lower-case, once each.
    /// </summary>
    public static readonly SettingDefinition<string[]> Recipients = new(
        "alerts.recipients",
        [],
        addresses =>
            addresses.FirstOrDefault(a => !Address().IsMatch(a)) is { } wrong
                ? $"\"{wrong}\" is not an email address."
                : null,
        addresses =>
            addresses
                .Select(a => a.Trim().ToLowerInvariant())
                .Where(a => a != "")
                .Distinct()
                .ToArray()
    );

    /// <summary>How long before the date an alert goes out.</summary>
    public static readonly SettingDefinition<AlertLead> Lead = new(
        "alerts.lead",
        new(2, "months"),
        lead =>
            lead.Amount is < 1 or > 999 ? "From 1 to 999."
            : !AlertLead.Units.Contains(lead.Unit) ? "Days, weeks or months."
            : null,
        lead => lead with { Unit = lead.Unit?.Trim().ToLowerInvariant() ?? "" }
    );

    /// <summary>One <c>@</c>, nothing blank in the local part, a domain of dotted labels.</summary>
    [GeneratedRegex(@"^[^@\s]+@[a-z0-9]([a-z0-9-]*[a-z0-9])?(\.[a-z0-9]([a-z0-9-]*[a-z0-9])?)+$")]
    private static partial Regex Address();
}
