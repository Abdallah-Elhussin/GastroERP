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
        builder.Property(x => x.DefaultCurrencyCode).HasMaxLength(10);
        builder.Property(x => x.CostPrecision);
        // Preserve existing column names for renamed flags
        builder.Property(x => x.AllowNegativeStock).HasColumnName("AllowNegativeInventory");
        builder.Property(x => x.EnableReservation).HasColumnName("AutoReserveStock");
        builder.Property(x => x.EnableBatchTracking).HasColumnName("RequireBatchTracking");
        builder.Property(x => x.EnableExpiryTracking).HasColumnName("RequireExpiryTracking");
        builder.Property(x => x.AutoGenerateItemCode).HasColumnName("AutoGenerateSku");
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.TenantId, x.BranchId }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasMany(x => x.DocumentSeries)
            .WithOne()
            .HasForeignKey(x => x.InventorySettingId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.DocumentSeries)
            .HasField("_documentSeries")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class InventoryDocumentNumberSeriesConfiguration : IEntityTypeConfiguration<InventoryDocumentNumberSeries>
{
    public void Configure(EntityTypeBuilder<InventoryDocumentNumberSeries> builder)
    {
        builder.ToTable("InventoryDocumentNumberSeries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DocumentType).HasConversion<byte>();
        builder.Property(x => x.Prefix).IsRequired().HasMaxLength(20);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.InventorySettingId, x.DocumentType }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
    }
}

// ─── Catalog ──────────────────────────────────────────────────────────────
public sealed class InventoryCategoryConfiguration : IEntityTypeConfiguration<InventoryCategory>
{
    public void Configure(EntityTypeBuilder<InventoryCategory> builder)
    {
        builder.ToTable("InventoryCategories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.DescriptionAr).HasMaxLength(500);
        builder.Property(x => x.DescriptionEn).HasMaxLength(500);
        builder.Property(x => x.Icon).HasMaxLength(100);
        builder.Property(x => x.ImageUrl).HasMaxLength(500);
        builder.Property(x => x.Color).HasMaxLength(30);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasOne(x => x.ParentCategory).WithMany(x => x.SubCategories).HasForeignKey(x => x.ParentCategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.NameAr }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.SortOrder });
    }
}

public sealed class InventoryUnitConfiguration : IEntityTypeConfiguration<InventoryUnit>
{
    public void Configure(EntityTypeBuilder<InventoryUnit> builder)
    {
        builder.ToTable("InventoryUnits");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NameEn).HasMaxLength(100);
        builder.Property(x => x.Symbol).IsRequired().HasMaxLength(20);
        builder.Property(x => x.SymbolAr).HasMaxLength(20);
        builder.Property(x => x.ConversionFactor).HasPrecision(18, 6);
        builder.Property(x => x.UnitType).HasConversion<byte>();
        builder.Property(x => x.Classification).HasConversion<byte>();
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.Symbol }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.BaseUnitId).HasFilter("[IsDeleted] = 0 AND [BaseUnitId] IS NOT NULL");
        builder.HasIndex(x => new { x.TenantId, x.SortOrder });
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
        builder.Property(x => x.ImageUrl).HasMaxLength(500);
        builder.Property(x => x.ItemKind).HasConversion<byte>();
        builder.Property(x => x.ItemTypeId);
        builder.HasIndex(x => x.ItemTypeId).HasFilter("[IsDeleted] = 0 AND [ItemTypeId] IS NOT NULL");
        builder.Property(x => x.ReorderLevel).HasPrecision(18, 4);
        builder.Property(x => x.ReorderQuantity).HasPrecision(18, 4);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.CategoryId).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class InventoryItemTypeConfiguration : IEntityTypeConfiguration<InventoryItemType>
{
    public void Configure(EntityTypeBuilder<InventoryItemType> builder)
    {
        builder.ToTable("InventoryItemTypes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Category).HasConversion<byte>();
        builder.Property(x => x.Color).IsRequired().HasMaxLength(30);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.NameAr }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.SortOrder });
    }
}

