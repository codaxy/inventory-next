using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddContactPersonIdField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ContactPerson", table: "Vendor");

            migrationBuilder.AddColumn<Guid>(
                name: "ContactPersonId",
                table: "Vendor",
                nullable: false,
                defaultValue: new Guid("017279e9-9209-4439-b0ab-40c98e12a204")
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

        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
