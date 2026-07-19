using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Sales.ProductPricing.Dtos;

public sealed record SalesPriceListDto(
    Guid Id,
    Guid TenantId,
    string Code,
    string NameAr,
    string? NameEn,
    string? Description,
    SalesChannel? DefaultSalesChannel,
    int SortOrder,
    bool IsDefault,
    bool IsSystem,
    bool IsActive,
    DateTime CreatedAt);

public sealed record CreateSalesPriceListRequest(
    Guid TenantId,
    string Code,
    string NameAr,
    string? NameEn = null,
    string? Description = null,
    SalesChannel? DefaultSalesChannel = null,
    int SortOrder = 0,
    bool IsDefault = false);

public sealed record UpdateSalesPriceListRequest(
    Guid TenantId,
    string NameAr,
    string? NameEn = null,
    string? Description = null,
    SalesChannel? DefaultSalesChannel = null,
    int SortOrder = 0,
    bool IsDefault = false,
    bool IsActive = true);

public sealed record ProductPriceDto(
    Guid Id,
    Guid TenantId,
    Guid ProductId,
    string? ProductNameAr,
    string? ProductSku,
    Guid? BranchId,
    string? BranchNameAr,
    Guid PriceListId,
    string? PriceListNameAr,
    SalesChannel SalesChannel,
    Guid UnitId,
    string? UnitNameAr,
    decimal UnitFactor,
    PricingMethod PricingMethod,
    ProductCostType CostType,
    decimal Cost,
    decimal ProfitMargin,
    decimal ProfitAmount,
    decimal SellingPrice,
    decimal? MinimumPrice,
    decimal? MaximumDiscount,
    DateTimeOffset StartDate,
    DateTimeOffset? EndDate,
    int Priority,
    Guid? CurrencyId,
    bool IsDefault,
    bool IsActive,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record ProductPriceUnitLineRequest(
    Guid UnitId,
    decimal UnitFactor,
    decimal Cost,
    decimal ProfitMargin,
    decimal ProfitAmount,
    decimal SellingPrice,
    decimal? MinimumPrice = null,
    bool Save = true);

public sealed record CreateProductPriceRequest(
    Guid TenantId,
    Guid ProductId,
    Guid PriceListId,
    Guid UnitId,
    PricingMethod PricingMethod,
    ProductCostType CostType,
    decimal Cost,
    decimal ProfitMargin,
    decimal ProfitAmount,
    decimal SellingPrice,
    DateTimeOffset StartDate,
    Guid? BranchId = null,
    SalesChannel SalesChannel = SalesChannel.All,
    decimal? MinimumPrice = null,
    decimal? MaximumDiscount = null,
    DateTimeOffset? EndDate = null,
    int Priority = 0,
    Guid? CurrencyId = null,
    bool IsDefault = false,
    string? Notes = null);

public sealed record CreateProductPricesBatchRequest(
    Guid TenantId,
    Guid ProductId,
    Guid PriceListId,
    PricingMethod PricingMethod,
    ProductCostType CostType,
    DateTimeOffset StartDate,
    IReadOnlyList<ProductPriceUnitLineRequest> Lines,
    Guid? BranchId = null,
    SalesChannel SalesChannel = SalesChannel.All,
    decimal? MaximumDiscount = null,
    DateTimeOffset? EndDate = null,
    int Priority = 0,
    Guid? CurrencyId = null,
    bool IsDefault = false,
    string? Notes = null);

public sealed record UpdateProductPriceRequest(
    Guid TenantId,
    Guid? BranchId,
    Guid PriceListId,
    SalesChannel SalesChannel,
    Guid UnitId,
    PricingMethod PricingMethod,
    ProductCostType CostType,
    decimal Cost,
    decimal ProfitMargin,
    decimal ProfitAmount,
    decimal SellingPrice,
    decimal? MinimumPrice,
    decimal? MaximumDiscount,
    DateTimeOffset StartDate,
    DateTimeOffset? EndDate,
    int Priority,
    Guid? CurrencyId,
    bool IsDefault,
    bool IsActive,
    string? Notes);

public sealed record CopyPriceListRequest(
    Guid TenantId,
    Guid SourcePriceListId,
    Guid TargetPriceListId,
    Guid? BranchId = null,
    DateTimeOffset? NewStartDate = null,
    bool DeactivateSource = false);

public sealed record ProductUnitPricingRowDto(
    Guid UnitId,
    string UnitNameAr,
    string? UnitNameEn,
    decimal Factor,
    decimal Cost,
    bool IsBaseUnit);
