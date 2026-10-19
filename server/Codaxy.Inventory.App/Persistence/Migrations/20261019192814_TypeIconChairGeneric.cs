using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TypeIconChairGeneric : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // A chair is the generic icon now: no icon of its own.
            migrationBuilder.Sql("update furniture_type set icon = null where icon = 'chair';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "update furniture_type set icon = 'chair' where icon is null and name ~* 'stolica|chair';"
            );
        }
    }
}
