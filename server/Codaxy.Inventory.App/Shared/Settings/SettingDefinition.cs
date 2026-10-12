using System.Text.Json;

namespace Codaxy.Inventory.App.Shared.Settings;

/// <summary>
/// A setting an administrator changes in the application: its key, its default and what is wrong with
/// a value. Defined beside the code that reads it and listed in <see cref="SettingCatalogue"/>.
/// </summary>
public abstract class SettingDefinition(string key)
{
    protected static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <summary><c>group.name</c>: the group is the screen's card.</summary>
    public string Key { get; } = key;

    public string Group => Key[..Key.IndexOf('.')];

    /// <summary>The default, as stored: what a database without the setting is seeded with.</summary>
    public abstract string DefaultJson { get; }

    /// <summary>Why a stored value cannot be used, or null.</summary>
    public abstract string? StoredProblem(string json);

    /// <summary>
    /// A value from the screen, tidied as it is stored — trimmed, say — or why it may not be saved.
    /// </summary>
    public abstract (string? Json, string? Problem) Accept(JsonElement value);
}

/// <param name="tidy">What saving does to a value before it is checked: trimming, a canonical case.</param>
public sealed class SettingDefinition<T>(
    string key,
    T @default,
    Func<T, string?>? problem = null,
    Func<T, T>? tidy = null
) : SettingDefinition(key)
{
    public T Default { get; } = @default;

    public override string DefaultJson => Serialize(Default);

    /// <summary>What is wrong with the value, or null when it may be saved.</summary>
    public string? Problem(T value) => problem?.Invoke(value);

    public string Serialize(T value) => JsonSerializer.Serialize(value, Json);

    /// <summary>
    /// The stored value, or why it cannot be used: JSON that is not a <typeparamref name="T"/>, a
    /// null, or a value with a problem — none of which the screen saves, so an edit by hand.
    /// </summary>
    public (T Value, string? Problem) Read(string json)
    {
        T? value;
        try
        {
            value = JsonSerializer.Deserialize<T>(json, Json);
        }
        catch (JsonException e)
        {
            return (Default, e.Message);
        }

        if (value is null)
            return (Default, "It is null.");

        return Problem(value) is { } wrong ? (Default, wrong) : (value, null);
    }

    public override string? StoredProblem(string json) => Read(json).Problem;

    public override (string? Json, string? Problem) Accept(JsonElement value)
    {
        T? parsed;
        try
        {
            parsed = value.Deserialize<T>(Json);
        }
        catch (JsonException)
        {
            return (null, "Not a valid value.");
        }

        if (parsed is null)
            return (null, "Required.");

        if (tidy is not null)
            parsed = tidy(parsed);

        return Problem(parsed) is { } wrong ? (null, wrong) : (Serialize(parsed), null);
    }
}
