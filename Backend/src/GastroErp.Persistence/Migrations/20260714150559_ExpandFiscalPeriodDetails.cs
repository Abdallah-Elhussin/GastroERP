using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GastroErp.Persistence.Migrations;

/// <inheritdoc />
public partial class ExpandFiscalPeriodDetails : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF COL_LENGTH('FiscalPeriods', 'Notes') IS NULL
                ALTER TABLE FiscalPeriods ADD Notes nvarchar(1000) NULL;

            IF COL_LENGTH('FiscalPeriods', 'PeriodPolicy') IS NULL
                ALTER TABLE FiscalPeriods ADD PeriodPolicy int NOT NULL CONSTRAINT DF_FiscalPeriods_PeriodPolicy DEFAULT(1);

            IF COL_LENGTH('FiscalPeriods', 'StartMonth') IS NULL
                ALTER TABLE FiscalPeriods ADD StartMonth tinyint NOT NULL CONSTRAINT DF_FiscalPeriods_StartMonth DEFAULT(1);

            IF OBJECT_ID(N'FiscalPeriodDetails', N'U') IS NULL
            BEGIN
                CREATE TABLE FiscalPeriodDetails (
                    Id uniqueidentifier NOT NULL,
                    FiscalPeriodId uniqueidentifier NOT NULL,
                    TenantId uniqueidentifier NOT NULL,
                    PeriodNumber int NOT NULL,
                    NameAr nvarchar(200) NOT NULL,
                    NameEn nvarchar(200) NOT NULL,
                    StartDate date NOT NULL,
                    EndDate date NOT NULL,
                    Status int NOT NULL,
                    CONSTRAINT PK_FiscalPeriodDetails PRIMARY KEY (Id),
                    CONSTRAINT FK_FiscalPeriodDetails_FiscalPeriods_FiscalPeriodId
                        FOREIGN KEY (FiscalPeriodId) REFERENCES FiscalPeriods(Id) ON DELETE CASCADE
                );

                CREATE UNIQUE INDEX IX_FiscalPeriodDetails_FiscalPeriodId_PeriodNumber
                    ON FiscalPeriodDetails (FiscalPeriodId, PeriodNumber);

                CREATE INDEX IX_FiscalPeriodDetails_TenantId
                    ON FiscalPeriodDetails (TenantId);
            END
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'FiscalPeriodDetails', N'U') IS NOT NULL
                DROP TABLE FiscalPeriodDetails;

            IF COL_LENGTH('FiscalPeriods', 'Notes') IS NOT NULL
                ALTER TABLE FiscalPeriods DROP COLUMN Notes;

            IF COL_LENGTH('FiscalPeriods', 'PeriodPolicy') IS NOT NULL
            BEGIN
                DECLARE @df1 sysname;
                SELECT @df1 = dc.name FROM sys.default_constraints dc
                INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
                WHERE dc.parent_object_id = OBJECT_ID('FiscalPeriods') AND c.name = 'PeriodPolicy';
                IF @df1 IS NOT NULL EXEC('ALTER TABLE FiscalPeriods DROP CONSTRAINT [' + @df1 + ']');
                ALTER TABLE FiscalPeriods DROP COLUMN PeriodPolicy;
            END

            IF COL_LENGTH('FiscalPeriods', 'StartMonth') IS NOT NULL
            BEGIN
                DECLARE @df2 sysname;
                SELECT @df2 = dc.name FROM sys.default_constraints dc
                INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
                WHERE dc.parent_object_id = OBJECT_ID('FiscalPeriods') AND c.name = 'StartMonth';
                IF @df2 IS NOT NULL EXEC('ALTER TABLE FiscalPeriods DROP CONSTRAINT [' + @df2 + ']');
                ALTER TABLE FiscalPeriods DROP COLUMN StartMonth;
            END
            """);
    }
}
