using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExpandDirectPurchaseInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CostCenterId",
                table: "PurchaseInvoices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "PurchaseInvoices",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "PurchaseInvoices",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<string>(
                name: "ExternalReference",
                table: "PurchaseInvoices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Nature",
                table: "PurchaseInvoices",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)1);

            migrationBuilder.AddColumn<Guid>(
                name: "ReversalJournalEntryId",
                table: "PurchaseInvoices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BatchNumber",
                table: "PurchaseInvoiceLines",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CostCenterId",
                table: "PurchaseInvoiceLines",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "PurchaseInvoiceLines",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercent",
                table: "PurchaseInvoiceLines",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExpiryDate",
                table: "PurchaseInvoiceLines",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LineWarehouseId",
                table: "PurchaseInvoiceLines",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ProductionDate",
                table: "PurchaseInvoiceLines",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "PurchaseInvoiceLines",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxPercent",
                table: "PurchaseInvoiceLines",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "FixedAssetAccountId",
                table: "AccountingSettings",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostCenterId",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "ExternalReference",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "Nature",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "ReversalJournalEntryId",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "BatchNumber",
                table: "PurchaseInvoiceLines");

            migrationBuilder.DropColumn(
                name: "CostCenterId",
                table: "PurchaseInvoiceLines");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "PurchaseInvoiceLines");

            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "PurchaseInvoiceLines");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "PurchaseInvoiceLines");

            migrationBuilder.DropColumn(
                name: "LineWarehouseId",
                table: "PurchaseInvoiceLines");

            migrationBuilder.DropColumn(
                name: "ProductionDate",
                table: "PurchaseInvoiceLines");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "PurchaseInvoiceLines");

            migrationBuilder.DropColumn(
                name: "TaxPercent",
                table: "PurchaseInvoiceLines");

            migrationBuilder.DropColumn(
                name: "FixedAssetAccountId",
                table: "AccountingSettings");
        }
    }
}
