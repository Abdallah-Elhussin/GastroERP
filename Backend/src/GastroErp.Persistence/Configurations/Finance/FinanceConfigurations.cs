using GastroErp.Domain.Entities.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastroErp.Persistence.Configurations.Finance;

public sealed class ChartOfAccountConfiguration : IEntityTypeConfiguration<ChartOfAccount>
{
    public void Configure(EntityTypeBuilder<ChartOfAccount> builder)
    {
        builder.ToTable("ChartOfAccounts");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AccountNumber).IsRequired().HasMaxLength(20);
        builder.Property(a => a.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(a => a.NameEn).HasMaxLength(200);
        builder.Property(a => a.Currency).IsRequired().HasMaxLength(3);
        builder.Property(a => a.CreatedBy).HasMaxLength(200);
        builder.Property(a => a.UpdatedBy).HasMaxLength(200);
        builder.Property(a => a.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(a => !a.IsDeleted);
        builder.HasIndex(a => new { a.TenantId, a.AccountNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(a => a.ParentAccountId).HasFilter("[IsDeleted] = 0 AND [ParentAccountId] IS NOT NULL");
        builder.HasIndex(a => new { a.TenantId, a.AccountType }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class FiscalPeriodConfiguration : IEntityTypeConfiguration<FiscalPeriod>
{
    public void Configure(EntityTypeBuilder<FiscalPeriod> builder)
    {
        builder.ToTable("FiscalPeriods");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.CreatedBy).HasMaxLength(200);
        builder.Property(p => p.UpdatedBy).HasMaxLength(200);
        builder.Property(p => p.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(p => !p.IsDeleted);
        builder.HasIndex(p => new { p.TenantId, p.FiscalYear }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(p => new { p.TenantId, p.Status }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class CostCenterConfiguration : IEntityTypeConfiguration<CostCenter>
{
    public void Configure(EntityTypeBuilder<CostCenter> builder)
    {
        builder.ToTable("CostCenters");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code).IsRequired().HasMaxLength(20);
        builder.Property(c => c.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(c => c.NameEn).HasMaxLength(200);
        builder.Property(c => c.CreatedBy).HasMaxLength(200);
        builder.Property(c => c.UpdatedBy).HasMaxLength(200);
        builder.Property(c => c.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(c => !c.IsDeleted);
        builder.HasIndex(c => new { c.TenantId, c.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(c => new { c.TenantId, c.BranchId }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("JournalEntries");
        builder.HasKey(j => j.Id);

        builder.Property(j => j.EntryNumber).IsRequired().HasMaxLength(50);
        builder.Property(j => j.Description).IsRequired().HasMaxLength(500);
        builder.Property(j => j.Reference).HasMaxLength(100);
        builder.Property(j => j.CreatedBy).HasMaxLength(200);
        builder.Property(j => j.UpdatedBy).HasMaxLength(200);
        builder.Property(j => j.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(j => !j.IsDeleted);

        builder.HasMany(j => j.Lines).WithOne().HasForeignKey(l => l.JournalEntryId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(j => j.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(j => new { j.TenantId, j.EntryNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(j => new { j.TenantId, j.PostingDate }).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(j => new { j.TenantId, j.Status }).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(j => new { j.SourceModule, j.SourceDocumentId }).HasFilter("[IsDeleted] = 0 AND [SourceDocumentId] IS NOT NULL");
    }
}

public sealed class JournalEntryLineConfiguration : IEntityTypeConfiguration<JournalEntryLine>
{
    public void Configure(EntityTypeBuilder<JournalEntryLine> builder)
    {
        builder.ToTable("JournalEntryLines");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Debit).HasPrecision(18, 4);
        builder.Property(l => l.Credit).HasPrecision(18, 4);
        builder.Property(l => l.Currency).IsRequired().HasMaxLength(3);
        builder.Property(l => l.Description).HasMaxLength(500);
        builder.Property(l => l.CreatedBy).HasMaxLength(200);
        builder.Property(l => l.UpdatedBy).HasMaxLength(200);
        builder.Property(l => l.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(l => !l.IsDeleted);
        builder.HasIndex(l => l.JournalEntryId);
        builder.HasIndex(l => l.ChartOfAccountId);
        builder.HasIndex(l => l.CostCenterId).HasFilter("[CostCenterId] IS NOT NULL");
    }
}

public sealed class AccountingTransactionConfiguration : IEntityTypeConfiguration<AccountingTransaction>
{
    public void Configure(EntityTypeBuilder<AccountingTransaction> builder)
    {
        builder.ToTable("AccountingTransactions");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.ReferenceNumber).IsRequired().HasMaxLength(100);
        builder.Property(t => t.CreatedBy).HasMaxLength(200);
        builder.Property(t => t.UpdatedBy).HasMaxLength(200);
        builder.Property(t => t.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(t => !t.IsDeleted);
        builder.HasIndex(t => new { t.TenantId, t.SourceModule, t.SourceDocumentId }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(t => t.JournalEntryId);
    }
}
