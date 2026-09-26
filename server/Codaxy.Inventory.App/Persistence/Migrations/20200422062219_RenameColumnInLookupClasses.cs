using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class RenameColumnInLookupClasses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Description", table: "LicenseType");

            migrationBuilder.DropColumn(name: "Description", table: "LicenseExpirationModel");

            migrationBuilder.DropColumn(name: "Description", table: "LicenseClass");

            migrationBuilder.DropColumn(name: "Description", table: "LicenseCategory");

            migrationBuilder.RenameColumn(name: "Name", table: "VolumeType", newName: "Text");

            migrationBuilder.RenameColumn(name: "Value", table: "Period", newName: "Text");

            migrationBuilder.RenameColumn(name: "Name", table: "LicenseType", newName: "Text");

            migrationBuilder.RenameColumn(name: "Name", table: "LicenseModel", newName: "Text");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "LicenseExpirationModel",
                newName: "Text"
            );

            migrationBuilder.RenameColumn(name: "Name", table: "LicenseClass", newName: "Text");

            migrationBuilder.RenameColumn(name: "Name", table: "LicenseCategory", newName: "Text");

            migrationBuilder.RenameColumn(name: "Value", table: "Currency", newName: "Text");

            migrationBuilder.RenameColumn(name: "Name", table: "BusinessEntity", newName: "Text");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "Text", table: "VolumeType", newName: "Name");

            migrationBuilder.RenameColumn(name: "Text", table: "Period", newName: "Value");

            migrationBuilder.RenameColumn(name: "Text", table: "LicenseType", newName: "Name");

            migrationBuilder.RenameColumn(name: "Text", table: "LicenseModel", newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Text",
                table: "LicenseExpirationModel",
                newName: "Name"
            );

            migrationBuilder.RenameColumn(name: "Text", table: "LicenseClass", newName: "Name");

            migrationBuilder.RenameColumn(name: "Text", table: "LicenseCategory", newName: "Name");

            migrationBuilder.RenameColumn(name: "Text", table: "Currency", newName: "Value");

            migrationBuilder.RenameColumn(name: "Text", table: "BusinessEntity", newName: "Name");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "LicenseType",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "LicenseExpirationModel",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "LicenseClass",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "LicenseCategory",
                nullable: true
            );
        }
    }
}
