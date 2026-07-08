using GastroErp.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastroErp.Persistence.Configurations.Sales;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.ReceiptNumber).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Currency).IsRequired().HasMaxLength(3);
        builder.Property(p => p.Amount).HasPrecision(18, 4);
        builder.Property(p => p.TipAmount).HasPrecision(18, 4);
        builder.Property(p => p.ReferenceNumber).HasMaxLength(100);
        builder.Property(p => p.GatewayTransactionId).HasMaxLength(200);
        builder.Property(p => p.VoidReason).HasMaxLength(500);
        builder.Property(p => p.CreatedBy).HasMaxLength(200);
        builder.Property(p => p.UpdatedBy).HasMaxLength(200);
        builder.Property(p => p.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.HasMany(p => p.Allocations).WithOne().HasForeignKey(a => a.PaymentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Refunds).WithOne().HasForeignKey(r => r.PaymentId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.Allocations).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(p => p.Refunds).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(p => new { p.BranchId, p.ReceiptNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(p => p.CashierShiftId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(p => new { p.TenantId, p.Status }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class PaymentAllocationConfiguration : IEntityTypeConfiguration<PaymentAllocation>
{
    public void Configure(EntityTypeBuilder<PaymentAllocation> builder)
    {
        builder.ToTable("PaymentAllocations");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.AllocatedAmount).HasPrecision(18, 4);
        builder.Property(a => a.Currency).IsRequired().HasMaxLength(3);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(a => !a.IsDeleted);
        builder.HasIndex(a => a.SalesOrderId);
        builder.HasIndex(a => a.PaymentId);
    }
}

public sealed class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.ToTable("Refunds");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.RefundAmount).HasPrecision(18, 4);
        builder.Property(r => r.Currency).IsRequired().HasMaxLength(3);
        builder.Property(r => r.Reason).IsRequired().HasMaxLength(500);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(r => !r.IsDeleted);
        builder.HasIndex(r => r.PaymentId);
        builder.HasIndex(r => r.SalesOrderId);
    }
}

public sealed class CashRegisterConfiguration : IEntityTypeConfiguration<CashRegister>
{
    public void Configure(EntityTypeBuilder<CashRegister> builder)
    {
        builder.ToTable("CashRegisters");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(r => r.NameEn).HasMaxLength(200);
        builder.Property(r => r.Code).IsRequired().HasMaxLength(50);
        builder.Property(r => r.OpeningBalance).HasPrecision(18, 4);
        builder.Property(r => r.ClosingBalance).HasPrecision(18, 4);
        builder.Property(r => r.ExpectedBalance).HasPrecision(18, 4);
        builder.Property(r => r.ActualBalance).HasPrecision(18, 4);
        builder.Property(r => r.Difference).HasPrecision(18, 4);
        builder.Property(r => r.CurrentBalance).HasPrecision(18, 4);
        builder.Property(r => r.DefaultOpeningFloat).HasPrecision(18, 4);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(r => !r.IsDeleted);

        builder.HasIndex(r => new { r.BranchId, r.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(r => new { r.TenantId, r.BranchId, r.Status }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class CashierShiftConfiguration : IEntityTypeConfiguration<CashierShift>
{
    public void Configure(EntityTypeBuilder<CashierShift> builder)
    {
        builder.ToTable("CashierShifts");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.ShiftNumber).IsRequired().HasMaxLength(50);
        builder.Property(s => s.OpeningFloat).HasPrecision(18, 4);
        builder.Property(s => s.ExpectedCash).HasPrecision(18, 4);
        builder.Property(s => s.ActualCash).HasPrecision(18, 4);
        builder.Property(s => s.Variance).HasPrecision(18, 4);
        builder.Property(s => s.Notes).HasMaxLength(1000);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(s => !s.IsDeleted);

        builder.HasMany(s => s.CashMovements).WithOne().HasForeignKey(m => m.CashierShiftId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.CashMovements).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(s => new { s.BranchId, s.ShiftNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(s => new { s.CashierId, s.Status }).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(s => s.DeviceId).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class CashMovementConfiguration : IEntityTypeConfiguration<CashMovement>
{
    public void Configure(EntityTypeBuilder<CashMovement> builder)
    {
        builder.ToTable("CashMovements");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Amount).HasPrecision(18, 4);
        builder.Property(m => m.Reason).IsRequired().HasMaxLength(500);
        builder.Property(m => m.ReferenceDocument).HasMaxLength(100);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasIndex(m => m.CashierShiftId);
        builder.HasIndex(m => m.CashRegisterId);
        builder.HasIndex(m => m.CreatedAtMovement);
    }
}
