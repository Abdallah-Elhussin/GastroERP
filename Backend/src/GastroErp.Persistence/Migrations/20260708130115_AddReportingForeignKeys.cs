using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReportingForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ScheduledReports_ReportDefinitionId",
                table: "ScheduledReports",
                column: "ReportDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledReports_TenantId_ReportDefinitionId",
                table: "ScheduledReports",
                columns: new[] { "TenantId", "ReportDefinitionId" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportExecutions_ReportDefinitionId",
                table: "ReportExecutions",
                column: "ReportDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_KpiSnapshots_KpiDefinitionId",
                table: "KpiSnapshots",
                column: "KpiDefinitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_KpiSnapshots_KpiDefinitions_KpiDefinitionId",
                table: "KpiSnapshots",
                column: "KpiDefinitionId",
                principalTable: "KpiDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportExecutions_ReportDefinitions_ReportDefinitionId",
                table: "ReportExecutions",
                column: "ReportDefinitionId",
                principalTable: "ReportDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduledReports_ReportDefinitions_ReportDefinitionId",
                table: "ScheduledReports",
                column: "ReportDefinitionId",
                principalTable: "ReportDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KpiSnapshots_KpiDefinitions_KpiDefinitionId",
                table: "KpiSnapshots");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportExecutions_ReportDefinitions_ReportDefinitionId",
                table: "ReportExecutions");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduledReports_ReportDefinitions_ReportDefinitionId",
                table: "ScheduledReports");

            migrationBuilder.DropIndex(
                name: "IX_ScheduledReports_ReportDefinitionId",
                table: "ScheduledReports");

            migrationBuilder.DropIndex(
                name: "IX_ScheduledReports_TenantId_ReportDefinitionId",
                table: "ScheduledReports");

            migrationBuilder.DropIndex(
                name: "IX_ReportExecutions_ReportDefinitionId",
                table: "ReportExecutions");

            migrationBuilder.DropIndex(
                name: "IX_KpiSnapshots_KpiDefinitionId",
                table: "KpiSnapshots");
        }
    }
}
