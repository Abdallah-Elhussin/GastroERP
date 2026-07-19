using AutoMapper;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Domain.Entities.Inventory.Catalog;
using GastroErp.Domain.Entities.Inventory.Suppliers;
using GastroErp.Domain.Entities.Inventory.Warehouse;
using GastroErp.Domain.Entities.Inventory.Purchasing;
using GastroErp.Domain.Entities.Inventory.Counting;
using GastroErp.Domain.Entities.Inventory.Waste;
using GastroErp.Domain.Entities.Inventory.Recipe;
using GastroErp.Domain.Entities.Inventory.Transactions;
using GastroErp.Domain.Entities.Inventory.Settings;

namespace GastroErp.Application.Features.Inventory.Mapping;

public class InventoryMappingProfile : Profile
{
    public InventoryMappingProfile()
    {
        // Record CTOR mapping uses type converters; ForMember alone does not cover ctor params.
        CreateMap<DateTimeOffset, DateTime>().ConvertUsing(src => src.UtcDateTime);
        CreateMap<DateTimeOffset?, DateTime?>().ConvertUsing(src =>
            src.HasValue ? src.Value.UtcDateTime : null);

        // InventoryCategory
        CreateMap<InventoryCategory, InventoryCategoryDto>();

        // InventoryUnit
        CreateMap<InventoryUnit, InventoryUnitDto>();

        // UnitConversion
        CreateMap<UnitConversion, UnitConversionDto>()
            .ForMember(d => d.Factor, opt => opt.MapFrom(src => src.ConversionFactor))
            .ForMember(d => d.FromUnitNameAr, opt => opt.Ignore())
            .ForMember(d => d.ToUnitNameAr, opt => opt.Ignore());

        // InventoryItem
        CreateMap<InventoryItem, InventoryItemDto>()
            .ForMember(d => d.CategoryNameAr, opt => opt.Ignore())
            .ForMember(d => d.BaseUnitNameAr, opt => opt.Ignore());

        // Warehouse
        CreateMap<Warehouse, WarehouseDto>()
            .ForMember(d => d.ZoneCount, opt => opt.MapFrom(src => src.Zones.Count))
            .ForMember(d => d.WarehouseTypeNameAr, opt => opt.Ignore())
            .ForMember(d => d.ParentWarehouseNameAr, opt => opt.Ignore())
            .ForMember(d => d.BranchNameAr, opt => opt.Ignore());

        CreateMap<WarehouseTypeDefinition, WarehouseTypeDefinitionDto>();

        CreateMap<Warehouse, WarehouseDetailDto>()
            .ForMember(d => d.ZoneCount, opt => opt.MapFrom(src => src.Zones.Count))
            .ForMember(d => d.Zones, opt => opt.MapFrom(src => src.Zones));

        // WarehouseZone
        CreateMap<WarehouseZone, WarehouseZoneDto>()
            .ForMember(d => d.ShelfCount, opt => opt.MapFrom(src => src.Shelves.Count));

        CreateMap<WarehouseZone, WarehouseZoneDetailDto>()
            .ForMember(d => d.Shelves, opt => opt.MapFrom(src => src.Shelves));

        CreateMap<WarehouseShelf, WarehouseShelfDto>()
            .ForMember(d => d.ZoneId, opt => opt.MapFrom(src => src.WarehouseZoneId))
            .ForMember(d => d.BinCount, opt => opt.MapFrom(src => src.Bins.Count));

        CreateMap<WarehouseShelf, WarehouseShelfDetailDto>()
            .ForMember(d => d.ZoneId, opt => opt.MapFrom(src => src.WarehouseZoneId))
            .ForMember(d => d.Bins, opt => opt.MapFrom(src => src.Bins));

        CreateMap<WarehouseBin, WarehouseBinDto>()
            .ForMember(d => d.ShelfId, opt => opt.MapFrom(src => src.WarehouseShelfId));

        CreateMap<InventoryBrand, InventoryBrandDto>();
        CreateMap<InventoryManufacturer, InventoryManufacturerDto>();
        CreateMap<InventoryAttributeValue, InventoryAttributeValueDto>();
        CreateMap<InventoryAttribute, InventoryAttributeDto>()
            .ForMember(d => d.Values, opt => opt.MapFrom(src => src.Values.OrderBy(v => v.SortOrder)));
        CreateMap<InventoryPriceListLine, InventoryPriceListLineDto>()
            .ForMember(d => d.InventoryItemNameAr, opt => opt.Ignore());
        CreateMap<InventoryPriceList, InventoryPriceListDto>()
            .ForMember(d => d.LineCount, opt => opt.MapFrom(src => src.Lines.Count))
            .ForMember(d => d.Lines, opt => opt.MapFrom(src => src.Lines));

        // Supplier — mapped manually via SupplierMapper
        CreateMap<SupplierContact, SupplierContactDto>();
        CreateMap<SupplierPaymentMethod, SupplierPaymentMethodDto>();
        CreateMap<SupplierAttachment, SupplierAttachmentDto>();

        // PurchaseOrder — mapped via PurchaseOrderMapper

// GoodsReceipt — mapped manually via GoodsReceiptMapper

        // GoodsIssue — mapped manually via GoodsIssueMapper
        CreateMap<GastroErp.Domain.Entities.Inventory.Issuing.GoodsIssue, GoodsIssueDto>()
            .ForMember(d => d.WarehouseNameAr, opt => opt.Ignore())
            .ForMember(d => d.IssueDestinationNameAr, opt => opt.Ignore())
            .ForMember(d => d.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(d => d.StatusCode, opt => opt.MapFrom(src => (byte)src.Status))
            .ForMember(d => d.Lines, opt => opt.Ignore())
            .ForMember(d => d.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
            .ForMember(d => d.LineCount, opt => opt.MapFrom(src => src.Lines.Count));

        // OpeningBalance — mapped manually via OpeningBalanceMapper
        CreateMap<GastroErp.Domain.Entities.Inventory.Opening.OpeningBalance, OpeningBalanceDto>()
            .ForMember(d => d.WarehouseNameAr, opt => opt.Ignore())
            .ForMember(d => d.ContraAccountName, opt => opt.Ignore())
            .ForMember(d => d.CostCenterNameAr, opt => opt.Ignore())
            .ForMember(d => d.Lines, opt => opt.Ignore())
            .ForMember(d => d.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(d => d.StatusCode, opt => opt.MapFrom(src => (byte)src.Status))
            .ForMember(d => d.EntryMethod, opt => opt.MapFrom(src => src.EntryMethod.ToString()))
            .ForMember(d => d.DisplayMethod, opt => opt.MapFrom(src => src.DisplayMethod.ToString()))
            .ForMember(d => d.CostingMethod, opt => opt.MapFrom(src => src.CostingMethod.ToString()))
            .ForMember(d => d.WeightedAverageScope, opt => opt.MapFrom(src => src.WeightedAverageScope.ToString()))
            .ForMember(d => d.LineCount, opt => opt.MapFrom(src => src.Lines.Count));

        // StockTransfer — mapped manually via StockTransferMapper
        CreateMap<StockTransfer, StockTransferDto>()
            .ForMember(d => d.SourceWarehouseNameAr, opt => opt.Ignore())
            .ForMember(d => d.DestinationWarehouseNameAr, opt => opt.Ignore())
            .ForMember(d => d.TransferType, opt => opt.MapFrom(src => src.TransferType.ToString()))
            .ForMember(d => d.TransferTypeCode, opt => opt.MapFrom(src => (byte)src.TransferType))
            .ForMember(d => d.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(d => d.StatusCode, opt => opt.MapFrom(src => (byte)src.Status))
            .ForMember(d => d.Lines, opt => opt.Ignore())
            .ForMember(d => d.LineCount, opt => opt.MapFrom(src => src.Lines.Count))
            .ForMember(d => d.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount));

        // StockAdjustment
        CreateMap<StockAdjustment, StockAdjustmentDto>()
            .ForMember(d => d.WarehouseNameAr, opt => opt.Ignore())
            .ForMember(d => d.InventoryItemId, opt => opt.Ignore())
            .ForMember(d => d.ItemNameAr, opt => opt.Ignore())
            .ForMember(d => d.QuantityAdjusted, opt => opt.Ignore())
            .ForMember(d => d.ReasonId, opt => opt.Ignore());

        // WasteRecord
        CreateMap<WasteRecord, WasteRecordDto>()
            .ForMember(d => d.WarehouseNameAr, opt => opt.Ignore())
            .ForMember(d => d.InventoryItemId, opt => opt.Ignore())
            .ForMember(d => d.ItemNameAr, opt => opt.Ignore())
            .ForMember(d => d.Quantity, opt => opt.Ignore())
            .ForMember(d => d.UnitCost, opt => opt.Ignore())
            .ForMember(d => d.ReasonId, opt => opt.Ignore())
            .ForMember(d => d.WasteNumber, opt => opt.MapFrom(src => src.RecordNumber));

        // Recipe
        CreateMap<Recipe, RecipeDto>()
            .ForMember(d => d.ProductNameAr, opt => opt.Ignore())
            .ForMember(d => d.IsActive, opt => opt.MapFrom(src => src.Status == GastroErp.Domain.Enums.RecipeStatus.Active))
            .ForMember(d => d.Yield, opt => opt.MapFrom(src => (int)src.Yield))
            .ForMember(d => d.IngredientCount, opt => opt.MapFrom(src => src.Items.Count));

        // RecipeItem
        CreateMap<RecipeItem, RecipeIngredientDto>()
            .ForMember(d => d.ItemNameAr, opt => opt.Ignore())
            .ForMember(d => d.UnitNameAr, opt => opt.Ignore())
            .ForMember(d => d.InventoryItemId, opt => opt.MapFrom(src => src.InventoryItemId));

        // InventoryTransaction
        CreateMap<InventoryTransaction, InventoryTransactionDto>()
            .ForMember(d => d.TransactionType, opt => opt.MapFrom(src => src.TransactionType.ToString()))
            .ForMember(d => d.MovementCount, opt => opt.MapFrom(src => src.Movements.Count));

        // StockCount
        CreateMap<StockCount, StockCountDto>()
            .ForMember(d => d.Lines, opt => opt.MapFrom(src => src.Lines));

        // StockCountLine
        CreateMap<StockCountLine, StockCountLineDto>()
            .ForMember(d => d.InventoryItemNameAr, opt => opt.Ignore())
            .ForMember(d => d.UnitNameAr, opt => opt.Ignore());

        // PurchaseReturn — mapped manually via PurchaseReturnMapper

        // InventoryReservation
        CreateMap<global::GastroErp.Domain.Entities.Inventory.Reservation.InventoryReservation, InventoryReservationDto>()
            .ForMember(d => d.InventoryItemNameAr, opt => opt.Ignore());

        // InventorySetting — mapped manually in SettingsHandlers (expanded DTO).
    }
}
