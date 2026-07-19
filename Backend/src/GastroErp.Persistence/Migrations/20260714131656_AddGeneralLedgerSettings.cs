using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneralLedgerSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GeneralLedgerSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoucherNumberLength = table.Column<int>(type: "int", nullable: false),
                    DecimalPlaces = table.Column<int>(type: "int", nullable: false),
                    ShowDateInReports = table.Column<bool>(type: "bit", nullable: false),
                    ShowPostingIndicator = table.Column<bool>(type: "bit", nullable: false),
                    AutoPostReceiptChecks = table.Column<bool>(type: "bit", nullable: false),
                    AutoPostPaymentChecks = table.Column<bool>(type: "bit", nullable: false),
                    UseBudgetPerCurrency = table.Column<bool>(type: "bit", nullable: false),
                    AllowZeroEffectEntries = table.Column<bool>(type: "bit", nullable: false),
                    RequireJournalType = table.Column<bool>(type: "bit", nullable: false),
                    AllowManualTaxEntries = table.Column<bool>(type: "bit", nullable: false),
                    RequireReferenceNumber = table.Column<bool>(type: "bit", nullable: false),
                    ClosingMethod = table.Column<int>(type: "int", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_GeneralLedgerSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLedgerSettings_BranchId",
                table: "GeneralLedgerSettings",
                column: "BranchId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLedgerSettings_CompanyId",
                table: "GeneralLedgerSettings",
                column: "CompanyId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLedgerSettings_TenantId",
                table: "GeneralLedgerSettings",
                column: "TenantId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLedgerSettings_TenantId_CompanyId_BranchId",
                table: "GeneralLedgerSettings",
                columns: new[] { "TenantId", "CompanyId", "BranchId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLedgerSettings_TenantId_Number",
                table: "GeneralLedgerSettings",
                columns: new[] { "TenantId", "Number" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneralLedgerSettings");
        }
    }
}
