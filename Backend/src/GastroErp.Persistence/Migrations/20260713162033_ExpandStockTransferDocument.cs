using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExpandStockTransferDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockTransfers_TenantId",
                table: "StockTransfers");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "StockTransfers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "TransferType",
                table: "StockTransfers",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)1);

            migrationBuilder.AlterColumn<string>(
                name: "BatchNumber",
                table: "StockTransferLines",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReceivedQuantity",
                table: "StockTransferLines",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCost",
                table: "StockTransferLines",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql("""
                UPDATE StockTransfers SET TransferType = 1 WHERE TransferType = 0;
                UPDATE l
                SET l.ReceivedQuantity = l.Quantity
                FROM StockTransferLines l
                INNER JOIN StockTransfers t ON t.Id = l.StockTransferId
                WHERE t.Status = 3;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfers_TenantId_Status",
                table: "StockTransfers",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfers_TenantId_TransferNumber",
                table: "StockTransfers",
                columns: new[] { "TenantId", "TransferNumber" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockTransfers_TenantId_Status",
                table: "StockTransfers");

            migrationBuilder.DropIndex(
                name: "IX_StockTransfers_TenantId_TransferNumber",
                table: "StockTransfers");

            migrationBuilder.DropColumn(
                name: "TransferType",
                table: "StockTransfers");

            migrationBuilder.DropColumn(
                name: "ReceivedQuantity",
                table: "StockTransferLines");

            migrationBuilder.DropColumn(
                name: "UnitCost",
                table: "StockTransferLines");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "StockTransfers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BatchNumber",
                table: "StockTransferLines",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfers_TenantId",
                table: "StockTransfers",
                column: "TenantId",
                filter: "[IsDeleted] = 0");
        }
    }
}
