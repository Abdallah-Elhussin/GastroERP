using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialOpeningBalances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinancialOpeningBalances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OpeningDate = table.Column<DateOnly>(type: "date", nullable: false),
                    FiscalPeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    EquityAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    JournalEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PostedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PostedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialOpeningBalances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialOpeningBalanceLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FinancialOpeningBalanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    ChartOfAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CostCenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialOpeningBalanceLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialOpeningBalanceLines_FinancialOpeningBalances_FinancialOpeningBalanceId",
                        column: x => x.FinancialOpeningBalanceId,
                        principalTable: "FinancialOpeningBalances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialOpeningBalanceLines_FinancialOpeningBalanceId",
                table: "FinancialOpeningBalanceLines",
                column: "FinancialOpeningBalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialOpeningBalanceLines_FinancialOpeningBalanceId_LineNumber",
                table: "FinancialOpeningBalanceLines",
                columns: new[] { "FinancialOpeningBalanceId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialOpeningBalances_Status",
                table: "FinancialOpeningBalances",
                column: "Status",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialOpeningBalances_TenantId_CompanyId_FiscalPeriodId",
                table: "FinancialOpeningBalances",
                columns: new[] { "TenantId", "CompanyId", "FiscalPeriodId" },
                unique: true,
                filter: "[IsDeleted] = 0 AND [Status] = 2");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialOpeningBalances_TenantId_DocumentNumber",
                table: "FinancialOpeningBalances",
                columns: new[] { "TenantId", "DocumentNumber" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialOpeningBalances_TenantId_Number",
                table: "FinancialOpeningBalances",
                columns: new[] { "TenantId", "Number" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinancialOpeningBalanceLines");

            migrationBuilder.DropTable(
                name: "FinancialOpeningBalances");
        }
    }
}
