using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryItemTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ItemTypeId",
                table: "InventoryItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InventoryItemTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<byte>(type: "tinyint", nullable: false),
                    CodeStart = table.Column<int>(type: "int", nullable: true),
                    CodeEnd = table.Column<int>(type: "int", nullable: true),
                    IsInventory = table.Column<bool>(type: "bit", nullable: false),
                    CanSell = table.Column<bool>(type: "bit", nullable: false),
                    CanPurchase = table.Column<bool>(type: "bit", nullable: false),
                    IsRecipe = table.Column<bool>(type: "bit", nullable: false),
                    IsProduction = table.Column<bool>(type: "bit", nullable: false),
                    AllowNegativeStock = table.Column<bool>(type: "bit", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItemTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_ItemTypeId",
                table: "InventoryItems",
                column: "ItemTypeId",
                filter: "[IsDeleted] = 0 AND [ItemTypeId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItemTypes_TenantId",
                table: "InventoryItemTypes",
                column: "TenantId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItemTypes_TenantId_Code",
                table: "InventoryItemTypes",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItemTypes_TenantId_NameAr",
                table: "InventoryItemTypes",
                columns: new[] { "TenantId", "NameAr" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItemTypes_TenantId_SortOrder",
                table: "InventoryItemTypes",
                columns: new[] { "TenantId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryItemTypes");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_ItemTypeId",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "ItemTypeId",
                table: "InventoryItems");
        }
    }
}
