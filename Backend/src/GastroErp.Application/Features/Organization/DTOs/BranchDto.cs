using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Organization.DTOs;

public record BranchDto(
    Guid Id,
    Guid TenantId,
    Guid CompanyId,
    string NameAr,
    string? NameEn,
    string? Code,
    BranchType BranchType,
    BranchStatus Status,
    string? PhoneNumber,
    string? Email,
    string? AddressStreetAr,
    string? AddressStreetEn,
    string? CityAr,
    string? CityEn,
    bool AllowNegativeStock,
    bool AllowOfflineSales,
    DateTime CreatedAt
);

public record CreateBranchDto(
    Guid TenantId,
    Guid CompanyId,
    string NameAr,
    string? NameEn,
    BranchType BranchType,
    string? Code = null
);

public record UpdateBranchDto(
    string? PhoneNumber,
    string? Email,
    bool AllowNegativeStock,
    bool AllowOfflineSales
);
