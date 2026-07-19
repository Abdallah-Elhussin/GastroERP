using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaxCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    AppliesTo = table.Column<int>(type: "int", nullable: false),
                    CalculationMethod = table.Column<int>(type: "int", nullable: false),
                    SalesAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PurchaseAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PriceIncludesTax = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    HasBeenUsed = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_TaxCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaxCodeRates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaxCodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ToDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Rate = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxCodeRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxCodeRates_TaxCodes_TaxCodeId",
                        column: x => x.TaxCodeId,
                        principalTable: "TaxCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaxCodeRates_TaxCodeId",
                table: "TaxCodeRates",
                column: "TaxCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxCodeRates_TaxCodeId_FromDate",
                table: "TaxCodeRates",
                columns: new[] { "TaxCodeId", "FromDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TaxCodes_IsActive",
                table: "TaxCodes",
                column: "IsActive",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TaxCodes_TenantId_CompanyId",
                table: "TaxCodes",
                columns: new[] { "TenantId", "CompanyId" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TaxCodes_TenantId_CompanyId_BranchId_Code",
                table: "TaxCodes",
                columns: new[] { "TenantId", "CompanyId", "BranchId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TaxCodes_TenantId_Number",
                table: "TaxCodes",
                columns: new[] { "TenantId", "Number" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaxCodeRates");

            migrationBuilder.DropTable(
                name: "TaxCodes");
        }
    }
}
