using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinancialNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    NoteKind = table.Column<byte>(type: "tinyint", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NoteDate = table.Column<DateOnly>(type: "date", nullable: false),
                    FiscalPeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartyType = table.Column<int>(type: "int", nullable: false),
                    PartyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PartyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MainAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    ReferenceType = table.Column<byte>(type: "tinyint", nullable: false),
                    ReferenceDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    JournalEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PostedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PostedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CancelledBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
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
                    table.PrimaryKey("PK_FinancialNotes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialNoteLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FinancialNoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    NotificationReasonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OffsetAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CostCenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AnalyticalAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialNoteLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialNoteLines_FinancialNotes_FinancialNoteId",
                        column: x => x.FinancialNoteId,
                        principalTable: "FinancialNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialNoteLines_FinancialNoteId",
                table: "FinancialNoteLines",
                column: "FinancialNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialNoteLines_FinancialNoteId_LineNumber",
                table: "FinancialNoteLines",
                columns: new[] { "FinancialNoteId", "LineNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialNoteLines_NotificationReasonId",
                table: "FinancialNoteLines",
                column: "NotificationReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialNotes_NoteKind",
                table: "FinancialNotes",
                column: "NoteKind",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialNotes_Status",
                table: "FinancialNotes",
                column: "Status",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialNotes_TenantId_DocumentNumber",
                table: "FinancialNotes",
                columns: new[] { "TenantId", "DocumentNumber" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialNotes_TenantId_NoteDate",
                table: "FinancialNotes",
                columns: new[] { "TenantId", "NoteDate" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialNotes_TenantId_Number",
                table: "FinancialNotes",
                columns: new[] { "TenantId", "Number" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinancialNoteLines");

            migrationBuilder.DropTable(
                name: "FinancialNotes");
        }
    }
}
