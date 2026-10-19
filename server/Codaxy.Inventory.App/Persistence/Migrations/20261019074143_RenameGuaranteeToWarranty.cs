using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameGuaranteeToWarranty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "guarantee_number",
                table: "electronic_device",
                newName: "warranty_number");

            migrationBuilder.RenameColumn(
                name: "guarantee_expiration_date",
                table: "electronic_device",
                newName: "warranty_expiration_date");

            // The audit log names properties inside its JSON, and the viewer labels by the current model.
            migrationBuilder.Sql(
                @"update audit_log set
                    old_values_json = replace(replace(old_values_json, '""GuaranteeNumber"":', '""WarrantyNumber"":'), '""GuaranteeExpirationDate"":', '""WarrantyExpirationDate"":'),
                    new_values_json = replace(replace(new_values_json, '""GuaranteeNumber"":', '""WarrantyNumber"":'), '""GuaranteeExpirationDate"":', '""WarrantyExpirationDate"":')
                  where ""table"" = 'ElectronicDevice';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "warranty_number",
                table: "electronic_device",
                newName: "guarantee_number");

            migrationBuilder.RenameColumn(
                name: "warranty_expiration_date",
                table: "electronic_device",
                newName: "guarantee_expiration_date");

            // The audit log names properties inside its JSON, and the viewer labels by the current model.
            migrationBuilder.Sql(
                @"update audit_log set
                    old_values_json = replace(replace(old_values_json, '""WarrantyNumber"":', '""GuaranteeNumber"":'), '""WarrantyExpirationDate"":', '""GuaranteeExpirationDate"":'),
                    new_values_json = replace(replace(new_values_json, '""WarrantyNumber"":', '""GuaranteeNumber"":'), '""WarrantyExpirationDate"":', '""GuaranteeExpirationDate"":')
                  where ""table"" = 'ElectronicDevice';");
        }
    }
}
