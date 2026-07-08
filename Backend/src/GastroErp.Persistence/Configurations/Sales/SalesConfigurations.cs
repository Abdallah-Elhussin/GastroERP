using GastroErp.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastroErp.Persistence.Configurations.Sales;

public sealed class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("SalesOrders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber).IsRequired().HasMaxLength(50);
        builder.Property(o => o.Currency).IsRequired().HasMaxLength(3);
        builder.Property(o => o.Notes).HasMaxLength(1000);
        builder.Property(o => o.CancellationReason).HasMaxLength(500);
        builder.Property(o => o.SubTotal).HasPrecision(18, 4);
        builder.Property(o => o.DiscountTotal).HasPrecision(18, 4);
        builder.Property(o => o.TaxTotal).HasPrecision(18, 4);
        builder.Property(o => o.ServiceChargeTotal).HasPrecision(18, 4);
        builder.Property(o => o.GrandTotal).HasPrecision(18, 4);
        builder.Property(o => o.PaidAmount).HasPrecision(18, 4);
        builder.Property(o => o.DeliveryOrderId).IsRequired(false);
        builder.Property(o => o.CreatedBy).HasMaxLength(200);
        builder.Property(o => o.UpdatedBy).HasMaxLength(200);
        builder.Property(o => o.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(o => !o.IsDeleted);

        builder.HasMany(o => o.Items).WithOne().HasForeignKey(i => i.SalesOrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(o => o.Discounts).WithOne().HasForeignKey(d => d.SalesOrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(o => o.Taxes).WithOne().HasForeignKey(t => t.SalesOrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(o => o.StatusHistory).WithOne().HasForeignKey(h => h.SalesOrderId).OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(o => o.Discounts).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(o => o.Taxes).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(o => o.StatusHistory).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(o => new { o.BranchId, o.OrderNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(o => new { o.TenantId, o.BranchId, o.Status }).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(o => o.DeviceId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(o => o.CreatedAt).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(o => o.DeliveryOrderId).HasFilter("[IsDeleted] = 0 AND [DeliveryOrderId] IS NOT NULL");
    }
}

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductNameAr).IsRequired().HasMaxLength(200);
        builder.Property(i => i.ProductNameEn).HasMaxLength(200);
        builder.Property(i => i.Sku).HasMaxLength(50);
        builder.Property(i => i.Quantity).HasPrecision(18, 4);
        builder.Property(i => i.UnitPrice).HasPrecision(18, 4);
        builder.Property(i => i.LineDiscount).HasPrecision(18, 4);
        builder.Property(i => i.LineTax).HasPrecision(18, 4);
        builder.Property(i => i.LineTotal).HasPrecision(18, 4);
        builder.Property(i => i.Currency).IsRequired().HasMaxLength(3);
        builder.Property(i => i.Notes).HasMaxLength(500);
        builder.Property(i => i.VoidReason).HasMaxLength(500);
        builder.Property(i => i.CreatedBy).HasMaxLength(200);
        builder.Property(i => i.UpdatedBy).HasMaxLength(200);
        builder.Property(i => i.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(i => !i.IsDeleted);

        builder.HasMany(i => i.Modifiers).WithOne().HasForeignKey(m => m.OrderItemId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(i => i.Modifiers).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(i => i.SalesOrderId);
        builder.HasIndex(i => i.ProductId);
    }
}

public sealed class OrderLineModifierConfiguration : IEntityTypeConfiguration<OrderLineModifier>
{
    public void Configure(EntityTypeBuilder<OrderLineModifier> builder)
    {
        builder.ToTable("OrderLineModifiers");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.ModifierNameAr).IsRequired().HasMaxLength(200);
        builder.Property(m => m.ModifierNameEn).HasMaxLength(200);
        builder.Property(m => m.ExtraPrice).HasPrecision(18, 4);
        builder.Property(m => m.CreatedBy).HasMaxLength(200);
        builder.Property(m => m.UpdatedBy).HasMaxLength(200);
        builder.Property(m => m.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(m => !m.IsDeleted);
        builder.HasIndex(m => m.OrderItemId);
    }
}

public sealed class OrderDiscountConfiguration : IEntityTypeConfiguration<OrderDiscount>
{
    public void Configure(EntityTypeBuilder<OrderDiscount> builder)
    {
        builder.ToTable("OrderDiscounts");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Description).HasMaxLength(500);
        builder.Property(d => d.Amount).HasPrecision(18, 4);
        builder.Property(d => d.Currency).IsRequired().HasMaxLength(3);
        builder.Property(d => d.CreatedBy).HasMaxLength(200);
        builder.Property(d => d.UpdatedBy).HasMaxLength(200);
        builder.Property(d => d.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(d => !d.IsDeleted);
        builder.HasIndex(d => d.SalesOrderId);
    }
}

public sealed class OrderTaxConfiguration : IEntityTypeConfiguration<OrderTax>
{
    public void Configure(EntityTypeBuilder<OrderTax> builder)
    {
        builder.ToTable("OrderTaxes");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TaxNameAr).IsRequired().HasMaxLength(200);
        builder.Property(t => t.TaxNameEn).HasMaxLength(200);
        builder.Property(t => t.TaxRate).HasPrecision(18, 4);
        builder.Property(t => t.TaxableAmount).HasPrecision(18, 4);
        builder.Property(t => t.TaxAmount).HasPrecision(18, 4);
        builder.Property(t => t.Currency).IsRequired().HasMaxLength(3);
        builder.Property(t => t.CreatedBy).HasMaxLength(200);
        builder.Property(t => t.UpdatedBy).HasMaxLength(200);
        builder.Property(t => t.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(t => !t.IsDeleted);
        builder.HasIndex(t => t.SalesOrderId);
    }
}

public sealed class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("OrderStatusHistories");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Reason).HasMaxLength(500);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(h => h.SalesOrderId);
        builder.HasIndex(h => h.ChangedAt);
    }
}
