using GastroErp.Domain.Entities.Reporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastroErp.Persistence.Configurations.Reporting;

internal static class ReportingConfigHelper
{
    public static void ConfigureAuditable<T>(EntityTypeBuilder<T> builder) where T : Domain.Common.AuditableBaseEntity
    {
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);
        builder.Property(x => x.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class DashboardConfiguration : IEntityTypeConfiguration<Dashboard>
{
    public void Configure(EntityTypeBuilder<Dashboard> builder)
    {
        builder.ToTable("ReportingDashboards");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.LayoutJson).HasColumnType("nvarchar(max)");
        ReportingConfigHelper.ConfigureAuditable(builder);
        builder.HasIndex(x => new { x.TenantId, x.Name }).HasFilter("[IsDeleted] = 0");
        builder.HasMany(x => x.Widgets).WithOne().HasForeignKey(w => w.DashboardId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Widgets).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class DashboardWidgetConfiguration : IEntityTypeConfiguration<DashboardWidget>
{
    public void Configure(EntityTypeBuilder<DashboardWidget> builder)
    {
        builder.ToTable("ReportingDashboardWidgets");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ConfigurationJson).HasColumnType("nvarchar(max)");
        ReportingConfigHelper.ConfigureAuditable(builder);
    }
}

public sealed class ReportDefinitionConfiguration : IEntityTypeConfiguration<ReportDefinition>
{
    public void Configure(EntityTypeBuilder<ReportDefinition> builder)
    {
        builder.ToTable("ReportDefinitions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.DataSource).IsRequired().HasMaxLength(200);
        builder.Property(x => x.QueryDefinition).HasColumnType("nvarchar(max)");
        builder.Property(x => x.ParametersJson).HasColumnType("nvarchar(max)");
        ReportingConfigHelper.ConfigureAuditable(builder);
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class ReportExecutionConfiguration : IEntityTypeConfiguration<ReportExecution>
{
    public void Configure(EntityTypeBuilder<ReportExecution> builder)
    {
        builder.ToTable("ReportExecutions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ResultJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);
        ReportingConfigHelper.ConfigureAuditable(builder);
        builder.HasIndex(x => new { x.TenantId, x.ReportDefinitionId, x.ExecutionDate });
        builder.HasOne<ReportDefinition>()
            .WithMany()
            .HasForeignKey(x => x.ReportDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ScheduledReportConfiguration : IEntityTypeConfiguration<ScheduledReport>
{
    public void Configure(EntityTypeBuilder<ScheduledReport> builder)
    {
        builder.ToTable("ScheduledReports");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CronExpression).HasMaxLength(100);
        builder.Property(x => x.EmailRecipients).HasMaxLength(2000);
        ReportingConfigHelper.ConfigureAuditable(builder);
        builder.HasIndex(x => new { x.TenantId, x.ReportDefinitionId });
        builder.HasOne<ReportDefinition>()
            .WithMany()
            .HasForeignKey(x => x.ReportDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class KpiDefinitionConfiguration : IEntityTypeConfiguration<KpiDefinition>
{
    public void Configure(EntityTypeBuilder<KpiDefinition> builder)
    {
        builder.ToTable("KpiDefinitions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Formula).IsRequired().HasMaxLength(500);
        builder.Property(x => x.TargetValue).HasPrecision(18, 4);
        builder.Property(x => x.WarningValue).HasPrecision(18, 4);
        builder.Property(x => x.CriticalValue).HasPrecision(18, 4);
        ReportingConfigHelper.ConfigureAuditable(builder);
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class KpiSnapshotConfiguration : IEntityTypeConfiguration<KpiSnapshot>
{
    public void Configure(EntityTypeBuilder<KpiSnapshot> builder)
    {
        builder.ToTable("KpiSnapshots");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Value).HasPrecision(18, 4);
        ReportingConfigHelper.ConfigureAuditable(builder);
        builder.HasIndex(x => new { x.TenantId, x.KpiDefinitionId, x.SnapshotDate });
        builder.HasOne<KpiDefinition>()
            .WithMany()
            .HasForeignKey(x => x.KpiDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
