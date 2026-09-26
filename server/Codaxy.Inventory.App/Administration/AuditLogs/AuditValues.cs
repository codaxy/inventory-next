using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Codaxy.Inventory.App.Administration.AuditLogs;

/// <summary>How a value should be shown; the server knows the JSON token, the client the viewer.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ValueKind>))]
public enum ValueKind
{
    Text,
    Number,
    Boolean,

    /// <summary>A moment with an offset: shown in the viewer's timezone.</summary>
    Instant,

    /// <summary>A date and time with no offset, as older rows wrote them: shown as written.</summary>
    DateTime,
    Date,
}

public sealed record FieldValue(string Text, ValueKind Kind);

/// <summary>One property of a logged entity, before and after.</summary>
public sealed record FieldChange(string Name, FieldValue? Old, FieldValue? New, bool Changed);

/// <summary>
/// A logged row's values: the complete property set the interceptor wrote as JSON, keys in the order
/// written. Every row carries every property, so what changed is only known by comparing the two.
/// </summary>
public sealed partial class AuditValues
{
    private readonly Dictionary<string, JsonElement> values;

    public IReadOnlyList<string> Names { get; }

    private AuditValues(Dictionary<string, JsonElement> values, IReadOnlyList<string> names)
    {
        this.values = values;
        Names = names;
    }

    public static readonly AuditValues Empty = new([], []);

    /// <summary>A missing document — the old side of a create, the new side of a delete — is empty.</summary>
    public static AuditValues Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Empty;

        using var document = JsonDocument.Parse(json);

        if (document.RootElement.ValueKind != JsonValueKind.Object)
            return Empty;

        Dictionary<string, JsonElement> values = [];
        List<string> names = [];

        foreach (var property in document.RootElement.EnumerateObject())
        {
            values[property.Name] = property.Value.Clone();
            names.Add(property.Name);
        }

        return new(values, names);
    }

    public bool Has(string name) => values.ContainsKey(name);

    public string? GetString(string name) =>
        values.TryGetValue(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    public int? GetInt(string name) =>
        values.TryGetValue(name, out var value)
        && value.ValueKind == JsonValueKind.Number
        && value.TryGetInt32(out var number)
            ? number
            : null;

    /// <summary>Null for a JSON null as for a missing key: the screen shows both as empty.</summary>
    public FieldValue? Get(string name) =>
        values.TryGetValue(name, out var value) ? Describe(value) : null;

    private string? Raw(string name) =>
        values.TryGetValue(name, out var value) && value.ValueKind != JsonValueKind.Null
            ? value.GetRawText()
            : null;

    /// <summary>
    /// Every property of either side, in the order written. A create or a delete has one side, and
    /// every property with a value on it counts as changed.
    /// </summary>
    public static IReadOnlyList<FieldChange> Compare(AuditValues old, AuditValues @new) =>
        [
            .. @new
                .Names.Concat(old.Names.Where(n => !@new.Has(n)))
                .Select(name => new FieldChange(
                    name,
                    old.Get(name),
                    @new.Get(name),
                    old.Raw(name) != @new.Raw(name)
                )),
        ];

    /// <summary>The properties an update changed; empty for a create or a delete, which change all.</summary>
    public static IReadOnlyList<string> ChangedNames(AuditValues old, AuditValues @new) =>
        old == Empty || @new == Empty
            ? []
            : [.. Compare(old, @new).Where(c => c.Changed).Select(c => c.Name)];

    private static FieldValue? Describe(JsonElement value) =>
        value.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.True => new("true", ValueKind.Boolean),
            JsonValueKind.False => new("false", ValueKind.Boolean),
            JsonValueKind.Number => new(value.GetRawText(), ValueKind.Number),
            JsonValueKind.String => DescribeString(value.GetString()!),
            _ => new(value.GetRawText(), ValueKind.Text),
        };

    private static FieldValue DescribeString(string text)
    {
        if (DatePattern().IsMatch(text))
            return new(text, ValueKind.Date);

        if (DateTimePattern().Match(text) is { Success: true } match)
            return new(
                text,
                match.Groups["offset"].Success
                && DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, out _)
                    ? ValueKind.Instant
                    : ValueKind.DateTime
            );

        return new(text, ValueKind.Text);
    }

    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}$")]
    private static partial Regex DatePattern();

    [GeneratedRegex(
        @"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}(:\d{2}(\.\d+)?)?(?<offset>Z|[+-]\d{2}:\d{2})?$"
    )]
    private static partial Regex DateTimePattern();
}
