using System.ComponentModel.DataAnnotations;

namespace Codaxy.Inventory.App.Shared.Icons;

/// <summary>
/// The icons a device type or a furniture type may carry, by key — what an employee's equipment shows
/// a thing as. None is the generic one, the menu's. The client names the same keys with their glyphs
/// and alone guesses one from a new type's name (`typeIcons.ts`); the migration that filled the first
/// ones guessed the same way, in SQL, once.
/// </summary>
public static class TypeIcons
{
    /// <summary>
    /// What a form's icon is when the request leaves it out — a client from before icons — so saving
    /// keeps the type's icon rather than clearing it; null is the generic one. No icon's key.
    /// </summary>
    public const string Unsent = "\u0000unsent";

    public static readonly IReadOnlyList<string> Device =
    [
        "laptop",
        "desktop",
        "parts",
        "monitor",
        "dock",
        "hub",
        "headset",
        "earbuds",
        "keyboard",
        "mouse",
        "webcam",
        "camera",
        "cctv",
        "phone",
        "tablet",
        "pen",
        "watch",
        "printer",
        "router",
        "wifi",
        "firewall",
        "server",
        "storage",
        "speaker",
        "microphone",
        "power",
        "console",
        "vr",
        "projector",
        "presenter",
        "backpack",
        "fridge",
        "air",
        "coffee",
    ];

    public static readonly IReadOnlyList<string> Furniture =
    [
        "armchair",
        "sofa",
        "desk",
        "table",
        "dresser",
        "drawers",
        "wardrobe",
        "shelf",
        "lamp",
        "whiteboard",
        "safe",
        "hanger",
    ];

    /// <summary>Whether <paramref name="icon"/> is none, one of <paramref name="set"/>, or left out.</summary>
    public static bool IsValid(string? icon, IReadOnlyList<string> set) =>
        icon is null || icon == Unsent || set.Contains(icon);
}

/// <summary>An icon key a device type may carry, or none.</summary>
public sealed class DeviceIconAttribute()
    : ValidationAttribute("That is not an icon a device type can have.")
{
    public override bool IsValid(object? value) =>
        TypeIcons.IsValid(value as string, TypeIcons.Device);
}

/// <summary>An icon key a furniture type may carry, or none.</summary>
public sealed class FurnitureIconAttribute()
    : ValidationAttribute("That is not an icon a furniture type can have.")
{
    public override bool IsValid(object? value) =>
        TypeIcons.IsValid(value as string, TypeIcons.Furniture);
}
