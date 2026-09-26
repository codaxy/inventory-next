using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    public partial class AddLocationLinkEventLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Address", table: "Location");

            migrationBuilder.DropColumn(name: "City", table: "Location");

            migrationBuilder.DropColumn(name: "Country", table: "Location");

            migrationBuilder.RenameColumn(name: "Floor", table: "Location", newName: "CountryCode");

            migrationBuilder.AlterColumn<string>(
                name: "Room",
                table: "Location",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "PostalCode",
                table: "Location",
                maxLength: 8,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Location",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Location",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "CountryCode",
                table: "Location",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true
            );

            migrationBuilder.AddColumn<Guid>(name: "CityId", table: "Location", nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "HouseNumber",
                table: "Location",
                nullable: false
            );

            migrationBuilder.AddColumn<Guid>(name: "StateId", table: "Location", nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "Location",
                maxLength: 50,
                nullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Level",
                table: "Integrity",
                maxLength: 15,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Integrity",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Level",
                table: "Importance",
                maxLength: 15,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Importance",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Level",
                table: "Confidentiality",
                maxLength: 15,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Confidentiality",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Level",
                table: "Availability",
                maxLength: 15,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Availability",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AssetType",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AssetCategory",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "URL",
                table: "Asset",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Asset",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Asset",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true
            );

            migrationBuilder.AddColumn<int>(
                name: "InventoryNumber",
                table: "Asset",
                nullable: false
            );

            migrationBuilder.CreateTable(
                name: "Country",
                columns: table => new
                {
                    Code = table.Column<string>(fixedLength: true, maxLength: 2, nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Country", x => x.Code);
                }
            );

            migrationBuilder.CreateTable(
                name: "EventCategory",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Category = table.Column<string>(maxLength: 30, nullable: true),
                    Description = table.Column<string>(maxLength: 300, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventCategory", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "LinkCategory",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Category = table.Column<string>(maxLength: 30, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkCategory", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "LinkStatus",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Status = table.Column<string>(maxLength: 30, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkStatus", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new { Id = table.Column<Guid>(nullable: false) },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "City",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: true),
                    CountryCode = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_City", x => x.Id);
                    table.ForeignKey(
                        name: "FK_City_Country_CountryCode",
                        column: x => x.CountryCode,
                        principalTable: "Country",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "State",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: true),
                    CountryCode = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_State", x => x.Id);
                    table.ForeignKey(
                        name: "FK_State_Country_CountryCode",
                        column: x => x.CountryCode,
                        principalTable: "Country",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "EventType",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Type = table.Column<string>(maxLength: 30, nullable: true),
                    Description = table.Column<string>(maxLength: 300, nullable: true),
                    EventCategoryId = table.Column<Guid>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventType_EventCategory_EventCategoryId",
                        column: x => x.EventCategoryId,
                        principalTable: "EventCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "LinkType",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    LinkName = table.Column<string>(maxLength: 30, nullable: true),
                    InverseLinkName = table.Column<string>(maxLength: 30, nullable: true),
                    Description = table.Column<string>(maxLength: 300, nullable: true),
                    LinkCategoryId = table.Column<Guid>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LinkType_LinkCategory_LinkCategoryId",
                        column: x => x.LinkCategoryId,
                        principalTable: "LinkCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "EventLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    EventData = table.Column<string>(nullable: true),
                    EventTypeId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    AssetId = table.Column<Guid>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventLog_Asset_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Asset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_EventLog_EventType_EventTypeId",
                        column: x => x.EventTypeId,
                        principalTable: "EventType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_EventLog_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "Link",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    LinkTypeId = table.Column<Guid>(nullable: false),
                    ParentAssetId = table.Column<Guid>(nullable: false),
                    ChildAssetId = table.Column<Guid>(nullable: false),
                    LinkStatusId = table.Column<Guid>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    ChangeDate = table.Column<DateTime>(nullable: false),
                    ChangedBy = table.Column<int>(nullable: false),
                    Description = table.Column<string>(maxLength: 300, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Link", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Link_Asset_ChildAssetId",
                        column: x => x.ChildAssetId,
                        principalTable: "Asset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Link_LinkStatus_LinkStatusId",
                        column: x => x.LinkStatusId,
                        principalTable: "LinkStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Link_LinkType_LinkTypeId",
                        column: x => x.LinkTypeId,
                        principalTable: "LinkType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Link_Asset_ParentAssetId",
                        column: x => x.ParentAssetId,
                        principalTable: "Asset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Location_CityId",
                table: "Location",
                column: "CityId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Location_CountryCode",
                table: "Location",
                column: "CountryCode"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Location_StateId",
                table: "Location",
                column: "StateId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_City_CountryCode",
                table: "City",
                column: "CountryCode"
            );

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_AssetId",
                table: "EventLog",
                column: "AssetId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_EventTypeId",
                table: "EventLog",
                column: "EventTypeId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_UserId",
                table: "EventLog",
                column: "UserId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_EventType_EventCategoryId",
                table: "EventType",
                column: "EventCategoryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Link_ChildAssetId",
                table: "Link",
                column: "ChildAssetId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Link_LinkStatusId",
                table: "Link",
                column: "LinkStatusId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Link_LinkTypeId",
                table: "Link",
                column: "LinkTypeId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Link_ParentAssetId",
                table: "Link",
                column: "ParentAssetId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_LinkType_LinkCategoryId",
                table: "LinkType",
                column: "LinkCategoryId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_State_CountryCode",
                table: "State",
                column: "CountryCode"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Location_City_CityId",
                table: "Location",
                column: "CityId",
                principalTable: "City",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Location_Country_CountryCode",
                table: "Location",
                column: "CountryCode",
                principalTable: "Country",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Location_State_StateId",
                table: "Location",
                column: "StateId",
                principalTable: "State",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Location_City_CityId", table: "Location");

            migrationBuilder.DropForeignKey(
                name: "FK_Location_Country_CountryCode",
                table: "Location"
            );

            migrationBuilder.DropForeignKey(name: "FK_Location_State_StateId", table: "Location");

            migrationBuilder.DropTable(name: "City");

            migrationBuilder.DropTable(name: "EventLog");

            migrationBuilder.DropTable(name: "Link");

            migrationBuilder.DropTable(name: "State");

            migrationBuilder.DropTable(name: "EventType");

            migrationBuilder.DropTable(name: "User");

            migrationBuilder.DropTable(name: "LinkStatus");

            migrationBuilder.DropTable(name: "LinkType");

            migrationBuilder.DropTable(name: "Country");

            migrationBuilder.DropTable(name: "EventCategory");

            migrationBuilder.DropTable(name: "LinkCategory");

            migrationBuilder.DropIndex(name: "IX_Location_CityId", table: "Location");

            migrationBuilder.DropIndex(name: "IX_Location_CountryCode", table: "Location");

            migrationBuilder.DropIndex(name: "IX_Location_StateId", table: "Location");

            migrationBuilder.DropColumn(name: "CityId", table: "Location");

            migrationBuilder.DropColumn(name: "HouseNumber", table: "Location");

            migrationBuilder.DropColumn(name: "StateId", table: "Location");

            migrationBuilder.DropColumn(name: "Street", table: "Location");

            migrationBuilder.DropColumn(name: "InventoryNumber", table: "Asset");

            migrationBuilder.RenameColumn(name: "CountryCode", table: "Location", newName: "Floor");

            migrationBuilder.AlterColumn<string>(
                name: "Room",
                table: "Location",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 30,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "PostalCode",
                table: "Location",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 8,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Location",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 50,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Location",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 300,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Floor",
                table: "Location",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true
            );

            migrationBuilder.AddColumn<string>(name: "Address", table: "Location", nullable: true);

            migrationBuilder.AddColumn<string>(name: "City", table: "Location", nullable: true);

            migrationBuilder.AddColumn<string>(name: "Country", table: "Location", nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Level",
                table: "Integrity",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 15,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Integrity",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 255,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Level",
                table: "Importance",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 15,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Importance",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 255,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Level",
                table: "Confidentiality",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 15,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Confidentiality",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 255,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Level",
                table: "Availability",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 15,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Availability",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 255,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AssetType",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 30,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AssetCategory",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 30,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "URL",
                table: "Asset",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 500,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Asset",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 30,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Asset",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 300,
                oldNullable: true
            );
        }
    }
}
