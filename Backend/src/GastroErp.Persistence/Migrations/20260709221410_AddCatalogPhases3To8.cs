using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogPhases3To8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BasePrice",
                table: "ProductCatalogDefinitions",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "ProductCatalogDefinitions",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailableOnPos",
                table: "ProductCatalogDefinitions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeaturedOnPos",
                table: "ProductCatalogDefinitions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "KitchenStationId",
                table: "ProductCatalogDefinitions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaUrlsJson",
                table: "ProductCatalogDefinitions",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrepTimeMinutes",
                table: "ProductCatalogDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PriceLevelsJson",
                table: "ProductCatalogDefinitions",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecipeInstructions",
                table: "ProductCatalogDefinitions",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecipePreparationTime",
                table: "ProductCatalogDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "RecipeWastePercentage",
                table: "ProductCatalogDefinitions",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RecipeYield",
                table: "ProductCatalogDefinitions",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "RelatedProductsJson",
                table: "ProductCatalogDefinitions",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierIdsJson",
                table: "ProductCatalogDefinitions",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VariantAttributesJson",
                table: "ProductCatalogDefinitions",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductPriceHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CatalogDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PriceLevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PriceLevelName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OldPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    NewPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
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
                    table.PrimaryKey("PK_ProductPriceHistories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductPriceHistories_TenantId_CatalogDefinitionId",
                table: "ProductPriceHistories",
                columns: new[] { "TenantId", "CatalogDefinitionId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductPriceHistories");

            migrationBuilder.DropColumn(
                name: "BasePrice",
                table: "ProductCatalogDefinitions");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "ProductCatalogDefinitions");

            migrationBuilder.DropColumn(
                name: "IsAvailableOnPos",
                table: "ProductCatalogDefinitions");

            migrationBuilder.DropColumn(
                name: "IsFeaturedOnPos",
                table: "ProductCatalogDefinitions");

            migrationBuilder.DropColumn(
                name: "KitchenStationId",
                table: "ProductCatalogDefinitions");

            migrationBuilder.DropColumn(
                name: "MediaUrlsJson",
                table: "ProductCatalogDefinitions");

            migrationBuilder.DropColumn(
                name: "PrepTimeMinutes",
                table: "ProductCatalogDefinitions");

            migrationBuilder.DropColumn(
                name: "PriceLevelsJson",
                table: "ProductCatalogDefinitions");

            migrationBuilder.DropColumn(
                name: "RecipeInstructions",
                table: "ProductCatalogDefinitions");

            migrationBuilder.DropColumn(
                name: "RecipePreparationTime",
                table: "ProductCatalogDefinitions");

            migrationBuilder.DropColumn(
                name: "RecipeWastePercentage",
                table: "ProductCatalogDefinitions");

            migrationBuilder.DropColumn(
                name: "RecipeYield",
                table: "ProductCatalogDefinitions");

            migrationBuilder.DropColumn(
                name: "RelatedProductsJson",
                table: "ProductCatalogDefinitions");

            migrationBuilder.DropColumn(
                name: "SupplierIdsJson",
                table: "ProductCatalogDefinitions");

            migrationBuilder.DropColumn(
                name: "VariantAttributesJson",
                table: "ProductCatalogDefinitions");
        }
    }
}
