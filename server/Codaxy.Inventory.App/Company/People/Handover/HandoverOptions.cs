namespace Codaxy.Inventory.App.Company.People.Handover;

public sealed class HandoverOptions
{
    public const string Section = "Handover";

    /// <summary>
    /// The place the sheet is signed at, printed beside "Mjesto". Each deployment's, since offices
    /// differ; empty, the line is left for a hand.
    /// </summary>
    public string Place { get; set; } = "";
}
