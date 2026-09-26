using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class UpdateVatnumberTypeToStringInVendorTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "VATNumber",
                table: "Vendor",
                nullable: true,
                oldClrType: typeof(decimal)
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "VATNumber",
                table: "Vendor",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true
            );
        }
    }
}
