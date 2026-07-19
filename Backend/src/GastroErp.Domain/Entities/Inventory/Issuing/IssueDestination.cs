using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Inventory.Issuing;

/// <summary>جهة الصرف (Aggregate Root) — master data لعمليات الصرف المخزني.</summary>
public sealed class IssueDestination : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public string Code { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string? Description { get; private set; }
    public IssueDestinationType DestinationType { get; private set; }

    public Guid? DefaultGlAccountId { get; private set; }
    public Guid? DefaultCostCenterId { get; private set; }
    public bool AllowChangeAccountOnIssue { get; private set; }

    public bool RequireEmployee { get; private set; }
    public bool RequireProject { get; private set; }
    public bool RequireCostCenter { get; private set; }
    public bool RequireBranch { get; private set; }
    public bool RequireReason { get; private set; }
    public bool RequireApproval { get; private set; }
    public bool AllowDirectIssue { get; private set; }
    public bool AllowNegativeStock { get; private set; }

    public Guid? WorkflowDefinitionId { get; private set; }

    public int SortOrder { get; private set; }
    public bool IsSystem { get; private set; }
    public bool IsActive { get; private set; }

    private IssueDestination()
    {
        Code = string.Empty;
        NameAr = string.Empty;
    }

    public IssueDestination(
        Guid tenantId,
        string code,
        string nameAr,
        IssueDestinationType destinationType,
        string? nameEn = null,
        string? description = null,
        Guid? defaultGlAccountId = null,
        Guid? defaultCostCenterId = null,
        bool allowChangeAccountOnIssue = true,
        bool requireEmployee = false,
        bool requireProject = false,
        bool requireCostCenter = false,
        bool requireBranch = false,
        bool requireReason = false,
        bool requireApproval = false,
        bool allowDirectIssue = true,
        bool allowNegativeStock = false,
        Guid? workflowDefinitionId = null,
        int sortOrder = 0,
        bool isSystem = false)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);

        TenantId = tenantId;
        Code = code.Trim().ToUpperInvariant();
        NameAr = nameAr.Trim();
        NameEn = nameEn?.Trim();
        Description = description?.Trim();
        DestinationType = destinationType;
        DefaultGlAccountId = defaultGlAccountId;
        DefaultCostCenterId = defaultCostCenterId;
        AllowChangeAccountOnIssue = allowChangeAccountOnIssue;
        RequireEmployee = requireEmployee;
        RequireProject = requireProject;
        RequireCostCenter = requireCostCenter;
        RequireBranch = requireBranch;
        RequireReason = requireReason;
        RequireApproval = requireApproval;
        AllowDirectIssue = allowDirectIssue;
        AllowNegativeStock = allowNegativeStock;
        WorkflowDefinitionId = workflowDefinitionId;
        SortOrder = Math.Max(0, sortOrder);
        IsSystem = isSystem;
        IsActive = true;
    }

    public void UpdateGeneral(string nameAr, string? nameEn, string? description, IssueDestinationType destinationType, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        NameAr = nameAr.Trim();
        NameEn = nameEn?.Trim();
        Description = description?.Trim();
        DestinationType = destinationType;
        SortOrder = Math.Max(0, sortOrder);
    }

    public void UpdateAccounting(Guid? defaultGlAccountId, Guid? defaultCostCenterId, bool allowChangeAccountOnIssue)
    {
        DefaultGlAccountId = defaultGlAccountId;
        DefaultCostCenterId = defaultCostCenterId;
        AllowChangeAccountOnIssue = allowChangeAccountOnIssue;
    }

    public void UpdatePolicies(
        bool requireEmployee,
        bool requireProject,
        bool requireCostCenter,
        bool requireBranch,
        bool requireReason,
        bool requireApproval,
        bool allowDirectIssue,
        bool allowNegativeStock)
    {
        RequireEmployee = requireEmployee;
        RequireProject = requireProject;
        RequireCostCenter = requireCostCenter;
        RequireBranch = requireBranch;
        RequireReason = requireReason;
        RequireApproval = requireApproval;
        AllowDirectIssue = allowDirectIssue;
        AllowNegativeStock = allowNegativeStock;
    }

    public void SetWorkflow(Guid? workflowDefinitionId) => WorkflowDefinitionId = workflowDefinitionId;

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public void SoftDeleteDestination(string? deletedBy)
    {
        if (IsSystem) throw new BusinessException(ErrorCodes.CannotModifyApprovedDocument);
        SoftDelete(deletedBy);
        IsActive = false;
    }

    public string BuildPolicySummary()
    {
        var parts = new List<string>(8);
        if (RequireEmployee) parts.Add("Employee");
        if (RequireProject) parts.Add("Project");
        if (RequireCostCenter) parts.Add("CostCenter");
        if (RequireBranch) parts.Add("Branch");
        if (RequireReason) parts.Add("Reason");
        if (RequireApproval) parts.Add("Approval");
        if (AllowDirectIssue) parts.Add("Direct");
        if (AllowNegativeStock) parts.Add("Negative");
        return parts.Count == 0 ? "—" : string.Join(", ", parts);
    }
}
