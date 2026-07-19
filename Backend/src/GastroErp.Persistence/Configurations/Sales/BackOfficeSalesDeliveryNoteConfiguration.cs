using GastroErp.Domain.Entities.Sales.BackOffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastroErp.Persistence.Configurations.Sales;

public sealed class BackOfficeSalesDeliveryNoteConfiguration : IEntityTypeConfiguration<BackOfficeSalesDeliveryNote>
{
    public void Configure(EntityTypeBuilder<BackOfficeSalesDeliveryNote> builder)
    {
        builder.ToTable("BackOfficeSalesDeliveryNotes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DeliveryNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.Status).HasConversion<byte>();
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.DeliveryNoteId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(x => new { x.TenantId, x.DeliveryNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.OrderId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.CustomerId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.Status).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class BackOfficeSalesDeliveryNoteLineConfiguration : IEntityTypeConfiguration<BackOfficeSalesDeliveryNoteLine>
{
    public void Configure(EntityTypeBuilder<BackOfficeSalesDeliveryNoteLine> builder)
    {
        builder.ToTable("BackOfficeSalesDeliveryNoteLines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitCost).HasPrecision(18, 4);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasIndex(x => x.OrderLineId).HasFilter("[IsDeleted] = 0");
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
