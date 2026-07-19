using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExpandGoodsIssueDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "WarehouseId",
                table: "GoodsIssues",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ApprovalDate",
                table: "GoodsIssues",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "GoodsIssues",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "SAR");

            migrationBuilder.AddColumn<Guid>(
                name: "IssueDestinationId",
                table: "GoodsIssues",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Status",
                table: "GoodsIssues",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)1);

            // Preserve legacy IsConfirmed / IsCompleted into Status before dropping.
            migrationBuilder.Sql("""
                UPDATE GoodsIssues
                SET [Status] = CASE
                    WHEN [IsCompleted] = 1 THEN 3
                    WHEN [IsConfirmed] = 1 THEN 2
                    ELSE 1
                END,
                [ApprovalDate] = CASE
                    WHEN [IsConfirmed] = 1 OR [IsCompleted] = 1 THEN SYSUTCDATETIME()
                    ELSE NULL
                END,
                [Currency] = CASE WHEN [Currency] IS NULL OR [Currency] = N'' THEN N'SAR' ELSE [Currency] END
                """);

            migrationBuilder.DropColumn(name: "IsCompleted", table: "GoodsIssues");
            migrationBuilder.DropColumn(name: "IsConfirmed", table: "GoodsIssues");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "GoodsIssueLines",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CostCenterId",
                table: "GoodsIssueLines",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCost",
                table: "GoodsIssueLines",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseId",
                table: "GoodsIssueLines",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // Copy header warehouse onto existing lines.
            migrationBuilder.Sql("""
                UPDATE l
                SET l.WarehouseId = g.WarehouseId
                FROM GoodsIssueLines l
                INNER JOIN GoodsIssues g ON g.Id = l.GoodsIssueId
                WHERE g.WarehouseId IS NOT NULL
                """);

            migrationBuilder.CreateTable(
                name: "IssueDestinations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DefaultCostCenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_IssueDestinations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoodsIssues_TenantId_Status",
                table: "GoodsIssues",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_IssueDestinations_TenantId_Code",
                table: "IssueDestinations",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "IssueDestinations");

            migrationBuilder.DropIndex(
                name: "IX_GoodsIssues_TenantId_Status",
                table: "GoodsIssues");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "GoodsIssues",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsConfirmed",
                table: "GoodsIssues",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE GoodsIssues
                SET [IsCompleted] = CASE WHEN [Status] = 3 THEN 1 ELSE 0 END,
                    [IsConfirmed] = CASE WHEN [Status] IN (2, 3) THEN 1 ELSE 0 END
                """);

            migrationBuilder.DropColumn(name: "ApprovalDate", table: "GoodsIssues");
            migrationBuilder.DropColumn(name: "Currency", table: "GoodsIssues");
            migrationBuilder.DropColumn(name: "IssueDestinationId", table: "GoodsIssues");
            migrationBuilder.DropColumn(name: "Status", table: "GoodsIssues");
            migrationBuilder.DropColumn(name: "CostCenterId", table: "GoodsIssueLines");
            migrationBuilder.DropColumn(name: "UnitCost", table: "GoodsIssueLines");
            migrationBuilder.DropColumn(name: "WarehouseId", table: "GoodsIssueLines");

            migrationBuilder.AlterColumn<Guid>(
                name: "WarehouseId",
                table: "GoodsIssues",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "GoodsIssueLines",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
