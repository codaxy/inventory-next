using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class CopyVATNumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update \"Vendor\" set \"VATNumber2\" = \"VATNumber\"::text");
        }

        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
