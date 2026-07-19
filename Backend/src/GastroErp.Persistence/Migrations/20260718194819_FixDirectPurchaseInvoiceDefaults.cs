using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixDirectPurchaseInvoiceDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PaymentTerms",
                table: "Suppliers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Suppliers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BlacklistReason",
                table: "Suppliers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Suppliers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Category",
                table: "Suppliers",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Suppliers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Suppliers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CommercialRegister",
                table: "Suppliers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "CommercialRegisterExpiry",
                table: "Suppliers",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommercialRegisterPath",
                table: "Suppliers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "Suppliers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactJobTitle",
                table: "Suppliers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPerson",
                table: "Suppliers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Suppliers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "DefaultPaymentMethod",
                table: "Suppliers",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<decimal>(
                name: "DefaultTaxPercent",
                table: "Suppliers",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "DiscountAccountId",
                table: "Suppliers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Suppliers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstablishmentNumber",
                table: "Suppliers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ExchangeDifferenceAccountId",
                table: "Suppliers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBlacklisted",
                table: "Suppliers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Mobile",
                table: "Suppliers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Suppliers",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OpeningBalance",
                table: "Suppliers",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateOnly>(
                name: "OpeningBalanceDate",
                table: "Suppliers",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Suppliers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Suppliers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PurchaseReturnAccountId",
                table: "Suppliers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "Suppliers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "SupplierType",
                table: "Suppliers",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "TaxCertificateExpiry",
                table: "Suppliers",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxCertificatePath",
                table: "Suppliers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxRegistrationCountry",
                table: "Suppliers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxType",
                table: "Suppliers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "VatEvaluation",
                table: "Suppliers",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Suppliers",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Position",
                table: "SupplierContacts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "SupplierContacts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mobile",
                table: "SupplierContacts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SupplierAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_SupplierAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierAttachments_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierPaymentMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kind = table.Column<byte>(type: "tinyint", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Iban = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Swift = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BeneficiaryName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_SupplierPaymentMethods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierPaymentMethods_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_TenantId_Code",
                table: "Suppliers",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_TenantId_CompanyId_NameAr",
                table: "Suppliers",
                columns: new[] { "TenantId", "CompanyId", "NameAr" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierAttachments_SupplierId",
                table: "SupplierAttachments",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPaymentMethods_SupplierId",
                table: "SupplierPaymentMethods",
                column: "SupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupplierAttachments");

            migrationBuilder.DropTable(
                name: "SupplierPaymentMethods");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_TenantId_Code",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_TenantId_CompanyId_NameAr",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "BlacklistReason",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "CommercialRegister",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "CommercialRegisterExpiry",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "CommercialRegisterPath",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "ContactJobTitle",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "ContactPerson",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "DefaultPaymentMethod",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "DefaultTaxPercent",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "DiscountAccountId",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "EstablishmentNumber",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "ExchangeDifferenceAccountId",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "IsBlacklisted",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Mobile",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "OpeningBalance",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "OpeningBalanceDate",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "PurchaseReturnAccountId",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "SupplierType",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "TaxCertificateExpiry",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "TaxCertificatePath",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "TaxRegistrationCountry",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "TaxType",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "VatEvaluation",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Mobile",
                table: "SupplierContacts");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentTerms",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Position",
                table: "SupplierContacts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "SupplierContacts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
