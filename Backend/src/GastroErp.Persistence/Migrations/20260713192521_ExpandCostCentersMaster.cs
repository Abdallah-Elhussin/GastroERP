using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExpandCostCentersMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "CostCenterType",
                table: "CostCenters",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)1);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CostCenters",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystem",
                table: "CostCenters",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Number",
                table: "CostCenters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentCostCenterId",
                table: "CostCenters",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "CostCenters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "UseInAssets",
                table: "CostCenters",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseInInventory",
                table: "CostCenters",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseInJournals",
                table: "CostCenters",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseInMaintenance",
                table: "CostCenters",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseInPayroll",
                table: "CostCenters",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseInProduction",
                table: "CostCenters",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseInPurchases",
                table: "CostCenters",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseInSales",
                table: "CostCenters",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.Sql("""
                ;WITH numbered AS (
                    SELECT Id, ROW_NUMBER() OVER (PARTITION BY TenantId ORDER BY Code, CreatedAt) AS rn
                    FROM CostCenters
                    WHERE IsDeleted = 0
                )
                UPDATE c
                SET Number = n.rn, SortOrder = n.rn,
                    CostCenterType = CASE WHEN c.CostCenterType = 0 THEN 1 ELSE c.CostCenterType END
                FROM CostCenters c
                INNER JOIN numbered n ON c.Id = n.Id;
                """);

            migrationBuilder.CreateTable(
                name: "CostCenterAllowedAccounts",
                columns: table => new
                {
                    CostCenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChartOfAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostCenterAllowedAccounts", x => new { x.CostCenterId, x.ChartOfAccountId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_CostCenters_ParentCostCenterId",
                table: "CostCenters",
                column: "ParentCostCenterId",
                filter: "[IsDeleted] = 0 AND [ParentCostCenterId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CostCenters_TenantId_CostCenterType",
                table: "CostCenters",
                columns: new[] { "TenantId", "CostCenterType" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_CostCenters_TenantId_NameAr'
                      AND object_id = OBJECT_ID(N'CostCenters'))
                BEGIN
                    CREATE UNIQUE INDEX [IX_CostCenters_TenantId_NameAr]
                    ON [CostCenters] ([TenantId], [NameAr])
                    WHERE [IsDeleted] = 0;
                END
                """);

            migrationBuilder.CreateIndex(
                name: "IX_CostCenters_TenantId_Number",
                table: "CostCenters",
                columns: new[] { "TenantId", "Number" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CostCenterAllowedAccounts_ChartOfAccountId",
                table: "CostCenterAllowedAccounts",
                column: "ChartOfAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CostCenterAllowedAccounts");

            migrationBuilder.DropIndex(
                name: "IX_CostCenters_ParentCostCenterId",
                table: "CostCenters");

            migrationBuilder.DropIndex(
                name: "IX_CostCenters_TenantId_CostCenterType",
                table: "CostCenters");

            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_CostCenters_TenantId_NameAr'
                      AND object_id = OBJECT_ID(N'CostCenters'))
                    DROP INDEX [IX_CostCenters_TenantId_NameAr] ON [CostCenters];
                """);

            migrationBuilder.DropIndex(
                name: "IX_CostCenters_TenantId_Number",
                table: "CostCenters");

            migrationBuilder.DropColumn(
                name: "CostCenterType",
                table: "CostCenters");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CostCenters");

            migrationBuilder.DropColumn(
                name: "IsSystem",
                table: "CostCenters");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "CostCenters");

            migrationBuilder.DropColumn(
                name: "ParentCostCenterId",
                table: "CostCenters");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "CostCenters");

            migrationBuilder.DropColumn(
                name: "UseInAssets",
                table: "CostCenters");

            migrationBuilder.DropColumn(
                name: "UseInInventory",
                table: "CostCenters");

            migrationBuilder.DropColumn(
                name: "UseInJournals",
                table: "CostCenters");

            migrationBuilder.DropColumn(
                name: "UseInMaintenance",
                table: "CostCenters");

            migrationBuilder.DropColumn(
                name: "UseInPayroll",
                table: "CostCenters");

            migrationBuilder.DropColumn(
                name: "UseInProduction",
                table: "CostCenters");

            migrationBuilder.DropColumn(
                name: "UseInPurchases",
                table: "CostCenters");

            migrationBuilder.DropColumn(
                name: "UseInSales",
                table: "CostCenters");
        }
    }
}
