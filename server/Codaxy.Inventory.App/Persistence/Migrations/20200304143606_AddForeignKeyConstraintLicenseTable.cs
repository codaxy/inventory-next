using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddForeignKeyConstraintLicenseTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LicenseModelId",
                table: "License",
                nullable: false,
                defaultValue: new Guid("70d21475-8f4e-41cb-9db5-287be132e323")
            );

            migrationBuilder.CreateIndex(
                name: "IX_License_LicenseModelId",
                table: "License",
                column: "LicenseModelId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_License_LicenseModel_LicenseModelId",
                table: "License",
                column: "LicenseModelId",
                principalTable: "LicenseModel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_License_LicenseModel_LicenseModelId",
                table: "License"
            );

            migrationBuilder.DropIndex(name: "IX_License_LicenseModelId", table: "License");

            migrationBuilder.DropColumn(name: "LicenseModelId", table: "License");
        }
    }
}
