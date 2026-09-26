using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddModelCodeAndModelName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Model",
                table: "ElectronicDevice",
                newName: "ModelName"
            );

            migrationBuilder.AddColumn<string>(
                name: "ModelCode",
                table: "ElectronicDevice",
                type: "text",
                nullable: true
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ModelCode", table: "ElectronicDevice");

            migrationBuilder.RenameColumn(
                name: "ModelName",
                table: "ElectronicDevice",
                newName: "Model"
            );
        }
    }
}
