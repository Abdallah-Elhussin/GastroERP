using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExpandIssueDestinationMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowChangeAccountOnIssue",
                table: "IssueDestinations",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowDirectIssue",
                table: "IssueDestinations",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowNegativeStock",
                table: "IssueDestinations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "DefaultGlAccountId",
                table: "IssueDestinations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "DestinationType",
                table: "IssueDestinations",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)13);

            migrationBuilder.AddColumn<bool>(
                name: "RequireApproval",
                table: "IssueDestinations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireBranch",
                table: "IssueDestinations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireCostCenter",
                table: "IssueDestinations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireEmployee",
                table: "IssueDestinations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireProject",
                table: "IssueDestinations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireReason",
                table: "IssueDestinations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkflowDefinitionId",
                table: "IssueDestinations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE IssueDestinations SET DestinationType = 13 WHERE DestinationType = 0;
                UPDATE IssueDestinations SET AllowDirectIssue = 1, AllowChangeAccountOnIssue = 1;

                -- Map legacy seeded codes to typed defaults when present.
                UPDATE IssueDestinations SET DestinationType = 1 WHERE Code IN (N'KIT', N'KITCHEN', N'BAR');
                UPDATE IssueDestinations SET DestinationType = 2 WHERE Code IN (N'PRD', N'PRODUCTION');
                UPDATE IssueDestinations SET DestinationType = 3 WHERE Code IN (N'BRN', N'BRANCH');
                UPDATE IssueDestinations SET DestinationType = 4 WHERE Code IN (N'ADM', N'DEPT');
                UPDATE IssueDestinations SET DestinationType = 5 WHERE Code = N'MKT';
                UPDATE IssueDestinations SET DestinationType = 6 WHERE Code = N'MNT';
                UPDATE IssueDestinations SET DestinationType = 7 WHERE Code IN (N'WST', N'WASTE');
                UPDATE IssueDestinations SET DestinationType = 8 WHERE Code = N'STM';
                UPDATE IssueDestinations SET DestinationType = 9 WHERE Code = N'CMP';
                UPDATE IssueDestinations SET DestinationType = 10 WHERE Code = N'AST';
                UPDATE IssueDestinations SET DestinationType = 11 WHERE Code = N'PROJECT';
                UPDATE IssueDestinations SET DestinationType = 99 WHERE Code IN (N'OTH', N'OTHER');
                """);

            migrationBuilder.CreateIndex(
                name: "IX_IssueDestinations_TenantId_DestinationType",
                table: "IssueDestinations",
                columns: new[] { "TenantId", "DestinationType" });

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_IssueDestinations_TenantId_NameAr'
                      AND object_id = OBJECT_ID(N'IssueDestinations'))
                BEGIN
                    CREATE UNIQUE INDEX [IX_IssueDestinations_TenantId_NameAr]
                    ON [IssueDestinations] ([TenantId], [NameAr])
                    WHERE [IsDeleted] = 0;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IssueDestinations_TenantId_DestinationType",
                table: "IssueDestinations");

            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_IssueDestinations_TenantId_NameAr'
                      AND object_id = OBJECT_ID(N'IssueDestinations'))
                    DROP INDEX [IX_IssueDestinations_TenantId_NameAr] ON [IssueDestinations];
                """);

            migrationBuilder.DropColumn(
                name: "AllowChangeAccountOnIssue",
                table: "IssueDestinations");

            migrationBuilder.DropColumn(
                name: "AllowDirectIssue",
                table: "IssueDestinations");

            migrationBuilder.DropColumn(
                name: "AllowNegativeStock",
                table: "IssueDestinations");

            migrationBuilder.DropColumn(
                name: "DefaultGlAccountId",
                table: "IssueDestinations");

            migrationBuilder.DropColumn(
                name: "DestinationType",
                table: "IssueDestinations");

            migrationBuilder.DropColumn(
                name: "RequireApproval",
                table: "IssueDestinations");

            migrationBuilder.DropColumn(
                name: "RequireBranch",
                table: "IssueDestinations");

            migrationBuilder.DropColumn(
                name: "RequireCostCenter",
                table: "IssueDestinations");

            migrationBuilder.DropColumn(
                name: "RequireEmployee",
                table: "IssueDestinations");

            migrationBuilder.DropColumn(
                name: "RequireProject",
                table: "IssueDestinations");

            migrationBuilder.DropColumn(
                name: "RequireReason",
                table: "IssueDestinations");

            migrationBuilder.DropColumn(
                name: "WorkflowDefinitionId",
                table: "IssueDestinations");
        }
    }
}
