using GastroErp.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastroErp.Persistence.Configurations.Sales;

public sealed class KitchenStationConfiguration : IEntityTypeConfiguration<KitchenStation>
{
    public void Configure(EntityTypeBuilder<KitchenStation> builder)
    {
        builder.ToTable("KitchenStations");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(s => s.NameEn).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(s => !s.IsDeleted);
        builder.HasIndex(s => new { s.BranchId, s.IsActive }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class KitchenTicketConfiguration : IEntityTypeConfiguration<KitchenTicket>
{
    public void Configure(EntityTypeBuilder<KitchenTicket> builder)
    {
        builder.ToTable("KitchenTickets");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.TicketNumber).IsRequired().HasMaxLength(50);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(t => !t.IsDeleted);
        builder.HasMany(t => t.Items).WithOne().HasForeignKey(i => i.KitchenTicketId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(t => t.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(t => t.SalesOrderId);
        builder.HasIndex(t => new { t.BranchId, t.Status }).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(t => t.KitchenStationId).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class KitchenTicketItemConfiguration : IEntityTypeConfiguration<KitchenTicketItem>
{
    public void Configure(EntityTypeBuilder<KitchenTicketItem> builder)
    {
        builder.ToTable("KitchenTicketItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ProductNameAr).IsRequired().HasMaxLength(200);
        builder.Property(i => i.ProductNameEn).HasMaxLength(200);
        builder.Property(i => i.Quantity).HasPrecision(18, 4);
        builder.Property(i => i.ModifiersSummary).HasMaxLength(500);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(i => !i.IsDeleted);
        builder.HasIndex(i => i.KitchenTicketId);
    }
}

public sealed class FloorPlanConfiguration : IEntityTypeConfiguration<FloorPlan>
{
    public void Configure(EntityTypeBuilder<FloorPlan> builder)
    {
        builder.ToTable("FloorPlans");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(f => f.NameEn).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(f => !f.IsDeleted);
        builder.HasMany(f => f.DiningAreas).WithOne().HasForeignKey(a => a.FloorPlanId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(f => f.DiningAreas).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(f => new { f.BranchId, f.IsActive }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class DiningAreaConfiguration : IEntityTypeConfiguration<DiningArea>
{
    public void Configure(EntityTypeBuilder<DiningArea> builder)
    {
        builder.ToTable("DiningAreas");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(a => a.NameEn).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(a => !a.IsDeleted);
        builder.HasMany(a => a.Tables).WithOne().HasForeignKey(t => t.DiningAreaId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(a => a.Tables).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(a => a.FloorPlanId);
    }
}

public sealed class RestaurantTableConfiguration : IEntityTypeConfiguration<RestaurantTable>
{
    public void Configure(EntityTypeBuilder<RestaurantTable> builder)
    {
        builder.ToTable("RestaurantTables");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.TableNumber).IsRequired().HasMaxLength(20);
        builder.Property(t => t.NameAr).HasMaxLength(100);
        builder.Property(t => t.NameEn).HasMaxLength(100);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(t => !t.IsDeleted);
        builder.HasIndex(t => new { t.DiningAreaId, t.TableNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(t => t.Status).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class TableReservationConfiguration : IEntityTypeConfiguration<TableReservation>
{
    public void Configure(EntityTypeBuilder<TableReservation> builder)
    {
        builder.ToTable("TableReservations");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.CustomerName).IsRequired().HasMaxLength(200);
        builder.Property(r => r.CustomerPhone).IsRequired().HasMaxLength(20);
        builder.Property(r => r.Notes).HasMaxLength(500);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(r => !r.IsDeleted);
        builder.HasIndex(r => new { r.BranchId, r.ReservationDate }).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(r => r.TableId).HasFilter("[IsDeleted] = 0");
    }
}
