namespace Codaxy.Inventory.App.Shared.Assets;

public static class AssetNames
{
    /// <summary>"Apple developer program #100684": the name, and the inventory number as the screens show it.</summary>
    public static string WithNumber(string name, int? number) =>
        number is null ? name : $"{name} #{number}";
}
