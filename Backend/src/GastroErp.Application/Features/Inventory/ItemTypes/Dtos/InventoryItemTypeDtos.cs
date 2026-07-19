using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Inventory.ItemTypes.Dtos;

public sealed record InventoryItemTypeDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string? NameEn { get; init; }
    public string? Description { get; init; }
    public InventoryItemTypeCategory Category { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public int? CodeStart { get; init; }
    public int? CodeEnd { get; init; }
    public bool IsInventory { get; init; }
    public bool CanSell { get; init; }
    public bool CanPurchase { get; init; }
    public bool IsRecipe { get; init; }
    public bool IsProduction { get; init; }
    public bool AllowNegativeStock { get; init; }
    public string Color { get; init; } = "#FFFFFF";
    public int SortOrder { get; init; }
    public bool IsSystem { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public sealed record CreateInventoryItemTypeRequest
{
    public Guid TenantId { get; init; }
    public Guid? CompanyId { get; init; }
    public required string Code { get; init; }
    public required string NameAr { get; init; }
    public string? NameEn { get; init; }
    public string? Description { get; init; }
    public InventoryItemTypeCategory Category { get; init; }
    public int? CodeStart { get; init; }
    public int? CodeEnd { get; init; }
    public bool IsInventory { get; init; }
    public bool CanSell { get; init; }
    public bool CanPurchase { get; init; }
    public bool IsRecipe { get; init; }
    public bool IsProduction { get; init; }
    public bool AllowNegativeStock { get; init; }
    public string? Color { get; init; }
    public int SortOrder { get; init; }
}

public sealed record UpdateInventoryItemTypeRequest
{
    public Guid TenantId { get; init; }
    public required string NameAr { get; init; }
    public string? NameEn { get; init; }
    public string? Description { get; init; }
    public InventoryItemTypeCategory Category { get; init; }
    public int? CodeStart { get; init; }
    public int? CodeEnd { get; init; }
    public bool IsInventory { get; init; }
    public bool CanSell { get; init; }
    public bool CanPurchase { get; init; }
    public bool IsRecipe { get; init; }
    public bool IsProduction { get; init; }
    public bool AllowNegativeStock { get; init; }
    public string? Color { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; } = true;
}
