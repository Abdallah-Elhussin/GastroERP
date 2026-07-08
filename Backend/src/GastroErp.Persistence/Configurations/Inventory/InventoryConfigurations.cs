using GastroErp.Domain.Entities.Inventory.Catalog;
using GastroErp.Domain.Entities.Inventory.Counting;
using GastroErp.Domain.Entities.Inventory.Purchasing;
using GastroErp.Domain.Entities.Inventory.Recipe;
using GastroErp.Domain.Entities.Inventory.Reservation;
using GastroErp.Domain.Entities.Inventory.Settings;
using GastroErp.Domain.Entities.Inventory.Suppliers;
using GastroErp.Domain.Entities.Inventory.Transactions;
using GastroErp.Domain.Entities.Inventory.Warehouse;
using GastroErp.Domain.Entities.Inventory.Waste;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastroErp.Persistence.Configurations.Inventory;

// ─── Settings ─────────────────────────────────────────────────────────────
public sealed class InventorySettingConfiguration : IEntityTypeConfiguration<InventorySetting>
{
    public void Configure(EntityTypeBuilder<InventorySetting> builder)
    {
        builder.ToTable("InventorySettings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CostingMethod).HasConversion<byte>();
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.TenantId, x.BranchId }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

// ─── Catalog ──────────────────────────────────────────────────────────────
public sealed class InventoryCategoryConfiguration : IEntityTypeConfiguration<InventoryCategory>
{
    public void Configure(EntityTypeBuilder<InventoryCategory> builder)
    {
        builder.ToTable("InventoryCategories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.DescriptionAr).HasMaxLength(500);
        builder.Property(x => x.DescriptionEn).HasMaxLength(500);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasOne(x => x.ParentCategory).WithMany(x => x.SubCategories).HasForeignKey(x => x.ParentCategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.NameAr }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class InventoryUnitConfiguration : IEntityTypeConfiguration<InventoryUnit>
{
    public void Configure(EntityTypeBuilder<InventoryUnit> builder)
    {
        builder.ToTable("InventoryUnits");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NameEn).HasMaxLength(100);
        builder.Property(x => x.Symbol).IsRequired().HasMaxLength(20);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.Symbol }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class UnitConversionConfiguration : IEntityTypeConfiguration<UnitConversion>
{
    public void Configure(EntityTypeBuilder<UnitConversion> builder)
    {
        builder.ToTable("UnitConversions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ConversionFactor).HasPrecision(18, 6);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.TenantId, x.FromUnitId, x.ToUnitId }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("InventoryItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.DescriptionAr).HasMaxLength(500);
        builder.Property(x => x.DescriptionEn).HasMaxLength(500);
        builder.Property(x => x.Sku).HasMaxLength(50);
        builder.Property(x => x.Barcode).HasMaxLength(50);
        builder.Property(x => x.ReorderLevel).HasPrecision(18, 4);
        builder.Property(x => x.ReorderQuantity).HasPrecision(18, 4);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.CategoryId).HasFilter("[IsDeleted] = 0");
    }
}

// ─── Recipe ───────────────────────────────────────────────────────────────
public sealed class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.ToTable("Recipes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.Yield).HasPrecision(18, 4);
        builder.Property(x => x.WastePercentage).HasPrecision(5, 2);
        builder.Property(x => x.Status).HasConversion<byte>();
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.RecipeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.ProductId).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class RecipeItemConfiguration : IEntityTypeConfiguration<RecipeItem>
{
    public void Configure(EntityTypeBuilder<RecipeItem> builder)
    {
        builder.ToTable("RecipeItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.WastePercentage).HasPrecision(5, 2);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

// ─── Warehouse ────────────────────────────────────────────────────────────
public sealed class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.Code).HasMaxLength(50);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasMany(x => x.Zones).WithOne().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0 AND [Code] IS NOT NULL");
    }
}

public sealed class WarehouseZoneConfiguration : IEntityTypeConfiguration<WarehouseZone>
{
    public void Configure(EntityTypeBuilder<WarehouseZone> builder)
    {
        builder.ToTable("WarehouseZones");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NameEn).HasMaxLength(100);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasMany(x => x.Shelves).WithOne().HasForeignKey(x => x.WarehouseZoneId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class WarehouseShelfConfiguration : IEntityTypeConfiguration<WarehouseShelf>
{
    public void Configure(EntityTypeBuilder<WarehouseShelf> builder)
    {
        builder.ToTable("WarehouseShelves");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NameEn).HasMaxLength(100);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasMany(x => x.Bins).WithOne().HasForeignKey(x => x.WarehouseShelfId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class WarehouseBinConfiguration : IEntityTypeConfiguration<WarehouseBin>
{
    public void Configure(EntityTypeBuilder<WarehouseBin> builder)
    {
        builder.ToTable("WarehouseBins");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NameEn).HasMaxLength(100);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

// ─── Suppliers ────────────────────────────────────────────────────────────
public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.TaxNumber).HasMaxLength(50);
        builder.Property(x => x.Currency).HasMaxLength(3);
        builder.Property(x => x.CreditLimit).HasPrecision(18, 4);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasMany(x => x.Contacts).WithOne().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.TaxNumber }).IsUnique().HasFilter("[IsDeleted] = 0 AND [TaxNumber] IS NOT NULL");
    }
}

public sealed class SupplierContactConfiguration : IEntityTypeConfiguration<SupplierContact>
{
    public void Configure(EntityTypeBuilder<SupplierContact> builder)
    {
        builder.ToTable("SupplierContacts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(50);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

// ─── Purchasing ───────────────────────────────────────────────────────────
public sealed class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PoNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Status).HasConversion<byte>();
        builder.Property(x => x.TotalAmount).HasPrecision(18, 4);
        builder.Property(x => x.Currency).HasMaxLength(3);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.PurchaseOrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.SupplierId).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class PurchaseOrderLineConfiguration : IEntityTypeConfiguration<PurchaseOrderLine>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderLine> builder)
    {
        builder.ToTable("PurchaseOrderLines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.ReceivedQuantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 4);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 4);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class GoodsReceiptConfiguration : IEntityTypeConfiguration<GoodsReceipt>
{
    public void Configure(EntityTypeBuilder<GoodsReceipt> builder)
    {
        builder.ToTable("GoodsReceipts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ReceiptNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Status).HasConversion<byte>();
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.GoodsReceiptId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class GoodsReceiptLineConfiguration : IEntityTypeConfiguration<GoodsReceiptLine>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptLine> builder)
    {
        builder.ToTable("GoodsReceiptLines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ReceivedQuantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitCost).HasPrecision(18, 4);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class PurchaseReturnConfiguration : IEntityTypeConfiguration<PurchaseReturn>
{
    public void Configure(EntityTypeBuilder<PurchaseReturn> builder)
    {
        builder.ToTable("PurchaseReturns");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ReturnNumber).IsRequired().HasMaxLength(50);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.PurchaseReturnId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class PurchaseReturnLineConfiguration : IEntityTypeConfiguration<PurchaseReturnLine>
{
    public void Configure(EntityTypeBuilder<PurchaseReturnLine> builder)
    {
        builder.ToTable("PurchaseReturnLines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ReturnQuantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitCost).HasPrecision(18, 4);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

// ─── Waste ────────────────────────────────────────────────────────────────
public sealed class WasteReasonConfiguration : IEntityTypeConfiguration<WasteReason>
{
    public void Configure(EntityTypeBuilder<WasteReason> builder)
    {
        builder.ToTable("WasteReasons");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NameEn).HasMaxLength(100);
        builder.Property(x => x.DescriptionAr).HasMaxLength(500);
        builder.Property(x => x.DescriptionEn).HasMaxLength(500);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class WasteRecordConfiguration : IEntityTypeConfiguration<WasteRecord>
{
    public void Configure(EntityTypeBuilder<WasteRecord> builder)
    {
        builder.ToTable("WasteRecords");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RecordNumber).IsRequired().HasMaxLength(50);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.WasteRecordId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class WasteItemConfiguration : IEntityTypeConfiguration<WasteItem>
{
    public void Configure(EntityTypeBuilder<WasteItem> builder)
    {
        builder.ToTable("WasteItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitCost).HasPrecision(18, 4);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

// ─── Transactions & Ledger ────────────────────────────────────────────────
public sealed class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.ToTable("InventoryTransactions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TransactionType).HasConversion<byte>();
        builder.Property(x => x.ReferenceDocumentNumber).IsRequired().HasMaxLength(50);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasMany(x => x.Movements).WithOne().HasForeignKey(x => x.InventoryTransactionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        // Append Only Ledger - No Audit properties, No Soft Delete
        builder.ToTable("StockMovements");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.QuantityChange).HasPrecision(18, 4);
        builder.Property(x => x.UnitCost).HasPrecision(18, 4);
        
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.InventoryItemId);
        builder.HasIndex(x => x.WarehouseId);
        
        builder.HasOne<InventoryItem>().WithMany().HasForeignKey(x => x.InventoryItemId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class InventoryBatchConfiguration : IEntityTypeConfiguration<InventoryBatch>
{
    public void Configure(EntityTypeBuilder<InventoryBatch> builder)
    {
        builder.ToTable("InventoryBatches");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BatchNumber).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Status).HasConversion<byte>();
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.InventoryItemId).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class InventoryReservationConfiguration : IEntityTypeConfiguration<InventoryReservation>
{
    public void Configure(EntityTypeBuilder<InventoryReservation> builder)
    {
        builder.ToTable("InventoryReservations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SourceDocument).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ReservedQuantity).HasPrecision(18, 4);
        builder.Property(x => x.Status).HasConversion<byte>();
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
    }
}

// ─── Counting & Adjustments & Transfers ───────────────────────────────────
public sealed class StockTransferConfiguration : IEntityTypeConfiguration<StockTransfer>
{
    public void Configure(EntityTypeBuilder<StockTransfer> builder)
    {
        builder.ToTable("StockTransfers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TransferNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Status).HasConversion<byte>();
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.StockTransferId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class StockTransferLineConfiguration : IEntityTypeConfiguration<StockTransferLine>
{
    public void Configure(EntityTypeBuilder<StockTransferLine> builder)
    {
        builder.ToTable("StockTransferLines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class StockCountConfiguration : IEntityTypeConfiguration<StockCount>
{
    public void Configure(EntityTypeBuilder<StockCount> builder)
    {
        builder.ToTable("StockCounts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CountNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Status).HasConversion<byte>();
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.StockCountId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class StockCountLineConfiguration : IEntityTypeConfiguration<StockCountLine>
{
    public void Configure(EntityTypeBuilder<StockCountLine> builder)
    {
        builder.ToTable("StockCountLines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExpectedQuantity).HasPrecision(18, 4);
        builder.Property(x => x.ActualQuantity).HasPrecision(18, 4);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class AdjustmentReasonConfiguration : IEntityTypeConfiguration<AdjustmentReason>
{
    public void Configure(EntityTypeBuilder<AdjustmentReason> builder)
    {
        builder.ToTable("AdjustmentReasons");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NameEn).HasMaxLength(100);
        builder.Property(x => x.DescriptionAr).HasMaxLength(500);
        builder.Property(x => x.DescriptionEn).HasMaxLength(500);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class StockAdjustmentConfiguration : IEntityTypeConfiguration<StockAdjustment>
{
    public void Configure(EntityTypeBuilder<StockAdjustment> builder)
    {
        builder.ToTable("StockAdjustments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AdjustmentNumber).IsRequired().HasMaxLength(50);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.StockAdjustmentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class StockAdjustmentLineConfiguration : IEntityTypeConfiguration<StockAdjustmentLine>
{
    public void Configure(EntityTypeBuilder<StockAdjustmentLine> builder)
    {
        builder.ToTable("StockAdjustmentLines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AdjustmentQuantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitCost).HasPrecision(18, 4);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