public sealed class InventoryValuationGroupConfiguration : IEntityTypeConfiguration<InventoryValuationGroup>
{
    public void Configure(EntityTypeBuilder<InventoryValuationGroup> builder)
    {
        builder.ToTable("InventoryValuationGroups");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.CostCenterId);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.NameAr }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.CostCenterId).HasFilter("[IsDeleted] = 0 AND [CostCenterId] IS NOT NULL");
        builder.HasIndex(x => new { x.TenantId, x.SortOrder });
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
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.Phone).HasMaxLength(50);
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.WarehouseType).HasConversion<byte>();
        builder.Property(x => x.ParentWarehouseId);
        builder.Property(x => x.WarehouseTypeId);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasMany(x => x.Zones).WithOne().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0 AND [Code] IS NOT NULL");
        builder.HasIndex(x => new { x.TenantId, x.BranchId }).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.CompanyId }).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.ParentWarehouseId).HasFilter("[IsDeleted] = 0 AND [ParentWarehouseId] IS NOT NULL");
        builder.HasIndex(x => x.WarehouseTypeId).HasFilter("[IsDeleted] = 0 AND [WarehouseTypeId] IS NOT NULL");
    }
}

public sealed class WarehouseTypeDefinitionConfiguration : IEntityTypeConfiguration<WarehouseTypeDefinition>
{
    public void Configure(EntityTypeBuilder<WarehouseTypeDefinition> builder)
    {
        builder.ToTable("WarehouseTypeDefinitions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
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
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.TaxNumber).HasMaxLength(50);
        builder.Property(x => x.CommercialRegister).HasMaxLength(50);
        builder.Property(x => x.EstablishmentNumber).HasMaxLength(50);
        builder.Property(x => x.TaxRegistrationCountry).HasMaxLength(100);
        builder.Property(x => x.TaxType).HasMaxLength(50);
        builder.Property(x => x.Currency).HasMaxLength(3);
        builder.Property(x => x.ContactPerson).HasMaxLength(200);
        builder.Property(x => x.ContactJobTitle).HasMaxLength(100);
        builder.Property(x => x.Phone).HasMaxLength(50);
        builder.Property(x => x.Mobile).HasMaxLength(50);
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.Website).HasMaxLength(300);
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.Region).HasMaxLength(100);
        builder.Property(x => x.Country).HasMaxLength(100);
        builder.Property(x => x.PostalCode).HasMaxLength(20);
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.PaymentTerms).HasMaxLength(200);
        builder.Property(x => x.BlacklistReason).HasMaxLength(500);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.TaxCertificatePath).HasMaxLength(500);
        builder.Property(x => x.CommercialRegisterPath).HasMaxLength(500);
        builder.Property(x => x.SupplierType).HasConversion<byte>();
        builder.Property(x => x.Category).HasConversion<byte>();
        builder.Property(x => x.DefaultPaymentMethod).HasConversion<byte>();
        builder.Property(x => x.VatEvaluation).HasConversion<byte>();
        builder.Property(x => x.CreditLimit).HasPrecision(18, 4);
        builder.Property(x => x.OpeningBalance).HasPrecision(18, 4);
        builder.Property(x => x.DefaultTaxPercent).HasPrecision(18, 4);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasMany(x => x.Contacts).WithOne().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.PaymentMethods).WithOne().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Attachments).WithOne().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Contacts).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.PaymentMethods).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Attachments).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.TaxNumber }).IsUnique().HasFilter("[IsDeleted] = 0 AND [TaxNumber] IS NOT NULL");
        builder.HasIndex(x => new { x.TenantId, x.CompanyId, x.NameAr }).IsUnique().HasFilter("[IsDeleted] = 0");
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
        builder.Property(x => x.Mobile).HasMaxLength(50);
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.Position).HasMaxLength(100);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class SupplierPaymentMethodConfiguration : IEntityTypeConfiguration<SupplierPaymentMethod>
{
    public void Configure(EntityTypeBuilder<SupplierPaymentMethod> builder)
    {
        builder.ToTable("SupplierPaymentMethods");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Kind).HasConversion<byte>();
        builder.Property(x => x.BankName).HasMaxLength(200);
        builder.Property(x => x.Iban).HasMaxLength(50);
        builder.Property(x => x.Swift).HasMaxLength(20);
        builder.Property(x => x.AccountNumber).HasMaxLength(50);
        builder.Property(x => x.BeneficiaryName).HasMaxLength(200);
        builder.Property(x => x.Currency).IsRequired().HasMaxLength(3);
        builder.Property(x => x.Notes).HasMaxLength(500);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.SupplierId);
    }
}

