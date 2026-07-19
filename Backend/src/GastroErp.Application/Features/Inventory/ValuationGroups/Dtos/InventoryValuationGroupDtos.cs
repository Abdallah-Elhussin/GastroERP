namespace GastroErp.Application.Features.Inventory.ValuationGroups.Dtos;

public sealed record InventoryValuationGroupDto(
    Guid Id,
    Guid TenantId,
    string Code,
    string NameAr,
    string? NameEn,
    string? Description,
    Guid? CostCenterId,
    string? CostCenterNameAr,
    int SortOrder,
    bool IsSystem,
    bool IsActive,
    DateTime CreatedAt);

public sealed record CreateInventoryValuationGroupRequest(
    Guid TenantId,
    string Code,
    string NameAr,
    string? NameEn = null,
    string? Description = null,
    Guid? CostCenterId = null,
    int SortOrder = 0);

public sealed record UpdateInventoryValuationGroupRequest(
    Guid TenantId,
    string NameAr,
    string? NameEn = null,
    string? Description = null,
    Guid? CostCenterId = null,
    int SortOrder = 0,
    bool IsActive = true);
