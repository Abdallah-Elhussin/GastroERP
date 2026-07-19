using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExpandInventorySettingsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowDeleteDraft",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowEditDraft",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowNegativeCost",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowUnpost",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowZeroCost",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AutoPostAfterApproval",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AutoRecalculateCost",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AutoReleaseReservation",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CheckAvailableQuantity",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "InventorySettings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "CostPrecision",
                table: "InventorySettings",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<bool>(
                name: "CreateReverseEntry",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CycleCountReminder",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DefaultCurrencyCode",
                table: "InventorySettings",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DefaultUnitId",
                table: "InventorySettings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailNotifications",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableAccountingIntegration",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableBarcode",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableBins",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableDeliveryIntegration",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableKitchenIntegration",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableMobileScanner",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableMultiBranch",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableMultiCompany",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableMultiWarehouse",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnablePosIntegration",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableProductionIntegration",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnablePurchasingIntegration",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableQrCode",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableRfid",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableSerialTracking",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableShelves",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableWarehouseHierarchy",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableWarehouseZones",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ExpiredItemsAlert",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FreezeDuringCount",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LockPostedDocuments",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LowStockAlert",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NearExpiryAlert",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OutOfStockAlert",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PushNotifications",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireApprovalBeforePosting",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RoundCost",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ValidateWarehouseBeforePosting",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "InventoryDocumentNumberSeries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventorySettingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<byte>(type: "tinyint", nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NumberLength = table.Column<byte>(type: "tinyint", nullable: false),
                    NextNumber = table.Column<long>(type: "bigint", nullable: false),
                    AutoIncrement = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_InventoryDocumentNumberSeries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryDocumentNumberSeries_InventorySettings_InventorySettingId",
                        column: x => x.InventorySettingId,
                        principalTable: "InventorySettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDocumentNumberSeries_InventorySettingId_DocumentType",
                table: "InventoryDocumentNumberSeries",
                columns: new[] { "InventorySettingId", "DocumentType" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDocumentNumberSeries_TenantId",
                table: "InventoryDocumentNumberSeries",
                column: "TenantId",
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryDocumentNumberSeries");

            migrationBuilder.DropColumn(
                name: "AllowDeleteDraft",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "AllowEditDraft",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "AllowNegativeCost",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "AllowUnpost",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "AllowZeroCost",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "AutoPostAfterApproval",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "AutoRecalculateCost",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "AutoReleaseReservation",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "CheckAvailableQuantity",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "CostPrecision",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "CreateReverseEntry",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "CycleCountReminder",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "DefaultCurrencyCode",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "DefaultUnitId",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EmailNotifications",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EnableAccountingIntegration",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EnableBarcode",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EnableBins",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EnableDeliveryIntegration",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EnableKitchenIntegration",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EnableMobileScanner",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EnableMultiBranch",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EnableMultiCompany",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EnableMultiWarehouse",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EnablePosIntegration",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EnableProductionIntegration",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EnablePurchasingIntegration",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EnableQrCode",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EnableRfid",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EnableSerialTracking",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EnableShelves",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EnableWarehouseHierarchy",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "EnableWarehouseZones",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "ExpiredItemsAlert",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "FreezeDuringCount",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "LockPostedDocuments",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "LowStockAlert",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "NearExpiryAlert",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "OutOfStockAlert",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "PushNotifications",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "RequireApprovalBeforePosting",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "RoundCost",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "ValidateWarehouseBeforePosting",
                table: "InventorySettings");
        }
    }
}