public sealed class SupplierAttachmentConfiguration : IEntityTypeConfiguration<SupplierAttachment>
{
    public void Configure(EntityTypeBuilder<SupplierAttachment> builder)
    {
        builder.ToTable("SupplierAttachments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FileName).IsRequired().HasMaxLength(255);
        builder.Property(x => x.ContentType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.StoragePath).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Category).HasMaxLength(50);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.SupplierId);
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
        builder.Property(x => x.OrderType);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 4);
        builder.Property(x => x.ExchangeRate).HasPrecision(18, 6);
        builder.Property(x => x.Currency).HasMaxLength(3);
        builder.Property(x => x.PaymentMethod).HasMaxLength(100);
        builder.Property(x => x.PaymentTerms).HasMaxLength(200);
        builder.Property(x => x.ExternalReference).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Ignore(x => x.CompletionPercent);
        builder.Ignore(x => x.RemainingQuantity);
        builder.Ignore(x => x.HasAnyReceipt);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.PurchaseOrderId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.SupplierId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.PoNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
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
        builder.Property(x => x.InvoicedQuantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 4);
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 4);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 4);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.LineNotes).HasMaxLength(1000);
        builder.Ignore(x => x.RemainingQuantity);
        builder.Ignore(x => x.LineSubTotal);
        builder.Ignore(x => x.LineTotal);
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
        builder.Property(x => x.ReferenceNumber).HasMaxLength(100);
        builder.Property(x => x.SupplierInvoiceNumber).HasMaxLength(100);
        builder.Property(x => x.Currency).IsRequired().HasMaxLength(3);
        builder.Property(x => x.ExchangeRate).HasPrecision(18, 6);
        builder.Property(x => x.ReceiptMethod).HasMaxLength(100);
        builder.Property(x => x.ReceivedByName).HasMaxLength(200);
        builder.Property(x => x.SupplierRepName).HasMaxLength(200);
        builder.Property(x => x.VehicleNumber).HasMaxLength(50);
        builder.Property(x => x.WaybillNumber).HasMaxLength(100);
        builder.Property(x => x.Status).HasConversion<byte>();
        builder.Property(x => x.Source).HasConversion<byte>();
        builder.Property(x => x.InspectionResult).HasConversion<byte>();
        builder.Property(x => x.InspectedBy).HasMaxLength(200);
        builder.Property(x => x.QualityNotes).HasMaxLength(2000);
        builder.Property(x => x.RejectionReason).HasMaxLength(1000);
        builder.Property(x => x.QualityCertificateRef).HasMaxLength(200);
        builder.Property(x => x.ExpiryCertificateRef).HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.GoodsReceiptId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.ReceiptNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.JournalEntryId).HasFilter("[JournalEntryId] IS NOT NULL");
        builder.HasIndex(x => x.PurchaseOrderId).HasFilter("[PurchaseOrderId] IS NOT NULL");
        builder.HasIndex(x => x.SupplierId);
    }
}

