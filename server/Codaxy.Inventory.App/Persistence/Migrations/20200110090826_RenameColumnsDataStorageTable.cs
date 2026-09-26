using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class RenameColumnsDataStorageTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PersonalData",
                table: "DataStorage",
                newName: "IsPersonalData"
            );

            migrationBuilder.RenameColumn(
                name: "Encryption",
                table: "DataStorage",
                newName: "IsEncrypted"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsPersonalData",
                table: "DataStorage",
                newName: "PersonalData"
            );

            migrationBuilder.RenameColumn(
                name: "IsEncrypted",
                table: "DataStorage",
                newName: "Encryption"
            );
        }
    }
}
