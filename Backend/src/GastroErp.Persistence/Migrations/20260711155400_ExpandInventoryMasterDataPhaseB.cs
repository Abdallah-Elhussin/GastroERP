using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExpandInventoryMasterDataPhaseB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Warehouses",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowInventoryCount",
                table: "Warehouses",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowManufacturing",
                table: "Warehouses",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowPurchase",
                table: "Warehouses",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowSales",
                table: "Warehouses",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowTransfer",
                table: "Warehouses",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "Warehouses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Warehouses",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ManagerUserId",
                table: "Warehouses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Warehouses",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Warehouses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ResponsibleEmployeeId",
                table: "Warehouses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "WarehouseType",
                table: "Warehouses",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)1);

            migrationBuilder.AlterColumn<string>(
                name: "SymbolAr",
                table: "InventoryUnits",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BaseUnitId",
                table: "InventoryUnits",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "InventoryUnits",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE InventoryUnits
                SET Code = UPPER(LEFT(Symbol, 50))
                WHERE Code IS NULL OR Code = '';
                """);

            migrationBuilder.AddColumn<byte>(
                name: "DecimalPlaces",
                table: "InventoryUnits",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)2);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "InventoryCategories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE InventoryCategories
                SET Code = 'CAT-' + UPPER(LEFT(CONVERT(varchar(36), Id), 8))
                WHERE Code IS NULL OR Code = '';
                """);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "InventoryCategories",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "InventoryCategories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "InventoryCategories",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "InventoryCategories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_TenantId_BranchId",
                table: "Warehouses",
                columns: new[] { "TenantId", "BranchId" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_TenantId_CompanyId",
                table: "Warehouses",
                columns: new[] { "TenantId", "CompanyId" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryUnits_BaseUnitId",
                table: "InventoryUnits",
                column: "BaseUnitId",
                filter: "[IsDeleted] = 0 AND [BaseUnitId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryUnits_TenantId_Code",
                table: "InventoryUnits",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCategories_TenantId_Code",
                table: "InventoryCategories",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCategories_TenantId_SortOrder",
                table: "InventoryCategories",
                columns: new[] { "TenantId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Warehouses_TenantId_BranchId",
                table: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_Warehouses_TenantId_CompanyId",
                table: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_InventoryUnits_BaseUnitId",
                table: "InventoryUnits");

            migrationBuilder.DropIndex(
                name: "IX_InventoryUnits_TenantId_Code",
                table: "InventoryUnits");

            migrationBuilder.DropIndex(
                name: "IX_InventoryCategories_TenantId_Code",
                table: "InventoryCategories");

            migrationBuilder.DropIndex(
                name: "IX_InventoryCategories_TenantId_SortOrder",
                table: "InventoryCategories");

            migrationBuilder.DropColumn(
                name: "AllowInventoryCount",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "AllowManufacturing",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "AllowPurchase",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "AllowSales",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "AllowTransfer",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "ManagerUserId",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "ResponsibleEmployeeId",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "WarehouseType",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "BaseUnitId",
                table: "InventoryUnits");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "InventoryUnits");

            migrationBuilder.DropColumn(
                name: "DecimalPlaces",
                table: "InventoryUnits");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "InventoryCategories");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "InventoryCategories");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "InventoryCategories");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "InventoryCategories");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "InventoryCategories");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Warehouses",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SymbolAr",
                table: "InventoryUnits",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);
        }
    }
}
