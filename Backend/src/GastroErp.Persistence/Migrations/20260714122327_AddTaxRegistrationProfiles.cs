using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxRegistrationProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaxRegistrationProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VatNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BranchVatNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TaxOffice = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TaxpayerType = table.Column<int>(type: "int", nullable: false),
                    ActivityCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ActivityNameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ActivityNameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DefaultTaxRate = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    RegistrationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_TaxRegistrationProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaxRegistrationCertificates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaxRegistrationProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DocumentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    UploadedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxRegistrationCertificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxRegistrationCertificates_TaxRegistrationProfiles_TaxRegistrationProfileId",
                        column: x => x.TaxRegistrationProfileId,
                        principalTable: "TaxRegistrationProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaxRegistrationCertificates_TaxRegistrationProfileId",
                table: "TaxRegistrationCertificates",
                column: "TaxRegistrationProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRegistrationCertificates_TaxRegistrationProfileId_Version",
                table: "TaxRegistrationCertificates",
                columns: new[] { "TaxRegistrationProfileId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxRegistrationProfiles_TenantId_CompanyId_BranchId",
                table: "TaxRegistrationProfiles",
                columns: new[] { "TenantId", "CompanyId", "BranchId" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRegistrationProfiles_TenantId_Number",
                table: "TaxRegistrationProfiles",
                columns: new[] { "TenantId", "Number" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRegistrationProfiles_TenantId_VatNumber",
                table: "TaxRegistrationProfiles",
                columns: new[] { "TenantId", "VatNumber" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaxRegistrationCertificates");

            migrationBuilder.DropTable(
                name: "TaxRegistrationProfiles");
        }
    }
}
