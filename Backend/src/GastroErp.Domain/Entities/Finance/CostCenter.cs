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
    public string Code { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public CostCenterStatus Status { get; private set; }

    private CostCenter()
    {
        Code = string.Empty;
        NameAr = string.Empty;
    }

    public static CostCenter Create(
        Guid tenantId, Guid branchId, string code, string nameAr,
        Guid? departmentId = null, string? nameEn = null)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new BusinessException(ErrorCodes.RequiredField);
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);

        var center = new CostCenter
        {
            TenantId = tenantId,
            BranchId = branchId,
            DepartmentId = departmentId,
            Code = code.ToUpperInvariant(),
            NameAr = nameAr,
            NameEn = nameEn,
            Status = CostCenterStatus.Active
        };

        center.RaiseDomainEvent(new CostCenterCreatedEvent(center.Id, tenantId, code));
        return center;
    }

    public void Update(string nameAr, string? nameEn, Guid? departmentId)
    {
        NameAr = nameAr;
        NameEn = nameEn;
        DepartmentId = departmentId;
    }

    public void Activate() => Status = CostCenterStatus.Active;
    public void Deactivate() => Status = CostCenterStatus.Inactive;
}
