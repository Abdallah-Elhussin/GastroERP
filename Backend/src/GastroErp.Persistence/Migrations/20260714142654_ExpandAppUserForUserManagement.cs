using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations;

/// <inheritdoc />
public partial class ExpandAppUserForUserManagement : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Code",
            table: "AppUsers",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "IsPosUser",
            table: "AppUsers",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "MobileNumber",
            table: "AppUsers",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "UserName",
            table: "AppUsers",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: "");

        // Backfill UserName from Email local-part; suffix Id when colliding.
        migrationBuilder.Sql("""
            UPDATE u
            SET UserName = CASE
                WHEN CHARINDEX('@', u.Email) > 1 THEN LEFT(u.Email, CHARINDEX('@', u.Email) - 1)
                ELSE LEFT(REPLACE(CAST(u.Id AS nvarchar(36)), '-', ''), 20)
            END
            FROM AppUsers u
            WHERE u.UserName = '' OR u.UserName IS NULL;

            ;WITH d AS (
                SELECT Id, UserName,
                       ROW_NUMBER() OVER (PARTITION BY TenantId, UserName ORDER BY CreatedAt) AS rn
                FROM AppUsers
                WHERE IsDeleted = 0
            )
            UPDATE u
            SET UserName = LEFT(d.UserName, 80) + CAST(d.rn AS nvarchar(10))
            FROM AppUsers u
            INNER JOIN d ON d.Id = u.Id
            WHERE d.rn > 1;
            """);

        migrationBuilder.CreateIndex(
            name: "IX_AppUsers_TenantId_UserName",
            table: "AppUsers",
            columns: new[] { "TenantId", "UserName" },
            unique: true,
            filter: "[IsDeleted] = 0");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_AppUsers_TenantId_UserName",
            table: "AppUsers");

        migrationBuilder.DropColumn(
            name: "Code",
            table: "AppUsers");

        migrationBuilder.DropColumn(
            name: "IsPosUser",
            table: "AppUsers");

        migrationBuilder.DropColumn(
            name: "MobileNumber",
            table: "AppUsers");

        migrationBuilder.DropColumn(
            name: "UserName",
            table: "AppUsers");
    }
}
