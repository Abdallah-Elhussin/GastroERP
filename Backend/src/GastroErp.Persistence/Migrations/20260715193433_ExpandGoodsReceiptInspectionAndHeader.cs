using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExpandGoodsReceiptInspectionAndHeader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastReceiptDate",
                table: "PurchaseOrders",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SupplierInvoiceNumber",
                table: "GoodsReceipts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "GoodsReceipts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "GoodsReceipts",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "GoodsReceipts",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ExpiryCertificateRef",
                table: "GoodsReceipts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InspectedBy",
                table: "GoodsReceipts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "InspectionDate",
                table: "GoodsReceipts",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "InspectionResult",
                table: "GoodsReceipts",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "QualityCertificateRef",
                table: "GoodsReceipts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QualityNotes",
                table: "GoodsReceipts",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptMethod",
                table: "GoodsReceipts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceivedByName",
                table: "GoodsReceipts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                table: "GoodsReceipts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "GoodsReceipts",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Source",
                table: "GoodsReceipts",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "SupplierRepName",
                table: "GoodsReceipts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleNumber",
                table: "GoodsReceipts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WaybillNumber",
                table: "GoodsReceipts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BatchNumber",
                table: "GoodsReceiptLines",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AcceptedQuantity",
                table: "GoodsReceiptLines",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "GoodsReceiptLines",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "GoodsReceiptLines",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OrderedQuantity",
                table: "GoodsReceiptLines",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PreviouslyReceivedQuantity",
                table: "GoodsReceiptLines",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RejectedQuantity",
                table: "GoodsReceiptLines",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "StorageLocation",
                table: "GoodsReceiptLines",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "GoodsReceiptLines",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxPercent",
                table: "GoodsReceiptLines",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceipts_PurchaseOrderId",
                table: "GoodsReceipts",
                column: "PurchaseOrderId",
                filter: "[PurchaseOrderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceipts_SupplierId",
                table: "GoodsReceipts",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceipts_TenantId_ReceiptNumber",
                table: "GoodsReceipts",
                columns: new[] { "TenantId", "ReceiptNumber" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GoodsReceipts_PurchaseOrderId",
                table: "GoodsReceipts");

            migrationBuilder.DropIndex(
                name: "IX_GoodsReceipts_SupplierId",
                table: "GoodsReceipts");

            migrationBuilder.DropIndex(
                name: "IX_GoodsReceipts_TenantId_ReceiptNumber",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "LastReceiptDate",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "ExpiryCertificateRef",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "InspectedBy",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "InspectionDate",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "InspectionResult",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "QualityCertificateRef",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "QualityNotes",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "ReceiptMethod",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "ReceivedByName",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "SupplierRepName",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "VehicleNumber",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "WaybillNumber",
                table: "GoodsReceipts");

            migrationBuilder.DropColumn(
                name: "AcceptedQuantity",
                table: "GoodsReceiptLines");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "GoodsReceiptLines");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "GoodsReceiptLines");

            migrationBuilder.DropColumn(
                name: "OrderedQuantity",
                table: "GoodsReceiptLines");

            migrationBuilder.DropColumn(
                name: "PreviouslyReceivedQuantity",
                table: "GoodsReceiptLines");

            migrationBuilder.DropColumn(
                name: "RejectedQuantity",
                table: "GoodsReceiptLines");

            migrationBuilder.DropColumn(
                name: "StorageLocation",
                table: "GoodsReceiptLines");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "GoodsReceiptLines");

            migrationBuilder.DropColumn(
                name: "TaxPercent",
                table: "GoodsReceiptLines");

            migrationBuilder.AlterColumn<string>(
                name: "SupplierInvoiceNumber",
                table: "GoodsReceipts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BatchNumber",
                table: "GoodsReceiptLines",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
