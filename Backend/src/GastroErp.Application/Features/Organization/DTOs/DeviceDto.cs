using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Organization.DTOs;

public record DeviceDto(
    Guid Id,
    Guid TenantId,
    string NameAr,
    string? NameEn,
    DeviceType DeviceType,
    string? SerialNumber,
    string? MacAddress,
    string ActivationCode,
    bool IsActivated,
    bool IsOnline,
    bool IsActive,
    DateTimeOffset? ActivatedAt,
    DateTimeOffset? LastSyncAt,
    DateTime CreatedAt
);

public record CreateDeviceDto(
    Guid TenantId,
    string NameAr,
    string? NameEn,
    DeviceType DeviceType,
    string? SerialNumber = null,
    string? MacAddress = null
);

public record UpdateDeviceDto(
    string NameAr,
    string? NameEn
);

public record LinkBranchDeviceDto(Guid DeviceId, Guid BranchId);
