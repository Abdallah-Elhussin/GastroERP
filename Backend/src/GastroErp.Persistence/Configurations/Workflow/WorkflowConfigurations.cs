using GastroErp.Domain.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastroErp.Persistence.Configurations.Workflow;

internal static class WorkflowConfigHelper
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

public sealed class WorkflowDefinitionConfiguration : IEntityTypeConfiguration<WorkflowDefinition>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinition> builder)
    {
        builder.ToTable("WorkflowDefinitions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Description).HasMaxLength(1000);
        WorkflowConfigHelper.ConfigureAuditable(builder);
        builder.HasIndex(x => new { x.TenantId, x.Code, x.Version }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class WorkflowStepConfiguration : IEntityTypeConfiguration<WorkflowStep>
{
    public void Configure(EntityTypeBuilder<WorkflowStep> builder)
    {
        builder.ToTable("WorkflowSteps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        WorkflowConfigHelper.ConfigureAuditable(builder);
        builder.HasIndex(x => new { x.TenantId, x.WorkflowDefinitionId, x.StepOrder }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class WorkflowConditionConfiguration : IEntityTypeConfiguration<WorkflowCondition>
{
    public void Configure(EntityTypeBuilder<WorkflowCondition> builder)
    {
        builder.ToTable("WorkflowConditions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FieldName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Value).IsRequired().HasMaxLength(500);
        builder.Property(x => x.LogicalOperator).HasMaxLength(10);
        WorkflowConfigHelper.ConfigureAuditable(builder);
    }
}

public sealed class WorkflowInstanceConfiguration : IEntityTypeConfiguration<WorkflowInstance>
{
    public void Configure(EntityTypeBuilder<WorkflowInstance> builder)
    {
        builder.ToTable("WorkflowInstances");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ReferenceType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ContextJson).HasMaxLength(4000);
        WorkflowConfigHelper.ConfigureAuditable(builder);
        builder.HasIndex(x => new { x.TenantId, x.ReferenceType, x.ReferenceId }).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.Status }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class WorkflowApprovalConfiguration : IEntityTypeConfiguration<WorkflowApproval>
{
    public void Configure(EntityTypeBuilder<WorkflowApproval> builder)
    {
        builder.ToTable("WorkflowApprovals");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Comments).HasMaxLength(2000);
        WorkflowConfigHelper.ConfigureAuditable(builder);
    }
}

public sealed class WorkflowHistoryConfiguration : IEntityTypeConfiguration<WorkflowHistory>
{
    public void Configure(EntityTypeBuilder<WorkflowHistory> builder)
    {
        builder.ToTable("WorkflowHistories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).IsRequired().HasMaxLength(100);
        builder.Property(x => x.OldStatus).HasMaxLength(50);
        builder.Property(x => x.NewStatus).HasMaxLength(50);
        builder.Property(x => x.Details).HasMaxLength(2000);
        WorkflowConfigHelper.ConfigureAuditable(builder);
    }
}

public sealed class ApprovalDelegateConfiguration : IEntityTypeConfiguration<ApprovalDelegate>
{
    public void Configure(EntityTypeBuilder<ApprovalDelegate> builder)
    {
        builder.ToTable("ApprovalDelegates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).HasMaxLength(500);
        WorkflowConfigHelper.ConfigureAuditable(builder);
        builder.HasIndex(x => new { x.TenantId, x.UserId, x.IsActive }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class ApprovalEscalationConfiguration : IEntityTypeConfiguration<ApprovalEscalation>
{
    public void Configure(EntityTypeBuilder<ApprovalEscalation> builder)
    {
        builder.ToTable("ApprovalEscalations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EscalateToRole).IsRequired().HasMaxLength(100);
        WorkflowConfigHelper.ConfigureAuditable(builder);
    }
}
