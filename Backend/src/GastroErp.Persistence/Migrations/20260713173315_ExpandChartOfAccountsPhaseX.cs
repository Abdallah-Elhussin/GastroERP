using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExpandChartOfAccountsPhaseX : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSystemAccount",
                table: "ChartOfAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "ChartOfAccounts",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AccountingSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AccountNumberMaxLength = table.Column<int>(type: "int", nullable: false),
                    MaxTreeLevels = table.Column<int>(type: "int", nullable: false),
                    LevelLengthsCsv = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LevelSeparator = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    CashAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BankAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InventoryAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CogsAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SalesRevenueAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PurchaseAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AccountsReceivableAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AccountsPayableAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VatInputAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VatOutputAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DiscountAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RoundOffAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OpeningBalanceAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RetainedEarningsAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PayrollExpenseAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PayrollLiabilityAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProductionVarianceAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InventoryAdjustmentAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WasteAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeliveryRevenueAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeliveryExpenseAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    KitchenConsumptionAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerAdvancesAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SupplierAdvancesAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExchangeDifferenceAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AutoPostSales = table.Column<bool>(type: "bit", nullable: false),
                    AutoPostPurchases = table.Column<bool>(type: "bit", nullable: false),
                    AutoPostGoodsReceipt = table.Column<bool>(type: "bit", nullable: false),
                    AutoPostGoodsIssue = table.Column<bool>(type: "bit", nullable: false),
                    AutoPostStockTransfer = table.Column<bool>(type: "bit", nullable: false),
                    AutoPostWaste = table.Column<bool>(type: "bit", nullable: false),
                    AutoPostProduction = table.Column<bool>(type: "bit", nullable: false),
                    AutoPostPayroll = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_AccountingSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChartOfAccounts_TenantId_IsSystemAccount",
                table: "ChartOfAccounts",
                columns: new[] { "TenantId", "IsSystemAccount" },
                filter: "[IsDeleted] = 0 AND [IsSystemAccount] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingSettings_TenantId",
                table: "AccountingSettings",
                column: "TenantId",
                unique: true,
                filter: "[IsDeleted] = 0 AND [CompanyId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingSettings_TenantId_CompanyId",
                table: "AccountingSettings",
                columns: new[] { "TenantId", "CompanyId" },
                unique: true,
                filter: "[IsDeleted] = 0 AND [CompanyId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountingSettings");

            migrationBuilder.DropIndex(
                name: "IX_ChartOfAccounts_TenantId_IsSystemAccount",
                table: "ChartOfAccounts");

            migrationBuilder.DropColumn(
                name: "IsSystemAccount",
                table: "ChartOfAccounts");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "ChartOfAccounts");
        }
    }
}
