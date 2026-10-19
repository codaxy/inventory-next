using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PersonCanSignIn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "can_sign_in",
                table: "person",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            // Every person may sign in until someone decides otherwise; new rows take it from the
            // entity, so the column keeps no default the model does not have.
            migrationBuilder.Sql("alter table person alter column can_sign_in drop default;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "can_sign_in",
                table: "person");
        }
    }
}
