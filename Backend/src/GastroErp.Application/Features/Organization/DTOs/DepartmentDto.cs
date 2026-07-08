namespace GastroErp.Application.Features.Organization.DTOs;

public record DepartmentDto(
    Guid Id,
    Guid TenantId,
    Guid CompanyId,
    Guid? BranchId,
    Guid? ParentDepartmentId,
    string NameAr,
    string? NameEn,
    string? Code,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateDepartmentDto(
    Guid TenantId,
    Guid CompanyId,
    string NameAr,
    string? NameEn = null,
    Guid? BranchId = null,
    Guid? ParentDepartmentId = null,
    string? Code = null
);

public record UpdateDepartmentDto(
    string NameAr,
    string? NameEn
);
