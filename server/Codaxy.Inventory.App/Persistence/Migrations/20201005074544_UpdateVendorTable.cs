using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class UpdateVendorTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vendor_Person_ContactPersonId",
                table: "Vendor"
            );

            migrationBuilder.DropIndex(name: "IX_Vendor_ContactPersonId", table: "Vendor");

            migrationBuilder.DropColumn(name: "ContactPersonId", table: "Vendor");

            migrationBuilder.AddColumn<string>(
                name: "ContactPerson",
                table: "Vendor",
                nullable: true
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ContactPerson", table: "Vendor");

            migrationBuilder.AddColumn<Guid>(
                name: "ContactPersonId",
                table: "Vendor",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.CreateIndex(
                name: "IX_Vendor_ContactPersonId",
                table: "Vendor",
                column: "ContactPersonId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Vendor_Person_ContactPersonId",
                table: "Vendor",
                column: "ContactPersonId",
                principalTable: "Person",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
