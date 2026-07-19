using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExpandOpeningBalanceDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "WarehouseId",
                table: "OpeningBalances",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ApprovalDate",
                table: "OpeningBalances",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ContraAccountId",
                table: "OpeningBalances",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CostCenterId",
                table: "OpeningBalances",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "CostingMethod",
                table: "OpeningBalances",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)2);

            migrationBuilder.AddColumn<byte>(
                name: "DisplayMethod",
                table: "OpeningBalances",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)1);

            migrationBuilder.AddColumn<byte>(
                name: "EntryMethod",
                table: "OpeningBalances",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)1);

            migrationBuilder.AddColumn<byte>(
                name: "Status",
                table: "OpeningBalances",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)1);

            migrationBuilder.AddColumn<bool>(
                name: "UseBatchNumbers",
                table: "OpeningBalances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UseExpiryDate",
                table: "OpeningBalances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UseSerialNumbers",
                table: "OpeningBalances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte>(
                name: "WeightedAverageScope",
                table: "OpeningBalances",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)1);

            // Preserve legacy IsApproved / IsPosted into Status before dropping.
            migrationBuilder.Sql("""
                UPDATE OpeningBalances
                SET [Status] = CASE
                    WHEN [IsPosted] = 1 THEN 3
                    WHEN [IsApproved] = 1 THEN 2
                    ELSE 1
                END,
                [ApprovalDate] = CASE
                    WHEN [IsApproved] = 1 OR [IsPosted] = 1 THEN SYSUTCDATETIME()
                    ELSE NULL
                END
                """);

            migrationBuilder.DropColumn(name: "IsApproved", table: "OpeningBalances");
            migrationBuilder.DropColumn(name: "IsPosted", table: "OpeningBalances");

            migrationBuilder.AddColumn<string>(
                name: "BatchNumber",
                table: "OpeningBalanceLines",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExpiryDate",
                table: "OpeningBalanceLines",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "OpeningBalanceLines",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseId",
                table: "OpeningBalanceLines",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql("""
                UPDATE l
                SET l.WarehouseId = h.WarehouseId
                FROM OpeningBalanceLines l
                INNER JOIN OpeningBalances h ON h.Id = l.OpeningBalanceId
                WHERE l.WarehouseId = '00000000-0000-0000-0000-000000000000'
                  AND h.WarehouseId IS NOT NULL
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_OpeningBalances_TenantId_Status' AND object_id = OBJECT_ID(N'OpeningBalances'))
                    CREATE INDEX [IX_OpeningBalances_TenantId_Status] ON [OpeningBalances] ([TenantId], [Status]) WHERE [IsDeleted] = 0;

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_OpeningBalanceLines_InventoryItemId' AND object_id = OBJECT_ID(N'OpeningBalanceLines'))
                    CREATE INDEX [IX_OpeningBalanceLines_InventoryItemId] ON [OpeningBalanceLines] ([InventoryItemId]);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_OpeningBalanceLines_OpeningBalanceId' AND object_id = OBJECT_ID(N'OpeningBalanceLines'))
                    CREATE INDEX [IX_OpeningBalanceLines_OpeningBalanceId] ON [OpeningBalanceLines] ([OpeningBalanceId]);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_OpeningBalanceLines_WarehouseId' AND object_id = OBJECT_ID(N'OpeningBalanceLines'))
                    CREATE INDEX [IX_OpeningBalanceLines_WarehouseId] ON [OpeningBalanceLines] ([WarehouseId]);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OpeningBalances_TenantId_Status",
                table: "OpeningBalances");

            migrationBuilder.DropIndex(
                name: "IX_OpeningBalanceLines_InventoryItemId",
                table: "OpeningBalanceLines");

            migrationBuilder.DropIndex(
                name: "IX_OpeningBalanceLines_OpeningBalanceId",
                table: "OpeningBalanceLines");

            migrationBuilder.DropIndex(
                name: "IX_OpeningBalanceLines_WarehouseId",
                table: "OpeningBalanceLines");

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "OpeningBalances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPosted",
                table: "OpeningBalances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE OpeningBalances
                SET [IsApproved] = CASE WHEN [Status] IN (2, 3) THEN 1 ELSE 0 END,
                    [IsPosted] = CASE WHEN [Status] = 3 THEN 1 ELSE 0 END
                """);

            migrationBuilder.DropColumn(name: "ApprovalDate", table: "OpeningBalances");
            migrationBuilder.DropColumn(name: "ContraAccountId", table: "OpeningBalances");
            migrationBuilder.DropColumn(name: "CostCenterId", table: "OpeningBalances");
            migrationBuilder.DropColumn(name: "CostingMethod", table: "OpeningBalances");
            migrationBuilder.DropColumn(name: "DisplayMethod", table: "OpeningBalances");
            migrationBuilder.DropColumn(name: "EntryMethod", table: "OpeningBalances");
            migrationBuilder.DropColumn(name: "Status", table: "OpeningBalances");
            migrationBuilder.DropColumn(name: "UseBatchNumbers", table: "OpeningBalances");
            migrationBuilder.DropColumn(name: "UseExpiryDate", table: "OpeningBalances");
            migrationBuilder.DropColumn(name: "UseSerialNumbers", table: "OpeningBalances");
            migrationBuilder.DropColumn(name: "WeightedAverageScope", table: "OpeningBalances");
            migrationBuilder.DropColumn(name: "BatchNumber", table: "OpeningBalanceLines");
            migrationBuilder.DropColumn(name: "ExpiryDate", table: "OpeningBalanceLines");
            migrationBuilder.DropColumn(name: "SerialNumber", table: "OpeningBalanceLines");
            migrationBuilder.DropColumn(name: "WarehouseId", table: "OpeningBalanceLines");

            migrationBuilder.AlterColumn<Guid>(
                name: "WarehouseId",
                table: "OpeningBalances",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
