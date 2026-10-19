using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PersonEmailUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "email_key",
                table: "person",
                type: "text",
                nullable: true,
                computedColumnSql: "lower(trim(email))",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "ix_person_email_key",
                table: "person",
                column: "email_key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_person_email_key",
                table: "person");

            migrationBuilder.DropColumn(
                name: "email_key",
                table: "person");
        }
    }
}
