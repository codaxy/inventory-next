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
    /// The addresses every alert goes to, in one email. None by default, and none sends nothing: who
    /// is told is each deployment's to choose, and a guess would mail someone at the first start.
    /// Stored trimmed, lower-case, once each.
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

    /// <summary>How long before the date the first notice goes out.</summary>
    public static readonly SettingDefinition<AlertLead> Lead = new(
        "alerts.lead",
        new(1, "months"),
        LeadProblem,
        Tidy
    );

    /// <summary>
    /// Up to two reminders after the first notice, each closer to the date than the one before — which
    /// <see cref="OrderProblem"/> holds them to, against the lead, as the group is saved. None by default.
    /// </summary>
    public static readonly SettingDefinition<AlertLead[]> Reminders = new(
        "alerts.reminders",
        [],
        reminders =>
            reminders.Length > 2 ? "At most two reminders."
            : reminders.Any(r => r is null) ? "Not a valid value."
            : reminders.Select(LeadProblem).FirstOrDefault(p => p is not null),
        reminders => [.. reminders.Select(r => r is null ? r! : Tidy(r))]
    );

    /// <summary>Why the reminders do not each come closer to the date than the lead and each other, or null.</summary>
    public static string? OrderProblem(AlertLead lead, IReadOnlyList<AlertLead> reminders) =>
        InOrder(lead, reminders).Count == reminders.Count
            ? null
            : "Each reminder comes closer to the date than the one before it.";

    /// <summary>
    /// The reminders that keep the order, the rest left out: a stored value edited by hand still sends
    /// nothing out of order.
    /// </summary>
    public static IReadOnlyList<AlertLead> InOrder(
        AlertLead lead,
        IReadOnlyList<AlertLead> reminders
    )
    {
        List<AlertLead> kept = [];
        var before = Days(lead);
        foreach (var reminder in reminders)
        {
            if (Days(reminder) >= before)
                break;
            kept.Add(reminder);
            before = Days(reminder);
        }
        return kept;
    }

    /// <summary>A lead's length in days, from a fixed day, so months compare with weeks and days.</summary>
    private static int Days(AlertLead lead) =>
        lead.Until(Reference).DayNumber - Reference.DayNumber;

    private static readonly DateOnly Reference = new(2001, 1, 1);

    private static string? LeadProblem(AlertLead lead) =>
        lead.Amount is < 1 or > 999 ? "From 1 to 999."
        : !AlertLead.Units.Contains(lead.Unit) ? "Days, weeks or months."
        : null;

    private static AlertLead Tidy(AlertLead lead) =>
        lead with
        {
            Unit = lead.Unit?.Trim().ToLowerInvariant() ?? "",
        };

    /// <summary>One <c>@</c>, nothing blank in the local part, a domain of dotted labels.</summary>
    [GeneratedRegex(@"^[^@\s]+@[a-z0-9]([a-z0-9-]*[a-z0-9])?(\.[a-z0-9]([a-z0-9-]*[a-z0-9])?)+$")]
    private static partial Regex Address();
}
