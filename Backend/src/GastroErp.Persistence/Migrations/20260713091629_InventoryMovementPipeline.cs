using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InventoryMovementPipeline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AdjIncreasesOnHand",
                table: "StockMovements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte>(
                name: "MovementType",
                table: "StockMovements",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "StockMovements",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            // Backfill positive Quantity + MovementType from legacy signed QuantityChange.
            // IN=1, OUT=2 (best-effort for pre-pipeline rows).
            migrationBuilder.Sql("""
                UPDATE StockMovements
                SET
                    Quantity = ABS(QuantityChange),
                    MovementType = CASE WHEN QuantityChange >= 0 THEN CAST(1 AS tinyint) ELSE CAST(2 AS tinyint) END,
                    AdjIncreasesOnHand = CASE WHEN QuantityChange >= 0 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END
                WHERE Quantity = 0 AND MovementType = 0;
                """);

            migrationBuilder.CreateTable(
                name: "GoodsIssues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssueNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IssueDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_GoodsIssues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryBalances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QtyOnHand = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReservedQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AvgCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_InventoryBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryBalances_InventoryItems_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryBalances_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Seed balances from historical ledger (best-effort AvgCost = last inbound unit cost).
            migrationBuilder.Sql("""
                INSERT INTO InventoryBalances (
                    Id, TenantId, InventoryItemId, WarehouseId,
                    QtyOnHand, ReservedQty, AvgCost,
                    CreatedAt, CreatedBy, UpdatedAt, UpdatedBy,
                    IsDeleted, DeletedAt, DeletedBy)
                SELECT
                    NEWID(),
                    m.TenantId,
                    m.InventoryItemId,
                    m.WarehouseId,
                    SUM(m.QuantityChange),
                    0,
                    ISNULL((
                        SELECT TOP 1 m2.UnitCost
                        FROM StockMovements m2
                        WHERE m2.TenantId = m.TenantId
                          AND m2.InventoryItemId = m.InventoryItemId
                          AND m2.WarehouseId = m.WarehouseId
                          AND m2.QuantityChange > 0
                        ORDER BY m2.CreatedAt DESC
                    ), 0),
                    SYSUTCDATETIME(),
                    N'migration',
                    NULL,
                    NULL,
                    0,
                    NULL,
                    NULL
                FROM StockMovements m
                GROUP BY m.TenantId, m.InventoryItemId, m.WarehouseId;
                """);

            migrationBuilder.CreateTable(
                name: "OpeningBalances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DocumentDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    IsPosted = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_OpeningBalances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GoodsIssueLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoodsIssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_GoodsIssueLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoodsIssueLines_GoodsIssues_GoodsIssueId",
                        column: x => x.GoodsIssueId,
                        principalTable: "GoodsIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpeningBalanceLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpeningBalanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_OpeningBalanceLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpeningBalanceLines_OpeningBalances_OpeningBalanceId",
                        column: x => x.OpeningBalanceId,
                        principalTable: "OpeningBalances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_TenantId_TransactionType_ReferenceDocumentId",
                table: "InventoryTransactions",
                columns: new[] { "TenantId", "TransactionType", "ReferenceDocumentId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsIssueLines_GoodsIssueId",
                table: "GoodsIssueLines",
                column: "GoodsIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsIssues_TenantId_IssueNumber",
                table: "GoodsIssues",
                columns: new[] { "TenantId", "IssueNumber" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryBalances_InventoryItemId",
                table: "InventoryBalances",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryBalances_TenantId_InventoryItemId_WarehouseId",
                table: "InventoryBalances",
                columns: new[] { "TenantId", "InventoryItemId", "WarehouseId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryBalances_WarehouseId",
                table: "InventoryBalances",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_OpeningBalanceLines_OpeningBalanceId",
                table: "OpeningBalanceLines",
                column: "OpeningBalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_OpeningBalances_TenantId_DocumentNumber",
                table: "OpeningBalances",
                columns: new[] { "TenantId", "DocumentNumber" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoodsIssueLines");

            migrationBuilder.DropTable(
                name: "InventoryBalances");

            migrationBuilder.DropTable(
                name: "OpeningBalanceLines");

            migrationBuilder.DropTable(
                name: "GoodsIssues");

            migrationBuilder.DropTable(
                name: "OpeningBalances");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactions_TenantId_TransactionType_ReferenceDocumentId",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "AdjIncreasesOnHand",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "MovementType",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "StockMovements");
        }
    }
}
