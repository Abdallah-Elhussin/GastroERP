using GastroErp.Domain.Entities.Sales.BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastroErp.Persistence.Configurations.Sales;

public sealed class BackOfficeSalesInvoiceConfiguration : IEntityTypeConfiguration<BackOfficeSalesInvoice>
{
    public void Configure(EntityTypeBuilder<BackOfficeSalesInvoice> builder)
    {
        builder.ToTable("BackOfficeSalesInvoices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.InvoiceNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Currency).IsRequired().HasMaxLength(3);
        builder.Property(x => x.ExternalReference).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.Nature).HasConversion<byte>();
        builder.Property(x => x.PaymentMode).HasConversion<byte>();
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
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.BackOfficeSalesInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(x => new { x.TenantId, x.InvoiceNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.CustomerId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.Status).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class BackOfficeSalesInvoiceLineConfiguration : IEntityTypeConfiguration<BackOfficeSalesInvoiceLine>
{
    public void Configure(EntityTypeBuilder<BackOfficeSalesInvoiceLine> builder)
    {
        builder.ToTable("BackOfficeSalesInvoiceLines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(500);
        builder.Property(x => x.LineNature).HasConversion<byte>();
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 4);
        builder.Property(x => x.UnitCost).HasPrecision(18, 4);
        builder.Property(x => x.DiscountPercent).HasPrecision(18, 4);
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 4);
        builder.Property(x => x.TaxPercent).HasPrecision(18, 4);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 4);
        builder.Property(x => x.ReturnedQuantity).HasPrecision(18, 4);
        builder.HasIndex(x => x.SalesOrderLineId).HasFilter("[IsDeleted] = 0");
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
