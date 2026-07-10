using GastroErp.Domain.Entities.Catalog;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastroErp.Persistence.Configurations.Catalog;

public sealed class ProductCatalogDefinitionConfiguration : IEntityTypeConfiguration<ProductCatalogDefinition>
{
    public void Configure(EntityTypeBuilder<ProductCatalogDefinition> builder)
    {
        builder.ToTable("ProductCatalogDefinitions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.Sku).HasMaxLength(50);
        builder.Property(x => x.Barcode).HasMaxLength(50);
        builder.Property(x => x.ShortDescriptionAr).HasMaxLength(500);
        builder.Property(x => x.ShortDescriptionEn).HasMaxLength(500);
        builder.Property(x => x.LongDescriptionAr).HasMaxLength(2000);
        builder.Property(x => x.LongDescriptionEn).HasMaxLength(2000);
        builder.Property(x => x.Keywords).HasMaxLength(500);
        builder.Property(x => x.Brand).HasMaxLength(100);
        builder.Property(x => x.TagsJson).HasMaxLength(2000);
        builder.Property(x => x.PrimaryImageUrl).HasMaxLength(500);
        builder.Property(x => x.CatalogType).HasConversion<byte>();
        builder.Property(x => x.Status).HasConversion<byte>();
        builder.Property(x => x.CostingMethod).HasConversion<byte>();
        builder.Property(x => x.MinStock).HasPrecision(18, 4);
        builder.Property(x => x.MaxStock).HasPrecision(18, 4);
        builder.Property(x => x.SafetyStock).HasPrecision(18, 4);
        builder.Property(x => x.ReorderLevel).HasPrecision(18, 4);
        builder.Property(x => x.ReorderQuantity).HasPrecision(18, 4);
        builder.Property(x => x.RecipeYield).HasPrecision(18, 4);
        builder.Property(x => x.RecipeWastePercentage).HasPrecision(18, 4);
        builder.Property(x => x.BasePrice).HasPrecision(18, 4);
        builder.Property(x => x.Currency).HasMaxLength(3);
        builder.Property(x => x.RecipeInstructions).HasMaxLength(4000);
        builder.Property(x => x.PriceLevelsJson).HasMaxLength(4000);
        builder.Property(x => x.SupplierIdsJson).HasMaxLength(2000);
        builder.Property(x => x.MediaUrlsJson).HasMaxLength(4000);
        builder.Property(x => x.VariantAttributesJson).HasMaxLength(4000);
        builder.Property(x => x.RelatedProductsJson).HasMaxLength(4000);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class CatalogCodeSequenceConfiguration : IEntityTypeConfiguration<CatalogCodeSequence>
{
    public void Configure(EntityTypeBuilder<CatalogCodeSequence> builder)
    {
        builder.ToTable("CatalogCodeSequences");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Prefix).IsRequired().HasMaxLength(10);
        builder.HasIndex(x => new { x.TenantId, x.Prefix }).IsUnique();
    }
}

public sealed class ProductPriceHistoryConfiguration : IEntityTypeConfiguration<ProductPriceHistory>
{
    public void Configure(EntityTypeBuilder<ProductPriceHistory> builder)
    {
        builder.ToTable("ProductPriceHistories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Currency).IsRequired().HasMaxLength(3);
        builder.Property(x => x.PriceLevelName).HasMaxLength(100);
        builder.Property(x => x.OldPrice).HasPrecision(18, 4);
        builder.Property(x => x.NewPrice).HasPrecision(18, 4);
        builder.HasIndex(x => new { x.TenantId, x.CatalogDefinitionId });
    }
}
