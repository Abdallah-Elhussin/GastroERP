using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Organization;

/// <summary>
/// Department — القسم (Aggregate Root)
/// تقسيم تنظيمي داخل الشركة أو الفرع (مثل: مطبخ، خدمة، حسابات).
/// يدعم التدرج الهرمي (Parent Department).
/// </summary>
public sealed class Department : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid? BranchId { get; private set; }
    public Guid? ParentDepartmentId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string? Code { get; private set; }
    public bool IsActive { get; private set; }

    private Department()
    {
        NameAr = string.Empty;
    }

    public Department(Guid tenantId, Guid companyId, string nameAr,
                      Guid? branchId = null, Guid? parentDepartmentId = null,
                      string? nameEn = null, string? code = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (companyId == Guid.Empty) throw new ArgumentException("CompanyId cannot be empty.", nameof(companyId));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);

        TenantId = tenantId;
        CompanyId = companyId;
        BranchId = branchId;
        ParentDepartmentId = parentDepartmentId;
        NameAr = nameAr;
        NameEn = nameEn;
        Code = code;
        IsActive = true;
    }

    public void UpdateName(string nameAr, string? nameEn)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        NameAr = nameAr;
        NameEn = nameEn;
    }

    public void AssignToBranch(Guid branchId)
    {
        if (branchId == Guid.Empty) throw new ArgumentException("BranchId cannot be empty.", nameof(branchId));
        BranchId = branchId;
    }

    public void SetParent(Guid? parentId)
    {
        if (parentId == Id) throw new InvalidOperationException("A department cannot be its own parent.");
        ParentDepartmentId = parentId;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
