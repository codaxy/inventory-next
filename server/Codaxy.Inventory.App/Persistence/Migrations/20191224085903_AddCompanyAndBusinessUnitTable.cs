using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddCompanyAndBusinessUnitTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BusinessUnit",
                columns: table => new { Id = table.Column<Guid>(nullable: false) },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessUnit", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Company",
                columns: table => new { Id = table.Column<Guid>(nullable: false) },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company", x => x.Id);
                }
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "BusinessUnit");

            migrationBuilder.DropTable(name: "Company");
        }
    }
}
