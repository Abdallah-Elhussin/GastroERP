using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCashBoxesMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CashBoxes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PosDeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChartOfAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OpeningDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AllowReceive = table.Column<bool>(type: "bit", nullable: false),
                    AllowPay = table.Column<bool>(type: "bit", nullable: false),
                    AllowDeposit = table.Column<bool>(type: "bit", nullable: false),
                    AllowWithdraw = table.Column<bool>(type: "bit", nullable: false),
                    AllowTransfer = table.Column<bool>(type: "bit", nullable: false),
                    RequireShiftBeforeUse = table.Column<bool>(type: "bit", nullable: false),
                    AllowNegativeBalance = table.Column<bool>(type: "bit", nullable: false),
                    MinBalance = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MaxBalance = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    CurrentBalance = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CurrentUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastOpenedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastClosedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastMovementAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastCountAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsOpen = table.Column<bool>(type: "bit", nullable: false),
                    HasHadMovement = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_CashBoxes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CashBoxDevices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CashBoxId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeviceRole = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashBoxDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashBoxDevices_CashBoxes_CashBoxId",
                        column: x => x.CashBoxId,
                        principalTable: "CashBoxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CashBoxUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CashBoxId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsManager = table.Column<bool>(type: "bit", nullable: false),
                    IsCustodian = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashBoxUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashBoxUsers_CashBoxes_CashBoxId",
                        column: x => x.CashBoxId,
                        principalTable: "CashBoxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CashBoxDevices_CashBoxId",
                table: "CashBoxDevices",
                column: "CashBoxId");

            migrationBuilder.CreateIndex(
                name: "IX_CashBoxes_TenantId_ChartOfAccountId",
                table: "CashBoxes",
                columns: new[] { "TenantId", "ChartOfAccountId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CashBoxes_TenantId_Code",
                table: "CashBoxes",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CashBoxes_TenantId_CompanyId_BranchId",
                table: "CashBoxes",
                columns: new[] { "TenantId", "CompanyId", "BranchId" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CashBoxes_TenantId_NameAr",
                table: "CashBoxes",
                columns: new[] { "TenantId", "NameAr" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CashBoxes_TenantId_Number",
                table: "CashBoxes",
                columns: new[] { "TenantId", "Number" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CashBoxUsers_CashBoxId",
                table: "CashBoxUsers",
                column: "CashBoxId");

            migrationBuilder.CreateIndex(
                name: "IX_CashBoxUsers_CashBoxId_UserId",
                table: "CashBoxUsers",
                columns: new[] { "CashBoxId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CashBoxDevices");

            migrationBuilder.DropTable(
                name: "CashBoxUsers");

            migrationBuilder.DropTable(
                name: "CashBoxes");
        }
    }
}
