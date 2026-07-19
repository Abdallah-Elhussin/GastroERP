using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExpandWarehousesAndTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Warehouses_TenantId_Code",
                table: "Warehouses");

            migrationBuilder.AddColumn<bool>(
                name: "AllowAdjustment",
                table: "Warehouses",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowIssue",
                table: "Warehouses",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowNegativeStock",
                table: "Warehouses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowReceiving",
                table: "Warehouses",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowReservation",
                table: "Warehouses",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "Warehouses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPosWarehouse",
                table: "Warehouses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystem",
                table: "Warehouses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentWarehouseId",
                table: "Warehouses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseBins",
                table: "Warehouses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseTypeId",
                table: "Warehouses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WarehouseTypeDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_WarehouseTypeDefinitions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_ParentWarehouseId",
                table: "Warehouses",
                column: "ParentWarehouseId",
                filter: "[IsDeleted] = 0 AND [ParentWarehouseId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_TenantId_BranchId_Code",
                table: "Warehouses",
                columns: new[] { "TenantId", "BranchId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0 AND [Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_WarehouseTypeId",
                table: "Warehouses",
                column: "WarehouseTypeId",
                filter: "[IsDeleted] = 0 AND [WarehouseTypeId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseTypeDefinitions_TenantId",
                table: "WarehouseTypeDefinitions",
                column: "TenantId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseTypeDefinitions_TenantId_Code",
                table: "WarehouseTypeDefinitions",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WarehouseTypeDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_Warehouses_ParentWarehouseId",
                table: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_Warehouses_TenantId_BranchId_Code",
                table: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_Warehouses_WarehouseTypeId",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "AllowAdjustment",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "AllowIssue",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "AllowNegativeStock",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "AllowReceiving",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "AllowReservation",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "IsPosWarehouse",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "IsSystem",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "ParentWarehouseId",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "UseBins",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "WarehouseTypeId",
                table: "Warehouses");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_TenantId_Code",
                table: "Warehouses",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0 AND [Code] IS NOT NULL");
        }
    }
}
