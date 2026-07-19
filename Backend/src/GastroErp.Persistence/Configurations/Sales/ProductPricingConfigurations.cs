using GastroErp.Domain.Entities.Sales.Pricing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastroErp.Persistence.Configurations.Sales;

public sealed class SalesPriceListConfiguration : IEntityTypeConfiguration<SalesPriceList>
{
    public void Configure(EntityTypeBuilder<SalesPriceList> builder)
    {
        builder.ToTable("SalesPriceLists");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameEn).HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.DefaultSalesChannel).HasConversion<byte?>();
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.SortOrder });
        builder.HasIndex(x => new { x.TenantId, x.IsDefault }).HasFilter("[IsDeleted] = 0 AND [IsDefault] = 1");
    }
}

public sealed class ProductPriceConfiguration : IEntityTypeConfiguration<ProductPrice>
{
    public void Configure(EntityTypeBuilder<ProductPrice> builder)
    {
        builder.ToTable("ProductPrices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SalesChannel).HasConversion<byte>();
        builder.Property(x => x.PricingMethod).HasConversion<byte>();
        builder.Property(x => x.CostType).HasConversion<byte>();
        builder.Property(x => x.Cost).HasPrecision(18, 4);
        builder.Property(x => x.ProfitMargin).HasPrecision(18, 4);
        builder.Property(x => x.ProfitAmount).HasPrecision(18, 4);
        builder.Property(x => x.SellingPrice).HasPrecision(18, 4);
        builder.Property(x => x.MinimumPrice).HasPrecision(18, 4);
        builder.Property(x => x.MaximumDiscount).HasPrecision(18, 4);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.ProductId }).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.PriceListId }).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.BranchId }).HasFilter("[IsDeleted] = 0 AND [BranchId] IS NOT NULL");
        builder.HasIndex(x => new
        {
            x.TenantId,
            x.ProductId,
            x.UnitId,
            x.PriceListId,
            x.BranchId,
            x.SalesChannel,
            x.StartDate
        }).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.IsActive, x.Priority }).HasFilter("[IsDeleted] = 0");
    }
}
