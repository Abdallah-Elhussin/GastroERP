using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExpandExchangeRatePeriods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Preserve meaningful mid rate into SellRate before reshape.
            migrationBuilder.Sql("UPDATE [CurrencyExchangeRates] SET [SellRate] = [MidRate];");

            migrationBuilder.DropColumn(
                name: "BuyRate",
                table: "CurrencyExchangeRates");

            migrationBuilder.DropColumn(
                name: "MidRate",
                table: "CurrencyExchangeRates");

            migrationBuilder.RenameColumn(
                name: "SellRate",
                table: "CurrencyExchangeRates",
                newName: "Rate");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "CurrencyExchangeRates",
                newName: "ChangeReason");

            migrationBuilder.RenameColumn(
                name: "EffectiveDate",
                table: "CurrencyExchangeRates",
                newName: "StartDate");

            migrationBuilder.RenameIndex(
                name: "IX_CurrencyExchangeRates_TenantId_CurrencyId_EffectiveDate",
                table: "CurrencyExchangeRates",
                newName: "IX_CurrencyExchangeRates_TenantId_CurrencyId_StartDate");

            migrationBuilder.AddColumn<DateOnly>(
                name: "EndDate",
                table: "CurrencyExchangeRates",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CurrencyExchangeRates",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "Number",
                table: "CurrencyExchangeRates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                WITH numbered AS (
                    SELECT [Id],
                           ROW_NUMBER() OVER (PARTITION BY [TenantId] ORDER BY [StartDate], [CreatedAt], [Id]) AS [Rn]
                    FROM [CurrencyExchangeRates]
                    WHERE [IsDeleted] = 0
                )
                UPDATE c
                SET c.[Number] = n.[Rn],
                    c.[IsActive] = 1
                FROM [CurrencyExchangeRates] c
                INNER JOIN numbered n ON c.[Id] = n.[Id];
                """);

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyExchangeRates_TenantId_CurrencyId",
                table: "CurrencyExchangeRates",
                columns: new[] { "TenantId", "CurrencyId" },
                unique: true,
                filter: "[IsDeleted] = 0 AND [EndDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyExchangeRates_TenantId_Number",
                table: "CurrencyExchangeRates",
                columns: new[] { "TenantId", "Number" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CurrencyExchangeRates_TenantId_CurrencyId",
                table: "CurrencyExchangeRates");

            migrationBuilder.DropIndex(
                name: "IX_CurrencyExchangeRates_TenantId_Number",
                table: "CurrencyExchangeRates");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "CurrencyExchangeRates");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CurrencyExchangeRates");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "CurrencyExchangeRates");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "CurrencyExchangeRates",
                newName: "EffectiveDate");

            migrationBuilder.RenameColumn(
                name: "Rate",
                table: "CurrencyExchangeRates",
                newName: "SellRate");

            migrationBuilder.RenameColumn(
                name: "ChangeReason",
                table: "CurrencyExchangeRates",
                newName: "Notes");

            migrationBuilder.RenameIndex(
                name: "IX_CurrencyExchangeRates_TenantId_CurrencyId_StartDate",
                table: "CurrencyExchangeRates",
                newName: "IX_CurrencyExchangeRates_TenantId_CurrencyId_EffectiveDate");

            migrationBuilder.AddColumn<decimal>(
                name: "BuyRate",
                table: "CurrencyExchangeRates",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MidRate",
                table: "CurrencyExchangeRates",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql("""
                UPDATE [CurrencyExchangeRates]
                SET [BuyRate] = [SellRate],
                    [MidRate] = [SellRate];
                """);
        }
    }
}
