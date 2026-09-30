namespace Codaxy.Inventory.App.Shared.Assets;

/// <summary>An asset to pick, its name and inventory number apart: beside a name the screens mute the number.</summary>
public sealed record NumberedOption(Guid Id, string Name, int? Number);
