using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Ai;

/// <summary>Advisory AI recommendation with apply/dismiss audit trail</summary>
public sealed class RecommendationAction : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public RecommendationType Type { get; private set; }
    public RecommendationStatus Status { get; private set; }
    public RecommendationPriority Priority { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string PayloadJson { get; private set; }
    public string ExplainabilityJson { get; private set; }
    public string? ReferenceType { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public Guid? BranchId { get; private set; }
    public DateTimeOffset? AppliedAt { get; private set; }
    public Guid? AppliedBy { get; private set; }
    public DateTimeOffset? DismissedAt { get; private set; }
    public Guid? DismissedBy { get; private set; }
    public string? DismissReason { get; private set; }

    private RecommendationAction()
    {
        Title = string.Empty;
        Description = string.Empty;
        PayloadJson = "{}";
        ExplainabilityJson = "{}";
    }

    public static RecommendationAction Create(
        Guid tenantId, RecommendationType type, RecommendationPriority priority,
        string title, string description, string payloadJson, string explainabilityJson,
        string? referenceType = null, Guid? referenceId = null, Guid? branchId = null)
        => new()
        {
            TenantId = tenantId,
            Type = type,
            Status = RecommendationStatus.Pending,
            Priority = priority,
            Title = title,
            Description = description,
            PayloadJson = payloadJson,
            ExplainabilityJson = explainabilityJson,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            BranchId = branchId
        };

    public void Apply(Guid userId)
    {
        if (Status != RecommendationStatus.Pending)
            return;
        Status = RecommendationStatus.Applied;
        AppliedBy = userId;
        AppliedAt = DateTimeOffset.UtcNow;
    }

    public void Dismiss(Guid userId, string? reason = null)
    {
        if (Status != RecommendationStatus.Pending)
            return;
        Status = RecommendationStatus.Dismissed;
        DismissedBy = userId;
        DismissedAt = DateTimeOffset.UtcNow;
        DismissReason = reason;
    }
}