public sealed class GoodsReceiptLineConfiguration : IEntityTypeConfiguration<GoodsReceiptLine>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptLine> builder)
    {
        builder.ToTable("GoodsReceiptLines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OrderedQuantity).HasPrecision(18, 4);
        builder.Property(x => x.PreviouslyReceivedQuantity).HasPrecision(18, 4);
        builder.Property(x => x.ReceivedQuantity).HasPrecision(18, 4);
        builder.Property(x => x.AcceptedQuantity).HasPrecision(18, 4);
        builder.Property(x => x.RejectedQuantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitCost).HasPrecision(18, 4);
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 4);
        builder.Property(x => x.TaxPercent).HasPrecision(18, 4);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 4);
        builder.Property(x => x.InvoicedQuantity).HasPrecision(18, 4);
        builder.Property(x => x.ReturnedQuantity).HasPrecision(18, 4);
        builder.Property(x => x.BatchNumber).HasMaxLength(100);
        builder.Property(x => x.StorageLocation).HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.PurchaseOrderLineId).HasFilter("[PurchaseOrderLineId] IS NOT NULL");
    }
}

public sealed class PurchaseInvoiceConfiguration : IEntityTypeConfiguration<PurchaseInvoice>
{
    public void Configure(EntityTypeBuilder<PurchaseInvoice> builder)
    {
        builder.ToTable("PurchaseInvoices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.InvoiceNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Currency).IsRequired().HasMaxLength(3);
        builder.Property(x => x.SupplierInvoiceNumber).HasMaxLength(100);
        builder.Property(x => x.ExternalReference).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.Kind).HasConversion<byte>();
        builder.Property(x => x.PaymentMode).HasConversion<byte>();
        builder.Property(x => x.Nature).HasConversion<byte>();
        builder.Property(x => x.Status).HasConversion<byte>();
        builder.Property(x => x.PaymentStatus).HasConversion<byte>();
        builder.Property(x => x.ExchangeRate).HasPrecision(18, 6);
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 4);
        builder.Property(x => x.SubTotal).HasPrecision(18, 4);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 4);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 4);
        builder.Property(x => x.PaidAmount).HasPrecision(18, 4);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.PurchaseInvoiceId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(x => new { x.TenantId, x.InvoiceNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.GoodsReceiptId).HasFilter("[GoodsReceiptId] IS NOT NULL");
        builder.HasIndex(x => x.SupplierId).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class PurchaseInvoiceLineConfiguration : IEntityTypeConfiguration<PurchaseInvoiceLine>
{
    public void Configure(EntityTypeBuilder<PurchaseInvoiceLine> builder)
    {
        builder.ToTable("PurchaseInvoiceLines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 4);
        builder.Property(x => x.DiscountPercent).HasPrecision(18, 4);
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 4);
        builder.Property(x => x.TaxPercent).HasPrecision(18, 4);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 4);
        builder.Property(x => x.ReturnedQuantity).HasPrecision(18, 4);
        builder.Property(x => x.BatchNumber).HasMaxLength(100);
        builder.Property(x => x.SerialNumber).HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);
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
        builder.Property(x => x.Currency).IsRequired().HasMaxLength(3);
        builder.Property(x => x.ReferenceNumber).HasMaxLength(100);
        builder.Property(x => x.ReasonNotes).HasMaxLength(1000);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.ReturnType).HasConversion<byte>();
        builder.Property(x => x.Status).HasConversion<byte>();
        builder.Property(x => x.SubTotal).HasPrecision(18, 4);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 4);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 4);
        builder.Ignore(x => x.IsCompleted);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.PurchaseReturnId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.ReturnNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.GoodsReceiptId).HasFilter("[GoodsReceiptId] IS NOT NULL");
        builder.HasIndex(x => x.PurchaseInvoiceId).HasFilter("[PurchaseInvoiceId] IS NOT NULL");
        builder.HasIndex(x => x.SupplierId);
    }
}

