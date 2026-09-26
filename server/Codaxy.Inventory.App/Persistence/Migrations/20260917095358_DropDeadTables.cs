using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DropDeadTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "BusinessUnit");

            migrationBuilder.DropTable(name: "CommunicationEquipment");

            migrationBuilder.DropTable(name: "Company");

            migrationBuilder.DropTable(name: "ComputerEquipment");

            migrationBuilder.DropTable(name: "DataStorage");

            migrationBuilder.DropTable(name: "EventLog");

            migrationBuilder.DropTable(name: "Link");

            migrationBuilder.DropTable(name: "MobileDevice");

            migrationBuilder.DropTable(name: "Review");

            migrationBuilder.DropTable(name: "ReviewLog");

            migrationBuilder.DropTable(name: "TechnicalEquipment");

            migrationBuilder.DropTable(name: "CommunicationEquipmentCategory");

            migrationBuilder.DropTable(name: "CommunicationEquipmentClass");

            migrationBuilder.DropTable(name: "ComputerEquipmentCategory");

            migrationBuilder.DropTable(name: "DataStorageCategory");

            migrationBuilder.DropTable(name: "EventType");

            migrationBuilder.DropTable(name: "User");

            migrationBuilder.DropTable(name: "LinkStatus");

            migrationBuilder.DropTable(name: "LinkType");

            migrationBuilder.DropTable(name: "MobileDeviceCategory");

            migrationBuilder.DropTable(name: "ReviewStatus");

            migrationBuilder.DropTable(name: "TechnicalEquipmentCategory");

            migrationBuilder.DropTable(name: "EventCategory");

            migrationBuilder.DropTable(name: "LinkCategory");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Deliberately not reversible. Recreating these 24 tables would give back empty
            // shells: the ~101 seeded codebook rows they held are gone, and no migration can
            // put them back. Doing nothing instead would leave the history claiming a revert
            // that did not happen, so this fails loudly and rolling back means restoring a
            // backup.
            throw new NotSupportedException(
                "DropDeadTables cannot be reverted. Restore a backup taken before it ran."
            );
        }
    }
}
