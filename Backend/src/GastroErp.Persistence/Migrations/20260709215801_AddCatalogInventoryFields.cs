using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogInventoryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowNegativeStock",
                table: "ProductCatalogDefinitions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "BaseUnitId",
                table: "ProductCatalogDefinitions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "CostingMethod",
                table: "ProductCatalogDefinitions",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<Guid>(
                name: "DefaultPurchaseUnitId",
                table: "ProductCatalogDefinitions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DefaultRecipeUnitId",
                table: "ProductCatalogDefinitions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxStock",
                table: "ProductCatalogDefinitions",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MinStock",
                table: "ProductCatalogDefinitions",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ReorderLevel",
                table: "ProductCatalogDefinitions",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ReorderQuantity",
                table: "ProductCatalogDefinitions",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SafetyStock",
                table: "ProductCatalogDefinitions",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "TrackBatch",
                table: "ProductCatalogDefinitions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TrackExpiry",
                table: "ProductCatalogDefinitions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TrackSerial",
                table: "ProductCatalogDefinitions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowNegativeStock",
                table: "ProductCatalogDefinitions");

            migrationBuilder.DropColumn(
                name: "BaseUnitId",
                table: "ProductCatalogDefinitions");

            migrationBuilder.DropColumn(
                name: "CostingMethod",
                table: "ProductCatalogDefinitions");

            migrationBuilder.DropColumn(
                name: "DefaultPurchaseUnitId",
                table: "ProductCatalogDefinitions");

            migrationBuilder.DropColumn(
                name: "DefaultRecipeUnitId",
                table: "ProductCatalogDefinitions");

            migrationBuilder.DropColumn(
                name: "MaxStock",
                table: "ProductCatalogDefinitions");

            migrationBuilder.DropColumn(
                name: "MinStock",
                table: "ProductCatalogDefinitions");

            migrationBuilder.DropColumn(
                name: "ReorderLevel",
                table: "ProductCatalogDefinitions");

            migrationBuilder.DropColumn(
                name: "ReorderQuantity",
                table: "ProductCatalogDefinitions");

            migrationBuilder.DropColumn(
                name: "SafetyStock",
                table: "ProductCatalogDefinitions");

            migrationBuilder.DropColumn(
                name: "TrackBatch",
                table: "ProductCatalogDefinitions");

            migrationBuilder.DropColumn(
                name: "TrackExpiry",
                table: "ProductCatalogDefinitions");

            migrationBuilder.DropColumn(
                name: "TrackSerial",
                table: "ProductCatalogDefinitions");
        }
    }
}
