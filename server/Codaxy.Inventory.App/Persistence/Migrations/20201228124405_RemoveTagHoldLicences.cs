using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class RemoveTagHoldLicences : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "HoldLicences", table: "ElectronicDeviceTag");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HoldLicences",
                table: "ElectronicDeviceTag",
                nullable: false,
                defaultValue: false
            );
        }
    }
}
