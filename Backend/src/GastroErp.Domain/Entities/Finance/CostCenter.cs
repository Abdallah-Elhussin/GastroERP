using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Finance;

namespace GastroErp.Domain.Entities.Finance;

/// <summary>CostCenter — مركز تكلفة (Aggregate Root)</summary>
public sealed class CostCenter : AuditableBaseEntity, ITenantEntity, IBranchEntity
{
    public Guid TenantId { get; private set; }
    public Guid BranchId { get; private set; }
    public Guid? DepartmentId { get; private set; }
    public Guid? ParentCostCenterId { get; private set; }
    public int Number { get; private set; }
    public string Code { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string? Description { get; private set; }
    public CostCenterType CostCenterType { get; private set; }
    public CostCenterStatus Status { get; private set; }
    public bool IsSystem { get; private set; }
    public int SortOrder { get; private set; }

    // Module usage flags
    public bool UseInPurchases { get; private set; } = true;
    public bool UseInInventory { get; private set; } = true;
    public bool UseInProduction { get; private set; } = true;
    public bool UseInSales { get; private set; } = true;
    public bool UseInPayroll { get; private set; } = true;
    public bool UseInAssets { get; private set; } = true;
    public bool UseInMaintenance { get; private set; } = true;
    public bool UseInJournals { get; private set; } = true;

    private CostCenter()
    {
        Code = string.Empty;
        NameAr = string.Empty;
    }

    public static CostCenter Create(
        Guid tenantId,
        Guid branchId,
        int number,
        string code,
        string nameAr,
        CostCenterType type,
        Guid? parentCostCenterId = null,
        Guid? departmentId = null,
        string? nameEn = null,
        string? description = null,
        int sortOrder = 0,
        bool isSystem = false)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId required.", nameof(tenantId));
        if (branchId == Guid.Empty) throw new BusinessException(ErrorCodes.RequiredField);
        if (string.IsNullOrWhiteSpace(code)) throw new BusinessException(ErrorCodes.RequiredField);
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);

        var center = new CostCenter
        {
            TenantId = tenantId,
            BranchId = branchId,
            DepartmentId = departmentId,
            ParentCostCenterId = parentCostCenterId,
            Number = number,
            Code = code.Trim().ToUpperInvariant(),
            NameAr = nameAr.Trim(),
            NameEn = string.IsNullOrWhiteSpace(nameEn) ? null : nameEn.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            CostCenterType = type,
            Status = CostCenterStatus.Active,
            IsSystem = isSystem,
            SortOrder = sortOrder
        };

        center.RaiseDomainEvent(new CostCenterCreatedEvent(center.Id, tenantId, code));
        return center;
    }

    public void Update(
        string nameAr,
        string? nameEn,
        CostCenterType type,
        Guid? parentCostCenterId,
        Guid? departmentId,
        string? description,
        int sortOrder,
        bool useInPurchases,
        bool useInInventory,
        bool useInProduction,
        bool useInSales,
        bool useInPayroll,
        bool useInAssets,
        bool useInMaintenance,
        bool useInJournals)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        if (parentCostCenterId == Id)
            throw new BusinessException(ErrorCodes.CostCenterInvalidParent);

        NameAr = nameAr.Trim();
        NameEn = string.IsNullOrWhiteSpace(nameEn) ? null : nameEn.Trim();
        CostCenterType = type;
        ParentCostCenterId = parentCostCenterId;
        DepartmentId = departmentId;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        SortOrder = sortOrder;
        UseInPurchases = useInPurchases;
        UseInInventory = useInInventory;
        UseInProduction = useInProduction;
        UseInSales = useInSales;
        UseInPayroll = useInPayroll;
        UseInAssets = useInAssets;
        UseInMaintenance = useInMaintenance;
        UseInJournals = useInJournals;
    }

    public void Activate() => Status = CostCenterStatus.Active;
    public void Deactivate() => Status = CostCenterStatus.Inactive;

    public void EnsureCanDelete()
    {
        if (IsSystem)
            throw new BusinessException(ErrorCodes.CostCenterProtected);
    }

    public bool IsActive => Status == CostCenterStatus.Active;
}

/// <summary>حسابات مسموح بربطها بمركز التكلفة (Allowlist).</summary>
public sealed class CostCenterAllowedAccount
{
    public Guid CostCenterId { get; private set; }
    public Guid ChartOfAccountId { get; private set; }

    private CostCenterAllowedAccount() { }

    public static CostCenterAllowedAccount Create(Guid costCenterId, Guid chartOfAccountId)
        => new() { CostCenterId = costCenterId, ChartOfAccountId = chartOfAccountId };
}