public sealed class PurchaseReturnLineConfiguration : IEntityTypeConfiguration<PurchaseReturnLine>
{
    public void Configure(EntityTypeBuilder<PurchaseReturnLine> builder)
    {
        builder.ToTable("PurchaseReturnLines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OriginalQuantity).HasPrecision(18, 4);
        builder.Property(x => x.PreviouslyReturnedQuantity).HasPrecision(18, 4);
        builder.Property(x => x.ReturnQuantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitCost).HasPrecision(18, 4);
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 4);
        builder.Property(x => x.TaxPercent).HasPrecision(18, 4);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 4);
        builder.Property(x => x.ProductTemperature).HasPrecision(18, 4);
        builder.Property(x => x.BatchNumber).HasMaxLength(100);
        builder.Property(x => x.LineReason).HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(500);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.GoodsReceiptLineId).HasFilter("[GoodsReceiptLineId] IS NOT NULL");
        builder.HasIndex(x => x.PurchaseInvoiceLineId).HasFilter("[PurchaseInvoiceLineId] IS NOT NULL");
    }
}

public sealed class PurchaseReturnReasonConfiguration : IEntityTypeConfiguration<PurchaseReturnReason>
{
    public void Configure(EntityTypeBuilder<PurchaseReturnReason> builder)
    {
        builder.ToTable("PurchaseReturnReasons");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(30);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
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
        builder.HasIndex(x => new { x.TenantId, x.TransactionType, x.ReferenceDocumentId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}

public sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        // Append Only Ledger - No Audit properties, No Soft Delete
        builder.ToTable("StockMovements");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.QuantityChange).HasPrecision(18, 4);
        builder.Property(x => x.UnitCost).HasPrecision(18, 4);
        builder.Property(x => x.MovementType).HasConversion<byte>();

        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.InventoryItemId);
        builder.HasIndex(x => x.WarehouseId);

        builder.HasOne<InventoryItem>().WithMany().HasForeignKey(x => x.InventoryItemId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class InventoryBalanceConfiguration : IEntityTypeConfiguration<InventoryBalance>
{
    public void Configure(EntityTypeBuilder<InventoryBalance> builder)
    {
        builder.ToTable("InventoryBalances");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.QtyOnHand).HasPrecision(18, 4);
        builder.Property(x => x.ReservedQty).HasPrecision(18, 4);
        builder.Property(x => x.AvgCost).HasPrecision(18, 4);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.TenantId, x.InventoryItemId, x.WarehouseId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        builder.HasOne<InventoryItem>().WithMany().HasForeignKey(x => x.InventoryItemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Warehouse>().WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class IssueDestinationConfiguration : IEntityTypeConfiguration<GastroErp.Domain.Entities.Inventory.Issuing.IssueDestination>
{
    public void Configure(EntityTypeBuilder<GastroErp.Domain.Entities.Inventory.Issuing.IssueDestination> builder)
    {
        builder.ToTable("IssueDestinations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(30);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.DestinationType).HasConversion<byte>();
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.NameAr }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.DestinationType });
    }
}

public sealed class GoodsIssueConfiguration : IEntityTypeConfiguration<GastroErp.Domain.Entities.Inventory.Issuing.GoodsIssue>
{
    public void Configure(EntityTypeBuilder<GastroErp.Domain.Entities.Inventory.Issuing.GoodsIssue> builder)
    {
        builder.ToTable("GoodsIssues");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.IssueNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Currency).IsRequired().HasMaxLength(3);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.Status).HasConversion<byte>();
        builder.Ignore(x => x.IsConfirmed);
        builder.Ignore(x => x.IsCompleted);
        builder.Ignore(x => x.TotalAmount);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.GoodsIssueId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.TenantId, x.IssueNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.Status });
    }
}

