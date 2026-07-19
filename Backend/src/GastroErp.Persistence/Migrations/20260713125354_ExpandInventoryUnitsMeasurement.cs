using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExpandInventoryUnitsMeasurement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "Classification",
                table: "InventoryUnits",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)6);

            migrationBuilder.AddColumn<decimal>(
                name: "ConversionFactor",
                table: "InventoryUnits",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "InventoryUnits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte>(
                name: "UnitType",
                table: "InventoryUnits",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)1);

            migrationBuilder.Sql("""
                UPDATE InventoryUnits SET ConversionFactor = 1 WHERE ConversionFactor <= 0;
                UPDATE InventoryUnits SET UnitType = 1 WHERE UnitType = 0;
                UPDATE InventoryUnits SET Classification = 6 WHERE Classification = 0;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryUnits_TenantId_SortOrder",
                table: "InventoryUnits",
                columns: new[] { "TenantId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventoryUnits_TenantId_SortOrder",
                table: "InventoryUnits");

            migrationBuilder.DropColumn(
                name: "Classification",
                table: "InventoryUnits");

            migrationBuilder.DropColumn(
                name: "ConversionFactor",
                table: "InventoryUnits");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "InventoryUnits");

            migrationBuilder.DropColumn(
                name: "UnitType",
                table: "InventoryUnits");
        }
    }
}
