using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    /// <summary>
    /// The eight calendar-date columns become <c>date</c>, and the values are repaired as they
    /// convert. They were written as the instant of local midnight — the client sends midnight with
    /// its offset and the API applies it — so on a UTC server a date entered in Belgrade was stored
    /// as 22:00 or 23:00 the day before. Reading each value back in that zone recovers the day that
    /// was meant, leaves an unshifted midnight alone, and takes the local day of a row that carries
    /// a time of day.
    /// </summary>
    public partial class CalendarDatesToDate : Migration
    {
        private static readonly (string Table, string Column)[] Columns =
        [
            ("asset", "purchase_date"),
            ("activation", "activation_date"),
            ("activation", "deactivation_date"),
            ("electronic_device", "guarantee_expiration_date"),
            ("electronic_device", "manufacturing_date"),
            ("license", "subscription_expiration_date"),
            ("maintenance_contract", "expiration_date"),
            ("maintenance_contract", "service_due_date"),
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var (table, column) in Columns)
                migrationBuilder.Sql(
                    $"""
                    ALTER TABLE {table}
                        ALTER COLUMN {column} TYPE date
                        USING ({column} AT TIME ZONE 'UTC' AT TIME ZONE 'Europe/Belgrade')::date
                    """
                );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The time of day is gone; down restores the type at local midnight, not the values.
            foreach (var (table, column) in Columns)
                migrationBuilder.Sql(
                    $"""
                    ALTER TABLE {table}
                        ALTER COLUMN {column} TYPE timestamp without time zone
                        USING {column}::timestamp
                    """
                );
        }
    }
}
