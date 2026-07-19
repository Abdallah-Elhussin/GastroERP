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
    DateTime CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? CompanyNameAr,
    bool IsActive);

public record CreateBranchDto(
    Guid CompanyId,
    string NameAr,
    Guid? TenantId = null,
    string? NameEn = null,
    BranchType BranchType = BranchType.Restaurant,
    string? Code = null,
    string? Location = null,
    bool IsActive = true);

public record UpdateBranchDto(
    Guid CompanyId,
    string NameAr,
    string? NameEn = null,
    string? Code = null,
    string? Location = null,
    bool IsActive = true,
    string? PhoneNumber = null,
    string? Email = null,
    bool AllowNegativeStock = false,
    bool AllowOfflineSales = true);
