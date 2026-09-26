using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class SetLastModifiedDateNotNull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModified",
                table: "Asset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModified",
                table: "Asset",
                nullable: true,
                oldClrType: typeof(DateTime)
            );
        }
    }
}
