using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddColumnHoldLicences : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HoldLicences",
                table: "ElectronicDeviceType",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<bool>(
                name: "HoldLicences",
                table: "ElectronicDeviceTag",
                nullable: false,
                defaultValue: false
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "HoldLicences", table: "ElectronicDeviceType");

            migrationBuilder.DropColumn(name: "HoldLicences", table: "ElectronicDeviceTag");
        }
    }
}
