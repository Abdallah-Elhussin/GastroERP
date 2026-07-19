using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RedesignPurchaseReturnTypesAndReasons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "PurchaseReturns");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "PurchaseReturns",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreditNoteJournalEntryId",
                table: "PurchaseReturns",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "PurchaseReturns",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "JournalEntryId",
                table: "PurchaseReturns",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "PurchaseReturns",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PostedAt",
                table: "PurchaseReturns",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PostedBy",
                table: "PurchaseReturns",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PurchaseInvoiceId",
                table: "PurchaseReturns",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReasonNotes",
                table: "PurchaseReturns",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                table: "PurchaseReturns",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReturnReasonId",
                table: "PurchaseReturns",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "ReturnType",
                table: "PurchaseReturns",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<Guid>(
                name: "ReversalJournalEntryId",
                table: "PurchaseReturns",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Status",
                table: "PurchaseReturns",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<decimal>(
                name: "SubTotal",
                table: "PurchaseReturns",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "PurchaseReturns",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "PurchaseReturns",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "PurchaseReturnLines",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BatchNumber",
                table: "PurchaseReturnLines",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DestroyItem",
                table: "PurchaseReturnLines",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "PurchaseReturnLines",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExpiryDate",
                table: "PurchaseReturnLines",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GoodsReceiptLineId",
                table: "PurchaseReturnLines",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LineReason",
                table: "PurchaseReturnLines",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalQuantity",
                table: "PurchaseReturnLines",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PreviouslyReturnedQuantity",
                table: "PurchaseReturnLines",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ProductTemperature",
                table: "PurchaseReturnLines",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PurchaseInvoiceLineId",
                table: "PurchaseReturnLines",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "PurchaseReturnLines",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxPercent",
                table: "PurchaseReturnLines",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ReturnedQuantity",
                table: "PurchaseInvoiceLines",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ReturnedQuantity",
                table: "GoodsReceiptLines",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "PurchaseReturnReasons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_PurchaseReturnReasons", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_GoodsReceiptId",
                table: "PurchaseReturns",
                column: "GoodsReceiptId",
                filter: "[GoodsReceiptId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_PurchaseInvoiceId",
                table: "PurchaseReturns",
                column: "PurchaseInvoiceId",
                filter: "[PurchaseInvoiceId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_SupplierId",
                table: "PurchaseReturns",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_TenantId_ReturnNumber",
                table: "PurchaseReturns",
                columns: new[] { "TenantId", "ReturnNumber" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnLines_GoodsReceiptLineId",
                table: "PurchaseReturnLines",
                column: "GoodsReceiptLineId",
                filter: "[GoodsReceiptLineId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnLines_PurchaseInvoiceLineId",
                table: "PurchaseReturnLines",
                column: "PurchaseInvoiceLineId",
                filter: "[PurchaseInvoiceLineId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnReasons_TenantId_Code",
                table: "PurchaseReturnReasons",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseReturnReasons");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReturns_GoodsReceiptId",
                table: "PurchaseReturns");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReturns_PurchaseInvoiceId",
                table: "PurchaseReturns");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReturns_SupplierId",
                table: "PurchaseReturns");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReturns_TenantId_ReturnNumber",
                table: "PurchaseReturns");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReturnLines_GoodsReceiptLineId",
                table: "PurchaseReturnLines");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReturnLines_PurchaseInvoiceLineId",
                table: "PurchaseReturnLines");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "CreditNoteJournalEntryId",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "JournalEntryId",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "PostedAt",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "PostedBy",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "PurchaseInvoiceId",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "ReasonNotes",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "ReturnReasonId",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "ReturnType",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "ReversalJournalEntryId",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "SubTotal",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "BatchNumber",
                table: "PurchaseReturnLines");

            migrationBuilder.DropColumn(
                name: "DestroyItem",
                table: "PurchaseReturnLines");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "PurchaseReturnLines");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "PurchaseReturnLines");

            migrationBuilder.DropColumn(
                name: "GoodsReceiptLineId",
                table: "PurchaseReturnLines");

            migrationBuilder.DropColumn(
                name: "LineReason",
                table: "PurchaseReturnLines");

            migrationBuilder.DropColumn(
                name: "OriginalQuantity",
                table: "PurchaseReturnLines");

            migrationBuilder.DropColumn(
                name: "PreviouslyReturnedQuantity",
                table: "PurchaseReturnLines");

            migrationBuilder.DropColumn(
                name: "ProductTemperature",
                table: "PurchaseReturnLines");

            migrationBuilder.DropColumn(
                name: "PurchaseInvoiceLineId",
                table: "PurchaseReturnLines");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "PurchaseReturnLines");

            migrationBuilder.DropColumn(
                name: "TaxPercent",
                table: "PurchaseReturnLines");

            migrationBuilder.DropColumn(
                name: "ReturnedQuantity",
                table: "PurchaseInvoiceLines");

            migrationBuilder.DropColumn(
                name: "ReturnedQuantity",
                table: "GoodsReceiptLines");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "PurchaseReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "PurchaseReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "PurchaseReturnLines",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
