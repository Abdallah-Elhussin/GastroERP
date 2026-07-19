using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddJournalVoucherTypeAndApprove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AnalyticalAccountId",
                table: "JournalEntryLines",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "JournalEntryLines",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<byte>(
                name: "VoucherType",
                table: "JournalEntries",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_AnalyticalAccountId",
                table: "JournalEntryLines",
                column: "AnalyticalAccountId",
                filter: "[AnalyticalAccountId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JournalEntryLines_AnalyticalAccountId",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "AnalyticalAccountId",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "VoucherType",
                table: "JournalEntries");
        }
    }
}
