using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Domain.Enums;
using MediatR;

namespace GastroErp.Application.Features.Inventory.Queries;

// ─── InventoryCategory ────────────────────────────────────────────────────────
public record GetInventoryCategoriesQuery(Guid TenantId, bool? IsActive = null, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<InventoryCategoryDto>>;
public record GetInventoryCategoryByIdQuery(Guid Id) : IRequest<Result<InventoryCategoryDto>>;

// ─── InventoryUnit ────────────────────────────────────────────────────────────
public record GetInventoryUnitsQuery(Guid TenantId, bool? IsActive = null) : IRequest<Result<List<InventoryUnitDto>>>;

// ─── InventoryItem ────────────────────────────────────────────────────────────
public record GetInventoryItemsQuery(Guid TenantId, Guid? CategoryId = null, bool? IsActive = null, string? SearchTerm = null, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<InventoryItemDto>>;
public record GetInventoryItemByIdQuery(Guid Id) : IRequest<Result<InventoryItemDto>>;
public record GetLowStockItemsQuery(Guid TenantId, Guid? WarehouseId = null) : IRequest<Result<List<InventoryItemDto>>>;

// ─── Product Details (Phase D) ────────────────────────────────────────────────
public record GetInventoryItemStockByWarehouseQuery(Guid InventoryItemId) : IRequest<Result<List<WarehouseStockBalanceDto>>>;
public record GetInventoryItemMovementsQuery(Guid InventoryItemId, int PageNumber = 1, int PageSize = 50) : IRequest<PagedResult<ItemStockMovementDto>>;
public record GetInventoryItemPurchaseHistoryQuery(Guid InventoryItemId, int PageNumber = 1, int PageSize = 50) : IRequest<PagedResult<ItemPurchaseHistoryDto>>;
public record GetInventoryItemSalesHistoryQuery(Guid InventoryItemId, int PageNumber = 1, int PageSize = 50) : IRequest<PagedResult<ItemSalesHistoryDto>>;

// ─── Warehouse ────────────────────────────────────────────────────────────────
public record GetWarehousesQuery(
    Guid TenantId,
    Guid? BranchId = null,
    bool? IsActive = null,
    WarehouseType? WarehouseType = null,
    Guid? WarehouseTypeId = null,
    bool? IsPosWarehouse = null,
    bool? IsDefault = null,
    string? SearchTerm = null,
    string? SortBy = null,
    bool SortDesc = false,
    int PageNumber = 1,
    int PageSize = 50) : IRequest<PagedResult<WarehouseDto>>;
public record GetWarehouseByIdQuery(Guid Id) : IRequest<Result<WarehouseDetailDto>>;
public record GetWarehouseLookupQuery(Guid TenantId, Guid? BranchId = null, bool ActiveOnly = true) : IRequest<Result<List<WarehouseDto>>>;
public record GetWarehouseTypeDefinitionsQuery(Guid TenantId, bool? IsActive = null) : IRequest<Result<List<WarehouseTypeDefinitionDto>>>;

// ─── Phase J Master Data ──────────────────────────────────────────────────────
public record GetInventoryBrandsQuery(Guid TenantId, bool? IsActive = null) : IRequest<Result<List<InventoryBrandDto>>>;
public record GetInventoryManufacturersQuery(Guid TenantId, bool? IsActive = null) : IRequest<Result<List<InventoryManufacturerDto>>>;
public record GetInventoryAttributesQuery(Guid TenantId, bool? IsActive = null) : IRequest<Result<List<InventoryAttributeDto>>>;
public record GetInventoryAttributeByIdQuery(Guid Id) : IRequest<Result<InventoryAttributeDto>>;
public record GetInventoryPriceListsQuery(Guid TenantId, bool? IsActive = null) : IRequest<Result<List<InventoryPriceListDto>>>;
public record GetInventoryPriceListByIdQuery(Guid Id) : IRequest<Result<InventoryPriceListDto>>;

// ─── Supplier ─────────────────────────────────────────────────────────────────
public record GetSuppliersQuery(
    Guid TenantId,
    bool? IsActive = null,
    bool? IsPreferred = null,
    bool? IsBlacklisted = null,
    bool? HasBalance = null,
    bool? OverCreditLimit = null,
    SupplierCategory? Category = null,
    string? City = null,
    string? Country = null,
    string? Search = null,
    string? Code = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<SupplierListItemDto>>;
public record GetSupplierByIdQuery(Guid Id, bool IncludeStats = true) : IRequest<Result<SupplierDto>>;
public record GetSupplierPurchasingDefaultsQuery(Guid Id) : IRequest<Result<SupplierPurchasingDefaultsDto>>;
public record GetNextSupplierCodeQuery(Guid TenantId) : IRequest<Result<string>>;

// ─── PurchaseOrder ────────────────────────────────────────────────────────────
// ─── PurchaseOrder Queries ────────────────────────────────────────────────────
public record GetPurchaseOrdersQuery(
    Guid TenantId,
    Guid? SupplierId = null,
    PurchaseOrderStatus? Status = null,
    Guid? WarehouseId = null,
    string? Search = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    int PageNumber = 1,
    int PageSize = 50) : IRequest<PagedResult<PurchaseOrderDto>>;

public record GetPurchaseOrderByIdQuery(Guid Id) : IRequest<Result<PurchaseOrderDto>>;

public record GetPurchaseOrderDashboardQuery(Guid TenantId) : IRequest<Result<PurchaseOrderDashboardDto>>;

// ─── GoodsReceipt ─────────────────────────────────────────────────────────────
public record GetGoodsReceiptsQuery(
    Guid TenantId,
    Guid? SupplierId = null,
    GoodsReceiptStatus? Status = null,
    string? Search = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<GoodsReceiptDto>>;
public record GetGoodsReceiptByIdQuery(Guid Id) : IRequest<Result<GoodsReceiptDto>>;
public record PreviewGoodsReceiptFromPoQuery(Guid TenantId, Guid PurchaseOrderId) : IRequest<Result<GoodsReceiptDto>>;
public record GetNextGoodsReceiptNumberQuery(Guid TenantId) : IRequest<Result<string>>;

// ─── PurchaseInvoice ──────────────────────────────────────────────────────────
public record GetPurchaseInvoicesQuery(
    Guid TenantId,
    PurchaseInvoiceKind? Kind = null,
    PurchasingDocumentStatus? Status = null,
    Guid? SupplierId = null,
    Guid? WarehouseId = null,
    PurchaseInvoicePaymentMode? PaymentMode = null,
    DirectPurchaseNature? Nature = null,
    string? Search = null,
    DateOnly? From = null,
    DateOnly? To = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<PurchaseInvoiceDto>>;
public record GetPurchaseInvoiceByIdQuery(Guid Id) : IRequest<Result<PurchaseInvoiceDto>>;
public record GetNextPurchaseInvoiceNumberQuery(
    Guid TenantId,
    PurchaseInvoiceKind Kind = PurchaseInvoiceKind.FromReceipt) : IRequest<Result<string>>;

// ─── GoodsIssue ───────────────────────────────────────────────────────────────
public record GetGoodsIssuesQuery(
    Guid TenantId,
    byte? Status = null,
    string? Search = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<GoodsIssueDto>>;
public record GetGoodsIssueByIdQuery(Guid Id) : IRequest<Result<GoodsIssueDto>>;

// ─── IssueDestination ─────────────────────────────────────────────────────────
public record GetIssueDestinationsQuery(
    Guid TenantId,
    bool ActiveOnly = false,
    string? Search = null,
    byte? DestinationType = null) : IRequest<Result<IReadOnlyList<IssueDestinationDto>>>;
public record GetIssueDestinationByIdQuery(Guid Id) : IRequest<Result<IssueDestinationDto>>;

// ─── OpeningBalance ───────────────────────────────────────────────────────────
public record GetOpeningBalancesQuery(Guid TenantId, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<OpeningBalanceDto>>;
public record GetOpeningBalanceByIdQuery(Guid Id) : IRequest<Result<OpeningBalanceDto>>;

// ─── StockTransfer ────────────────────────────────────────────────────────────
public record GetStockTransfersQuery(
    Guid TenantId,
    Guid? SourceWarehouseId = null,
    Guid? DestinationWarehouseId = null,
    byte? Status = null,
    string? Search = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<StockTransferDto>>;
public record GetStockTransferByIdQuery(Guid Id) : IRequest<Result<StockTransferDto>>;

// ─── StockAdjustment ─────────────────────────────────────────────────────────
public record GetStockAdjustmentsQuery(Guid TenantId, Guid? WarehouseId = null, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<StockAdjustmentDto>>;

// ─── WasteRecord ──────────────────────────────────────────────────────────────
public record GetWasteRecordsQuery(Guid TenantId, Guid? WarehouseId = null, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<WasteRecordDto>>;

// ─── Recipe ───────────────────────────────────────────────────────────────────
public record GetRecipesQuery(Guid TenantId, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<RecipeDto>>;
public record GetRecipesByProductIdQuery(Guid ProductId) : IRequest<Result<List<RecipeDto>>>;
public record GetRecipeByIdQuery(Guid Id) : IRequest<Result<RecipeDto>>;

// ─── InventoryTransaction ─────────────────────────────────────────────────────
public record GetInventoryTransactionsQuery(Guid TenantId, Guid? WarehouseId = null, int PageNumber = 1, int PageSize = 50) : IRequest<PagedResult<InventoryTransactionDto>>;

// ─── Dashboard (Phase F) ──────────────────────────────────────────────────────
public record GetInventoryDashboardQuery(Guid TenantId) : IRequest<Result<InventoryDashboardDto>>;
public record GetPurchasingDashboardQuery(Guid TenantId) : IRequest<Result<PurchasingDashboardDto>>;

// ─── Inventory Settings (Phase I) ─────────────────────────────────────────────
public record GetInventorySettingQuery(Guid TenantId, Guid? BranchId = null, Guid? CompanyId = null)
    : IRequest<Result<InventorySettingDto>>;
public record GetInventorySettingsByCompanyQuery(Guid TenantId, Guid CompanyId)
    : IRequest<Result<InventorySettingDto>>;

// ─── StockCount ───────────────────────────────────────────────────────────────
public record GetStockCountsQuery(Guid TenantId, Guid? WarehouseId = null, StockCountStatus? Status = null, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<StockCountDto>>;
public record GetStockCountByIdQuery(Guid Id) : IRequest<Result<StockCountDto>>;

// ─── PurchaseReturn ───────────────────────────────────────────────────────────
public record GetPurchaseReturnsQuery(
    Guid TenantId,
    Guid? SupplierId = null,
    Guid? WarehouseId = null,
    PurchaseReturnType? ReturnType = null,
    /// <summary>When true, only AfterInvoice + Direct (invoice-based) returns.</summary>
    bool InvoiceBasedOnly = false,
    PurchasingDocumentStatus? Status = null,
    string? Search = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<PurchaseReturnDto>>;
public record GetPurchaseReturnByIdQuery(Guid Id) : IRequest<Result<PurchaseReturnDto>>;
public record GetNextPurchaseReturnNumberQuery(Guid TenantId) : IRequest<Result<string>>;
public record PreviewPurchaseReturnFromGrnQuery(Guid TenantId, Guid GoodsReceiptId) : IRequest<Result<PurchaseReturnDto>>;
public record PreviewPurchaseReturnFromInvoiceQuery(Guid TenantId, Guid PurchaseInvoiceId) : IRequest<Result<PurchaseReturnDto>>;
/// <summary>Loads purchase invoice header + lines + return quantities in one query for the return form.</summary>
public record GetPurchaseInvoiceForReturnQuery(Guid TenantId, Guid PurchaseInvoiceId)
    : IRequest<Result<PurchaseInvoiceForReturnDto>>;
public record GetPurchaseReturnReasonsQuery(Guid TenantId, bool ActiveOnly = true) : IRequest<Result<IReadOnlyList<PurchaseReturnReasonDto>>>;

// ─── InventoryReservation ─────────────────────────────────────────────────────
public record GetInventoryReservationsQuery(Guid TenantId, Guid? WarehouseId = null, Guid? InventoryItemId = null, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<InventoryReservationDto>>;
