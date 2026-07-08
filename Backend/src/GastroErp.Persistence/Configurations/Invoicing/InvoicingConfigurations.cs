using GastroErp.Domain.Entities.Invoicing;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastroErp.Persistence.Configurations.Invoicing;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(50);
        builder.Property(i => i.Currency).IsRequired().HasMaxLength(3);
        builder.Property(i => i.CustomerName).HasMaxLength(200);
        builder.Property(i => i.CancellationReason).HasMaxLength(500);
        builder.Property(i => i.Notes).HasMaxLength(1000);
        builder.Property(i => i.SubTotal).HasPrecision(18, 4);
        builder.Property(i => i.DiscountTotal).HasPrecision(18, 4);
        builder.Property(i => i.TaxTotal).HasPrecision(18, 4);
        builder.Property(i => i.GrandTotal).HasPrecision(18, 4);
        builder.Property(i => i.PaidAmount).HasPrecision(18, 4);
        builder.Property(i => i.CreditedAmount).HasPrecision(18, 4);
        builder.Property(i => i.CreatedBy).HasMaxLength(200);
        builder.Property(i => i.UpdatedBy).HasMaxLength(200);
        builder.Property(i => i.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(i => !i.IsDeleted);

        builder.HasMany(i => i.Lines).WithOne().HasForeignKey(l => l.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(i => i.TaxLines).WithOne().HasForeignKey(t => t.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(i => i.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(i => i.TaxLines).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(i => new { i.TenantId, i.InvoiceNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(i => new { i.TenantId, i.BranchId, i.Status }).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(i => i.SalesOrderId).HasFilter("[IsDeleted] = 0 AND [SalesOrderId] IS NOT NULL");
        builder.HasIndex(i => i.IssuedAt).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable("InvoiceLines");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.ProductNameAr).IsRequired().HasMaxLength(200);
        builder.Property(l => l.ProductNameEn).HasMaxLength(200);
        builder.Property(l => l.Sku).HasMaxLength(50);
        builder.Property(l => l.Currency).IsRequired().HasMaxLength(3);
        builder.Property(l => l.Quantity).HasPrecision(18, 4);
        builder.Property(l => l.UnitPrice).HasPrecision(18, 4);
        builder.Property(l => l.DiscountAmount).HasPrecision(18, 4);
        builder.Property(l => l.TaxAmount).HasPrecision(18, 4);
        builder.Property(l => l.NetAmount).HasPrecision(18, 4);
        builder.Property(l => l.TotalAmount).HasPrecision(18, 4);
        builder.Property(l => l.CreatedBy).HasMaxLength(200);
        builder.Property(l => l.UpdatedBy).HasMaxLength(200);
        builder.Property(l => l.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(l => !l.IsDeleted);
        builder.HasIndex(l => l.InvoiceId);
        builder.HasIndex(l => l.ProductId);
    }
}

public sealed class InvoiceTaxLineConfiguration : IEntityTypeConfiguration<InvoiceTaxLine>
{
    public void Configure(EntityTypeBuilder<InvoiceTaxLine> builder)
    {
        builder.ToTable("InvoiceTaxLines");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TaxNameAr).IsRequired().HasMaxLength(200);
        builder.Property(t => t.TaxNameEn).HasMaxLength(200);
        builder.Property(t => t.Currency).IsRequired().HasMaxLength(3);
        builder.Property(t => t.TaxRate).HasPrecision(18, 4);
        builder.Property(t => t.TaxableAmount).HasPrecision(18, 4);
        builder.Property(t => t.TaxAmount).HasPrecision(18, 4);
        builder.Property(t => t.CreatedBy).HasMaxLength(200);
        builder.Property(t => t.UpdatedBy).HasMaxLength(200);
        builder.Property(t => t.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(t => !t.IsDeleted);
        builder.HasIndex(t => t.InvoiceId);
    }
}

public sealed class TaxRateConfiguration : IEntityTypeConfiguration<TaxRate>
{
    public void Configure(EntityTypeBuilder<TaxRate> builder)
    {
        builder.ToTable("TaxRates");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Code).IsRequired().HasMaxLength(20);
        builder.Property(t => t.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(t => t.NameEn).HasMaxLength(200);
        builder.Property(t => t.Description).HasMaxLength(500);
        builder.Property(t => t.Rate).HasPrecision(18, 4);
        builder.Property(t => t.FixedAmount).HasPrecision(18, 4);
        builder.Property(t => t.CreatedBy).HasMaxLength(200);
        builder.Property(t => t.UpdatedBy).HasMaxLength(200);
        builder.Property(t => t.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(t => !t.IsDeleted);
        builder.HasIndex(t => new { t.TenantId, t.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class TaxGroupConfiguration : IEntityTypeConfiguration<TaxGroup>
{
    public void Configure(EntityTypeBuilder<TaxGroup> builder)
    {
        builder.ToTable("TaxGroups");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(g => g.NameEn).HasMaxLength(200);
        builder.Property(g => g.Description).HasMaxLength(500);
        builder.Property(g => g.CreatedBy).HasMaxLength(200);
        builder.Property(g => g.UpdatedBy).HasMaxLength(200);
        builder.Property(g => g.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(g => !g.IsDeleted);

        builder.HasMany(g => g.Rates).WithOne().HasForeignKey(r => r.TaxGroupId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(g => g.Rates).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(g => new { g.TenantId, g.NameAr }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class TaxGroupRateConfiguration : IEntityTypeConfiguration<TaxGroupRate>
{
    public void Configure(EntityTypeBuilder<TaxGroupRate> builder)
    {
        builder.ToTable("TaxGroupRates");
        builder.HasKey(r => r.Id);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(r => !r.IsDeleted);
        builder.HasIndex(r => new { r.TaxGroupId, r.TaxRateId }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class CreditNoteConfiguration : IEntityTypeConfiguration<CreditNote>
{
    public void Configure(EntityTypeBuilder<CreditNote> builder)
    {
        builder.ToTable("CreditNotes");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CreditNoteNumber).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Currency).IsRequired().HasMaxLength(3);
        builder.Property(c => c.Reason).IsRequired().HasMaxLength(500);
        builder.Property(c => c.CancellationReason).HasMaxLength(500);
        builder.Property(c => c.SubTotal).HasPrecision(18, 4);
        builder.Property(c => c.TaxTotal).HasPrecision(18, 4);
        builder.Property(c => c.TotalAmount).HasPrecision(18, 4);
        builder.Property(c => c.CreatedBy).HasMaxLength(200);
        builder.Property(c => c.UpdatedBy).HasMaxLength(200);
        builder.Property(c => c.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(c => !c.IsDeleted);

        builder.HasMany(c => c.Lines).WithOne().HasForeignKey(l => l.CreditNoteId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(c => new { c.TenantId, c.CreditNoteNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(c => c.OriginalInvoiceId).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class CreditNoteLineConfiguration : IEntityTypeConfiguration<CreditNoteLine>
{
    public void Configure(EntityTypeBuilder<CreditNoteLine> builder)
    {
        builder.ToTable("CreditNoteLines");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.ProductNameAr).IsRequired().HasMaxLength(200);
        builder.Property(l => l.ProductNameEn).HasMaxLength(200);
        builder.Property(l => l.Currency).IsRequired().HasMaxLength(3);
        builder.Property(l => l.Quantity).HasPrecision(18, 4);
        builder.Property(l => l.UnitPrice).HasPrecision(18, 4);
        builder.Property(l => l.TaxAmount).HasPrecision(18, 4);
        builder.Property(l => l.NetAmount).HasPrecision(18, 4);
        builder.Property(l => l.TotalAmount).HasPrecision(18, 4);
        builder.Property(l => l.CreatedBy).HasMaxLength(200);
        builder.Property(l => l.UpdatedBy).HasMaxLength(200);
        builder.Property(l => l.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(l => !l.IsDeleted);
        builder.HasIndex(l => l.CreditNoteId);
    }
}

public sealed class DebitNoteConfiguration : IEntityTypeConfiguration<DebitNote>
{
    public void Configure(EntityTypeBuilder<DebitNote> builder)
    {
        builder.ToTable("DebitNotes");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.DebitNoteNumber).IsRequired().HasMaxLength(50);
        builder.Property(d => d.Currency).IsRequired().HasMaxLength(3);
        builder.Property(d => d.Reason).IsRequired().HasMaxLength(500);
        builder.Property(d => d.CancellationReason).HasMaxLength(500);
        builder.Property(d => d.SubTotal).HasPrecision(18, 4);
        builder.Property(d => d.TaxTotal).HasPrecision(18, 4);
        builder.Property(d => d.TotalAmount).HasPrecision(18, 4);
        builder.Property(d => d.CreatedBy).HasMaxLength(200);
        builder.Property(d => d.UpdatedBy).HasMaxLength(200);
        builder.Property(d => d.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(d => !d.IsDeleted);

        builder.HasMany(d => d.Lines).WithOne().HasForeignKey(l => l.DebitNoteId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(d => d.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(d => new { d.TenantId, d.DebitNoteNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(d => d.OriginalInvoiceId).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class DebitNoteLineConfiguration : IEntityTypeConfiguration<DebitNoteLine>
{
    public void Configure(EntityTypeBuilder<DebitNoteLine> builder)
    {
        builder.ToTable("DebitNoteLines");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.DescriptionAr).IsRequired().HasMaxLength(200);
        builder.Property(l => l.DescriptionEn).HasMaxLength(200);
        builder.Property(l => l.Currency).IsRequired().HasMaxLength(3);
        builder.Property(l => l.Quantity).HasPrecision(18, 4);
        builder.Property(l => l.UnitPrice).HasPrecision(18, 4);
        builder.Property(l => l.TaxAmount).HasPrecision(18, 4);
        builder.Property(l => l.NetAmount).HasPrecision(18, 4);
        builder.Property(l => l.TotalAmount).HasPrecision(18, 4);
        builder.Property(l => l.CreatedBy).HasMaxLength(200);
        builder.Property(l => l.UpdatedBy).HasMaxLength(200);
        builder.Property(l => l.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(l => !l.IsDeleted);
        builder.HasIndex(l => l.DebitNoteId);
    }
}
