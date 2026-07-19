using GastroErp.Domain.Entities.Sales.BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastroErp.Persistence.Configurations.Sales;

public sealed class BackOfficeSalesReturnConfiguration : IEntityTypeConfiguration<BackOfficeSalesReturn>
{
    public void Configure(EntityTypeBuilder<BackOfficeSalesReturn> builder)
    {
        builder.ToTable("BackOfficeSalesReturns");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ReturnNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.Status).HasConversion<byte>();
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 4);
        builder.Property(x => x.SubTotal).HasPrecision(18, 4);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 4);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 4);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.ReturnId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(x => new { x.TenantId, x.ReturnNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.InvoiceId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.CustomerId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.Status).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class BackOfficeSalesReturnLineConfiguration : IEntityTypeConfiguration<BackOfficeSalesReturnLine>
{
    public void Configure(EntityTypeBuilder<BackOfficeSalesReturnLine> builder)
    {
        builder.ToTable("BackOfficeSalesReturnLines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(500);
        builder.Property(x => x.LineNature).HasConversion<byte>();
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 4);
        builder.Property(x => x.UnitCost).HasPrecision(18, 4);
        builder.Property(x => x.TaxPercent).HasPrecision(18, 4);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 4);
        builder.HasIndex(x => x.InvoiceLineId).HasFilter("[IsDeleted] = 0");
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