public sealed class GoodsIssueLineConfiguration : IEntityTypeConfiguration<GastroErp.Domain.Entities.Inventory.Issuing.GoodsIssueLine>
{
    public void Configure(EntityTypeBuilder<GastroErp.Domain.Entities.Inventory.Issuing.GoodsIssueLine> builder)
    {
        builder.ToTable("GoodsIssueLines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitCost).HasPrecision(18, 4);
        builder.Ignore(x => x.TotalCost);
        builder.Property(x => x.Notes).HasMaxLength(500);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class OpeningBalanceConfiguration : IEntityTypeConfiguration<GastroErp.Domain.Entities.Inventory.Opening.OpeningBalance>
{
    public void Configure(EntityTypeBuilder<GastroErp.Domain.Entities.Inventory.Opening.OpeningBalance> builder)
    {
        builder.ToTable("OpeningBalances");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DocumentNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.Status).HasConversion<byte>();
        builder.Property(x => x.EntryMethod).HasConversion<byte>();
        builder.Property(x => x.DisplayMethod).HasConversion<byte>();
        builder.Property(x => x.CostingMethod).HasConversion<byte>();
        builder.Property(x => x.WeightedAverageScope).HasConversion<byte>();
        builder.Ignore(x => x.IsApproved);
        builder.Ignore(x => x.IsPosted);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.OpeningBalanceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.TenantId, x.DocumentNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.Status }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class OpeningBalanceLineConfiguration : IEntityTypeConfiguration<GastroErp.Domain.Entities.Inventory.Opening.OpeningBalanceLine>
{
    public void Configure(EntityTypeBuilder<GastroErp.Domain.Entities.Inventory.Opening.OpeningBalanceLine> builder)
    {
        builder.ToTable("OpeningBalanceLines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitCost).HasPrecision(18, 4);
        builder.Property(x => x.BatchNumber).HasMaxLength(100);
        builder.Property(x => x.SerialNumber).HasMaxLength(100);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.OpeningBalanceId);
        builder.HasIndex(x => x.InventoryItemId);
        builder.HasIndex(x => x.WarehouseId);
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
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.Status).HasConversion<byte>();
        builder.Property(x => x.TransferType).HasConversion<byte>();
        builder.Ignore(x => x.TotalAmount);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.StockTransferId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.TenantId, x.TransferNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.Status });
    }
}

public sealed class StockTransferLineConfiguration : IEntityTypeConfiguration<StockTransferLine>
{
    public void Configure(EntityTypeBuilder<StockTransferLine> builder)
    {
        builder.ToTable("StockTransferLines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitCost).HasPrecision(18, 4);
        builder.Property(x => x.ReceivedQuantity).HasPrecision(18, 4);
        builder.Ignore(x => x.LineTotal);
        builder.Property(x => x.BatchNumber).HasMaxLength(100);
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

// ─── Phase J Master Data Extensions ───────────────────────────────────────
public sealed class InventoryBrandConfiguration : IEntityTypeConfiguration<InventoryBrand>
{
    public void Configure(EntityTypeBuilder<InventoryBrand> builder)
    {
        builder.ToTable("InventoryBrands");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class InventoryManufacturerConfiguration : IEntityTypeConfiguration<InventoryManufacturer>
{
    public void Configure(EntityTypeBuilder<InventoryManufacturer> builder)
    {
        builder.ToTable("InventoryManufacturers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.Country).HasMaxLength(100);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class InventoryAttributeConfiguration : IEntityTypeConfiguration<InventoryAttribute>
{
    public void Configure(EntityTypeBuilder<InventoryAttribute> builder)
    {
        builder.ToTable("InventoryAttributes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.DataType).HasConversion<byte>();
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasMany(x => x.Values).WithOne().HasForeignKey(x => x.AttributeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class InventoryAttributeValueConfiguration : IEntityTypeConfiguration<InventoryAttributeValue>
{
    public void Configure(EntityTypeBuilder<InventoryAttributeValue> builder)
    {
        builder.ToTable("InventoryAttributeValues");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ValueAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ValueEn).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.AttributeId).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class InventoryPriceListConfiguration : IEntityTypeConfiguration<InventoryPriceList>
{
    public void Configure(EntityTypeBuilder<InventoryPriceList> builder)
    {
        builder.ToTable("InventoryPriceLists");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.Currency).IsRequired().HasMaxLength(10);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.PriceListId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class InventoryPriceListLineConfiguration : IEntityTypeConfiguration<InventoryPriceListLine>
{
    public void Configure(EntityTypeBuilder<InventoryPriceListLine> builder)
    {
        builder.ToTable("InventoryPriceListLines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 4);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.PriceListId, x.InventoryItemId }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}
