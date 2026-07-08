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

namespace GastroErp.Application.Features.Inventory.Mapping;

public class InventoryMappingProfile : Profile
{
    public InventoryMappingProfile()
    {
        // InventoryCategory
        CreateMap<InventoryCategory, InventoryCategoryDto>()
            .ForMember(d => d.Color, opt => opt.Ignore()); // Color not in entity, handled via description

        // InventoryUnit
        CreateMap<InventoryUnit, InventoryUnitDto>()
            .ForMember(d => d.Symbol, opt => opt.MapFrom(src => src.Symbol));

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
            .ForMember(d => d.ZoneCount, opt => opt.MapFrom(src => src.Zones.Count));

        // WarehouseZone
        CreateMap<WarehouseZone, WarehouseZoneDto>()
            .ForMember(d => d.ShelfCount, opt => opt.MapFrom(src => src.Shelves.Count));

        // Supplier
        CreateMap<Supplier, SupplierDto>()
            .ForMember(d => d.ContactCount, opt => opt.MapFrom(src => src.Contacts.Count));

        // SupplierContact
        CreateMap<SupplierContact, SupplierContactDto>();

        // PurchaseOrder
        CreateMap<PurchaseOrder, PurchaseOrderDto>()
            .ForMember(d => d.SupplierNameAr, opt => opt.Ignore())
            .ForMember(d => d.WarehouseNameAr, opt => opt.Ignore())
            .ForMember(d => d.LineCount, opt => opt.MapFrom(src => src.Lines.Count));

        // PurchaseOrderLine
        CreateMap<PurchaseOrderLine, PurchaseOrderLineDto>()
            .ForMember(d => d.ItemNameAr, opt => opt.Ignore())
            .ForMember(d => d.UnitNameAr, opt => opt.Ignore());

        // GoodsReceipt
        CreateMap<GoodsReceipt, GoodsReceiptDto>()
            .ForMember(d => d.PoNumber, opt => opt.Ignore())
            .ForMember(d => d.WarehouseNameAr, opt => opt.Ignore())
            .ForMember(d => d.GrnNumber, opt => opt.MapFrom(src => src.ReceiptNumber))
            .ForMember(d => d.LineCount, opt => opt.MapFrom(src => src.Lines.Count));

        // StockTransfer
        CreateMap<StockTransfer, StockTransferDto>()
            .ForMember(d => d.SourceWarehouseNameAr, opt => opt.Ignore())
            .ForMember(d => d.DestinationWarehouseNameAr, opt => opt.Ignore())
            .ForMember(d => d.Status, opt => opt.MapFrom(src => src.Status.ToString()));

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

        // PurchaseReturn
        CreateMap<PurchaseReturn, PurchaseReturnDto>()
            .ForMember(d => d.Lines, opt => opt.MapFrom(src => src.Lines));

        // PurchaseReturnLine
        CreateMap<PurchaseReturnLine, PurchaseReturnLineDto>()
            .ForMember(d => d.InventoryItemNameAr, opt => opt.Ignore())
            .ForMember(d => d.UnitNameAr, opt => opt.Ignore());

        // InventoryReservation
        CreateMap<global::GastroErp.Domain.Entities.Inventory.Reservation.InventoryReservation, InventoryReservationDto>()
            .ForMember(d => d.InventoryItemNameAr, opt => opt.Ignore());
    }
}
