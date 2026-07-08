using GastroErp.Domain.Entities.Delivery;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastroErp.Persistence.Configurations.Delivery;

public sealed class DeliveryZoneConfiguration : IEntityTypeConfiguration<DeliveryZone>
{
    public void Configure(EntityTypeBuilder<DeliveryZone> builder)
    {
        builder.ToTable("DeliveryZones");
        builder.HasKey(z => z.Id);

        builder.Property(z => z.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(z => z.NameEn).HasMaxLength(200);
        builder.Property(z => z.CenterLatitude).HasPrecision(10, 7);
        builder.Property(z => z.CenterLongitude).HasPrecision(10, 7);
        builder.Property(z => z.RadiusKm).HasPrecision(10, 3);
        builder.Property(z => z.FixedFee).HasPrecision(18, 4);
        builder.Property(z => z.FeePerKm).HasPrecision(18, 4);
        builder.Property(z => z.CreatedBy).HasMaxLength(200);
        builder.Property(z => z.UpdatedBy).HasMaxLength(200);
        builder.Property(z => z.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(z => !z.IsDeleted);
        builder.HasIndex(z => new { z.TenantId, z.BranchId }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class DeliveryDriverConfiguration : IEntityTypeConfiguration<DeliveryDriver>
{
    public void Configure(EntityTypeBuilder<DeliveryDriver> builder)
    {
        builder.ToTable("DeliveryDrivers");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(d => d.NameEn).HasMaxLength(200);
        builder.Property(d => d.Phone).IsRequired().HasMaxLength(30);
        builder.Property(d => d.VehiclePlate).HasMaxLength(20);
        builder.Property(d => d.CreatedBy).HasMaxLength(200);
        builder.Property(d => d.UpdatedBy).HasMaxLength(200);
        builder.Property(d => d.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(d => !d.IsDeleted);
        builder.HasIndex(d => new { d.TenantId, d.BranchId, d.Status }).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(d => d.UserId).HasFilter("[IsDeleted] = 0 AND [UserId] IS NOT NULL");
    }
}

public sealed class DeliveryOrderConfiguration : IEntityTypeConfiguration<DeliveryOrder>
{
    public void Configure(EntityTypeBuilder<DeliveryOrder> builder)
    {
        builder.ToTable("DeliveryOrders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.DeliveryNumber).IsRequired().HasMaxLength(50);
        builder.Property(o => o.CustomerName).IsRequired().HasMaxLength(200);
        builder.Property(o => o.CustomerPhone).IsRequired().HasMaxLength(30);
        builder.Property(o => o.DeliveryAddress).IsRequired().HasMaxLength(500);
        builder.Property(o => o.AddressLine2).HasMaxLength(200);
        builder.Property(o => o.City).HasMaxLength(100);
        builder.Property(o => o.DeliveryNotes).HasMaxLength(500);
        builder.Property(o => o.Latitude).HasPrecision(10, 7);
        builder.Property(o => o.Longitude).HasPrecision(10, 7);
        builder.Property(o => o.DeliveryFee).HasPrecision(18, 4);
        builder.Property(o => o.ExternalProviderCode).HasMaxLength(50);
        builder.Property(o => o.ExternalOrderReference).HasMaxLength(100);
        builder.Property(o => o.FailureReason).HasMaxLength(500);
        builder.Property(o => o.CreatedBy).HasMaxLength(200);
        builder.Property(o => o.UpdatedBy).HasMaxLength(200);
        builder.Property(o => o.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(o => !o.IsDeleted);

        builder.HasMany(o => o.Assignments).WithOne().HasForeignKey(a => a.DeliveryOrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(o => o.TrackingEvents).WithOne().HasForeignKey(t => t.DeliveryOrderId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(o => o.Assignments).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(o => o.TrackingEvents).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(o => new { o.TenantId, o.DeliveryNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(o => o.SalesOrderId).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(o => new { o.TenantId, o.BranchId, o.Status }).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(o => o.CurrentDriverId).HasFilter("[IsDeleted] = 0 AND [CurrentDriverId] IS NOT NULL");
        builder.HasIndex(o => o.ExternalOrderReference).HasFilter("[IsDeleted] = 0 AND [ExternalOrderReference] IS NOT NULL");
    }
}

public sealed class DeliveryAssignmentConfiguration : IEntityTypeConfiguration<DeliveryAssignment>
{
    public void Configure(EntityTypeBuilder<DeliveryAssignment> builder)
    {
        builder.ToTable("DeliveryAssignments");
        builder.HasKey(a => a.Id);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(a => !a.IsDeleted);
        builder.HasIndex(a => a.DeliveryOrderId);
        builder.HasIndex(a => a.DriverId);
    }
}

public sealed class DeliveryTrackingEventConfiguration : IEntityTypeConfiguration<DeliveryTrackingEvent>
{
    public void Configure(EntityTypeBuilder<DeliveryTrackingEvent> builder)
    {
        builder.ToTable("DeliveryTrackingEvents");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Notes).IsRequired().HasMaxLength(500);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(t => !t.IsDeleted);
        builder.HasIndex(t => t.DeliveryOrderId);
        builder.HasIndex(t => t.OccurredAt);
    }
}
