using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentTypesMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Module = table.Column<byte>(type: "tinyint", nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    StartingNumber = table.Column<long>(type: "bigint", nullable: false),
                    LastNumber = table.Column<long>(type: "bigint", nullable: false),
                    NumberLength = table.Column<byte>(type: "tinyint", nullable: false),
                    ResetYearly = table.Column<bool>(type: "bit", nullable: false),
                    ResetMonthly = table.Column<bool>(type: "bit", nullable: false),
                    NumberPerBranch = table.Column<bool>(type: "bit", nullable: false),
                    NumberPerCompany = table.Column<bool>(type: "bit", nullable: false),
                    ApprovalMode = table.Column<byte>(type: "tinyint", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "bit", nullable: false),
                    UsesWorkflow = table.Column<bool>(type: "bit", nullable: false),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PostingMode = table.Column<byte>(type: "tinyint", nullable: false),
                    AutoPost = table.Column<bool>(type: "bit", nullable: false),
                    PostAfterApproval = table.Column<bool>(type: "bit", nullable: false),
                    AffectsInventory = table.Column<bool>(type: "bit", nullable: false),
                    AffectsCost = table.Column<bool>(type: "bit", nullable: false),
                    AffectsAccounting = table.Column<bool>(type: "bit", nullable: false),
                    AffectsCash = table.Column<bool>(type: "bit", nullable: false),
                    AffectsCustomers = table.Column<bool>(type: "bit", nullable: false),
                    AffectsSuppliers = table.Column<bool>(type: "bit", nullable: false),
                    AffectsAssets = table.Column<bool>(type: "bit", nullable: false),
                    AffectsPayroll = table.Column<bool>(type: "bit", nullable: false),
                    AllowCreate = table.Column<bool>(type: "bit", nullable: false),
                    AllowUpdate = table.Column<bool>(type: "bit", nullable: false),
                    AllowApprove = table.Column<bool>(type: "bit", nullable: false),
                    AllowPost = table.Column<bool>(type: "bit", nullable: false),
                    AllowCancel = table.Column<bool>(type: "bit", nullable: false),
                    AllowDelete = table.Column<bool>(type: "bit", nullable: false),
                    AllowAttachments = table.Column<bool>(type: "bit", nullable: false),
                    AllowPrint = table.Column<bool>(type: "bit", nullable: false),
                    AllowEditAfterSave = table.Column<bool>(type: "bit", nullable: false),
                    AllowDeleteDocuments = table.Column<bool>(type: "bit", nullable: false),
                    AllowCancelDocuments = table.Column<bool>(type: "bit", nullable: false),
                    AllowCopy = table.Column<bool>(type: "bit", nullable: false),
                    AllowReopen = table.Column<bool>(type: "bit", nullable: false),
                    ShowInReports = table.Column<bool>(type: "bit", nullable: false),
                    ShowInDashboard = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_DocumentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentTypeLifecycleStages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsTerminal = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypeLifecycleStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentTypeLifecycleStages_DocumentTypes_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypeLifecycleStages_DocumentTypeId",
                table: "DocumentTypeLifecycleStages",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypeLifecycleStages_DocumentTypeId_Code",
                table: "DocumentTypeLifecycleStages",
                columns: new[] { "DocumentTypeId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_TenantId_Code",
                table: "DocumentTypes",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_TenantId_Module",
                table: "DocumentTypes",
                columns: new[] { "TenantId", "Module" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_TenantId_NameAr",
                table: "DocumentTypes",
                columns: new[] { "TenantId", "NameAr" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentTypeLifecycleStages");

            migrationBuilder.DropTable(
                name: "DocumentTypes");
        }
    }
}
