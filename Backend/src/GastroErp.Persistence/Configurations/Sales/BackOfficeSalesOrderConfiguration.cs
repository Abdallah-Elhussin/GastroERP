using GastroErp.Domain.Entities.Sales.BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastroErp.Persistence.Configurations.Sales;

public sealed class BackOfficeSalesOrderConfiguration : IEntityTypeConfiguration<BackOfficeSalesOrder>
{
    public void Configure(EntityTypeBuilder<BackOfficeSalesOrder> builder)
    {
        builder.ToTable("BackOfficeSalesOrders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OrderNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Currency).IsRequired().HasMaxLength(3);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.Status).HasConversion<byte>();
        builder.Property(x => x.FulfillmentStatus).HasConversion<byte>();
        builder.Property(x => x.ExchangeRate).HasPrecision(18, 6);
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 4);
        builder.Property(x => x.SubTotal).HasPrecision(18, 4);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 4);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 4);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.BackOfficeSalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(x => new { x.TenantId, x.OrderNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.CustomerId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.Status).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.QuotationId).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class BackOfficeSalesOrderLineConfiguration : IEntityTypeConfiguration<BackOfficeSalesOrderLine>
{
    public void Configure(EntityTypeBuilder<BackOfficeSalesOrderLine> builder)
    {
        builder.ToTable("BackOfficeSalesOrderLines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(500);
        builder.Property(x => x.LineNature).HasConversion<byte>();
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 4);
        builder.Property(x => x.UnitCost).HasPrecision(18, 4);
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 4);
        builder.Property(x => x.TaxPercent).HasPrecision(18, 4);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 4);
        builder.Property(x => x.DeliveredQuantity).HasPrecision(18, 4);
        builder.Property(x => x.InvoicedQuantity).HasPrecision(18, 4);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
