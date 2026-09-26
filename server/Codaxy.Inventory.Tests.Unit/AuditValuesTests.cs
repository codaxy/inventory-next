using Codaxy.Inventory.App.Administration.AuditLogs;

namespace Codaxy.Inventory.Tests.Unit;

public class AuditValuesTests
{
    [Fact]
    public void An_update_changes_only_the_fields_whose_values_differ()
    {
        var old = AuditValues.Parse("""{ "Name": "A", "Floor": 1, "Note": null }""");
        var @new = AuditValues.Parse("""{ "Name": "B", "Floor": 1, "Note": "set" }""");

        Assert.Equal(["Name", "Note"], AuditValues.ChangedNames(old, @new));
    }

    [Fact]
    public void A_create_or_delete_lists_no_changed_names()
    {
        var values = AuditValues.Parse("""{ "Name": "A" }""");

        Assert.Empty(AuditValues.ChangedNames(AuditValues.Parse(null), values));
        Assert.Empty(AuditValues.ChangedNames(values, AuditValues.Parse("")));
    }

    [Fact]
    public void Compare_keeps_the_written_order_and_a_field_only_one_side_has()
    {
        var old = AuditValues.Parse("""{ "B": 1, "Gone": true }""");
        var @new = AuditValues.Parse("""{ "B": 2, "A": "x" }""");

        var fields = AuditValues.Compare(old, @new);

        Assert.Equal(["B", "A", "Gone"], fields.Select(f => f.Name));
        Assert.All(fields, f => Assert.True(f.Changed));
        Assert.Null(fields[2].New);
    }

    [Fact]
    public void A_created_field_left_empty_is_not_a_change()
    {
        var fields = AuditValues.Compare(
            AuditValues.Parse(null),
            AuditValues.Parse("""{ "Name": "A", "Note": null }""")
        );

        Assert.Equal([true, false], fields.Select(f => f.Changed));
    }

    [Theory]
    [InlineData("\"plain\"", ValueKind.Text)]
    [InlineData("12.50", ValueKind.Number)]
    [InlineData("true", ValueKind.Boolean)]
    [InlineData("\"2024-05-01\"", ValueKind.Date)]
    [InlineData("\"2024-05-01T10:00:00\"", ValueKind.DateTime)]
    [InlineData("\"2024-05-01T10:00:00.123+02:00\"", ValueKind.Instant)]
    [InlineData("\"2024-05-01T10:00:00Z\"", ValueKind.Instant)]
    public void Values_carry_the_kind_the_screen_formats_them_by(string json, ValueKind kind)
    {
        var values = AuditValues.Parse($$"""{ "V": {{json}} }""");

        Assert.Equal(kind, values.Get("V")!.Kind);
    }

    [Fact]
    public void A_document_that_is_not_an_object_reads_as_empty()
    {
        Assert.Same(AuditValues.Empty, AuditValues.Parse("[1, 2]"));
    }

    [Fact]
    public void Typed_reads_ignore_a_value_of_another_type()
    {
        var values = AuditValues.Parse("""{ "Name": 5, "InventoryNumber": "100001" }""");

        Assert.Null(values.GetString("Name"));
        Assert.Null(values.GetInt("InventoryNumber"));
        Assert.Null(values.GetInt("Missing"));
    }
}
