using System.Text.RegularExpressions;

namespace Codaxy.Inventory.App.Shared.Export;

/// <summary>
/// The zone an export's instants are written in, and what its headers call it. The request names
/// both: <c>tz</c>, the browser's IANA name, converts; <c>tzLabel</c>, its abbreviations ("CET/CEST"),
/// is only printed — .NET on Linux knows long names only.
/// </summary>
public sealed partial record ExportZone(TimeZoneInfo Time, string Label)
{
    public const string ZoneParameter = "tz";
    public const string LabelParameter = "tzLabel";

    public static readonly ExportZone Utc = new(TimeZoneInfo.Utc, "UTC");

    /// <summary>
    /// The zone a request names, labelled as it says or else by the zone's offsets in
    /// <paramref name="year"/>; UTC when it names none. The problem when either is unusable.
    /// </summary>
    public static IResult? Read(string? tz, string? label, int year, out ExportZone zone)
    {
        zone = Utc;
        var errors = new Dictionary<string, string[]>();

        TimeZoneInfo? time = TimeZoneInfo.Utc;
        if (!string.IsNullOrEmpty(tz) && !TimeZoneInfo.TryFindSystemTimeZoneById(tz, out time))
            errors[ZoneParameter] = [$"\"{tz}\" is not a time zone this server knows."];
        // It is written into the file, so it is held to what an abbreviation or an offset needs.
        if (!string.IsNullOrEmpty(label) && !Abbreviation().IsMatch(label))
            errors[LabelParameter] =
            [
                "A time zone label is at most 24 letters, digits and + - : /, as in CET/CEST.",
            ];
        if (errors.Count > 0)
            return Results.ValidationProblem(errors);

        if (time is not null && time != TimeZoneInfo.Utc)
            zone = new(time, string.IsNullOrEmpty(label) ? Offsets(time, year) : label);
        else if (!string.IsNullOrEmpty(label))
            zone = Utc with { Label = label };
        return null;
    }

    /// <summary>
    /// The zone's offsets in mid-January and mid-July, ordered, one when they agree: "GMT+1/GMT+2",
    /// "GMT+5:30". Both halves of the year, since one offset is wrong for every row on the other side
    /// of a clock change.
    /// </summary>
    public static string Offsets(TimeZoneInfo time, int year) =>
        string.Join(
            "/",
            new[] { new DateTime(year, 1, 15, 12, 0, 0), new DateTime(year, 7, 15, 12, 0, 0) }
                .Select(day => time.GetUtcOffset(DateTime.SpecifyKind(day, DateTimeKind.Utc)))
                .Distinct()
                .Order()
                .Select(Gmt)
        );

    private static string Gmt(TimeSpan offset) =>
        offset == TimeSpan.Zero
            ? "GMT"
            : $"GMT{(offset < TimeSpan.Zero ? "-" : "+")}{Math.Abs(offset.Hours)}"
                + (offset.Minutes == 0 ? "" : $":{Math.Abs(offset.Minutes):00}");

    [GeneratedRegex(@"^[A-Za-z0-9+:/-]{1,24}$")]
    private static partial Regex Abbreviation();
}
