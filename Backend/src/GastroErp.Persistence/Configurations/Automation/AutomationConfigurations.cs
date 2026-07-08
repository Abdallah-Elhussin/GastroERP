using GastroErp.Domain.Entities.Automation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastroErp.Persistence.Configurations.Automation;

public sealed class NotificationMessageConfiguration : IEntityTypeConfiguration<NotificationMessage>
{
    public void Configure(EntityTypeBuilder<NotificationMessage> builder)
    {
        builder.ToTable("NotificationMessages");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Body).IsRequired().HasMaxLength(2000);
        builder.Property(n => n.ReferenceType).HasMaxLength(100);
        builder.Property(n => n.ErrorMessage).HasMaxLength(1000);
        builder.Property(n => n.CreatedBy).HasMaxLength(200);
        builder.Property(n => n.UpdatedBy).HasMaxLength(200);
        builder.Property(n => n.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(n => !n.IsDeleted);
        builder.HasIndex(n => new { n.TenantId, n.UserId, n.Status }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class JobExecutionLogConfiguration : IEntityTypeConfiguration<JobExecutionLog>
{
    public void Configure(EntityTypeBuilder<JobExecutionLog> builder)
    {
        builder.ToTable("JobExecutionLogs");
        builder.HasKey(j => j.Id);
        builder.Property(j => j.JobName).IsRequired().HasMaxLength(100);
        builder.Property(j => j.ExternalJobId).HasMaxLength(100);
        builder.Property(j => j.ErrorMessage).HasMaxLength(2000);
        builder.Property(j => j.Payload).HasMaxLength(4000);
        builder.Property(j => j.CreatedBy).HasMaxLength(200);
        builder.Property(j => j.UpdatedBy).HasMaxLength(200);
        builder.Property(j => j.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(j => !j.IsDeleted);
        builder.HasIndex(j => new { j.TenantId, j.JobName, j.Status }).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(j => j.ExternalJobId).HasFilter("[ExternalJobId] IS NOT NULL");
    }
}

public sealed class IntegrationConfigurationConfiguration : IEntityTypeConfiguration<IntegrationConfiguration>
{
    public void Configure(EntityTypeBuilder<IntegrationConfiguration> builder)
    {
        builder.ToTable("IntegrationConfigurations");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.SettingsJson).IsRequired();
        builder.Property(i => i.CreatedBy).HasMaxLength(200);
        builder.Property(i => i.UpdatedBy).HasMaxLength(200);
        builder.Property(i => i.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(i => !i.IsDeleted);
        builder.HasIndex(i => new { i.TenantId, i.ProviderType, i.ProviderName }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class WebhookSubscriptionConfiguration : IEntityTypeConfiguration<WebhookSubscription>
{
    public void Configure(EntityTypeBuilder<WebhookSubscription> builder)
    {
        builder.ToTable("WebhookSubscriptions");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.EventName).IsRequired().HasMaxLength(100);
        builder.Property(w => w.TargetUrl).IsRequired().HasMaxLength(500);
        builder.Property(w => w.Secret).IsRequired().HasMaxLength(200);
        builder.Property(w => w.CreatedBy).HasMaxLength(200);
        builder.Property(w => w.UpdatedBy).HasMaxLength(200);
        builder.Property(w => w.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(w => !w.IsDeleted);
        builder.HasIndex(w => new { w.TenantId, w.EventName }).HasFilter("[IsDeleted] = 0 AND [IsActive] = 1");
    }
}

public sealed class ExternalEventLogConfiguration : IEntityTypeConfiguration<ExternalEventLog>
{
    public void Configure(EntityTypeBuilder<ExternalEventLog> builder)
    {
        builder.ToTable("ExternalEventLogs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Provider).IsRequired().HasMaxLength(50);
        builder.Property(e => e.ExternalEventId).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Payload).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(200);
        builder.Property(e => e.UpdatedBy).HasMaxLength(200);
        builder.Property(e => e.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(e => !e.IsDeleted);
        builder.HasIndex(e => new { e.TenantId, e.Provider, e.ExternalEventId }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}
