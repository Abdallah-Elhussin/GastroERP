using GastroErp.Domain.Entities.Finance;
using GastroErp.Domain.Enums;
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
        builder.Property(a => a.Notes).HasMaxLength(1000);
        builder.Property(a => a.CreatedBy).HasMaxLength(200);
        builder.Property(a => a.UpdatedBy).HasMaxLength(200);
        builder.Property(a => a.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(a => !a.IsDeleted);
        builder.HasIndex(a => new { a.TenantId, a.AccountNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(a => a.ParentAccountId).HasFilter("[IsDeleted] = 0 AND [ParentAccountId] IS NOT NULL");
        builder.HasIndex(a => new { a.TenantId, a.AccountType }).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(a => new { a.TenantId, a.IsSystemAccount }).HasFilter("[IsDeleted] = 0 AND [IsSystemAccount] = 1");
        builder.HasIndex(a => a.AccountClassificationId).HasFilter("[IsDeleted] = 0 AND [AccountClassificationId] IS NOT NULL");
    }
}

public sealed class AccountMainClassificationConfiguration : IEntityTypeConfiguration<AccountMainClassification>
{
    public void Configure(EntityTypeBuilder<AccountMainClassification> builder)
    {
        builder.ToTable("AccountMainClassifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameEn).IsRequired().HasMaxLength(200);
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);
        builder.Property(x => x.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class AccountClassificationConfiguration : IEntityTypeConfiguration<AccountClassification>
{
    public void Configure(EntityTypeBuilder<AccountClassification> builder)
    {
        builder.ToTable("AccountClassifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(80);
        builder.Property(x => x.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameEn).IsRequired().HasMaxLength(200);
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);
        builder.Property(x => x.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.TenantId, x.Number }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.MainClassificationId, x.NameAr }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class AccountingSettingsConfiguration : IEntityTypeConfiguration<AccountingSettings>
{
    public void Configure(EntityTypeBuilder<AccountingSettings> builder)
    {
        builder.ToTable("AccountingSettings");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.LevelLengthsCsv).IsRequired().HasMaxLength(50);
        builder.Property(s => s.LevelSeparator).IsRequired().HasMaxLength(5);
        builder.Property(s => s.CreatedBy).HasMaxLength(200);
        builder.Property(s => s.UpdatedBy).HasMaxLength(200);
        builder.Property(s => s.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(s => !s.IsDeleted);
        builder.HasIndex(s => s.TenantId)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0 AND [CompanyId] IS NULL");
        builder.HasIndex(s => new { s.TenantId, s.CompanyId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0 AND [CompanyId] IS NOT NULL");
    }
}

public sealed class FiscalPeriodConfiguration : IEntityTypeConfiguration<FiscalPeriod>
{
    public void Configure(EntityTypeBuilder<FiscalPeriod> builder)
    {
        builder.ToTable("FiscalPeriods");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Notes).HasMaxLength(1000);
        builder.Property(p => p.StartMonth).HasDefaultValue((byte)1);
        builder.Property(p => p.PeriodPolicy).HasDefaultValue(FiscalPeriodPolicy.Monthly);
        builder.Property(p => p.CreatedBy).HasMaxLength(200);
        builder.Property(p => p.UpdatedBy).HasMaxLength(200);
        builder.Property(p => p.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasMany(p => p.Details)
            .WithOne()
            .HasForeignKey(d => d.FiscalPeriodId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.Details)
            .HasField("_details")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasQueryFilter(p => !p.IsDeleted);
        builder.HasIndex(p => new { p.TenantId, p.FiscalYear }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(p => new { p.TenantId, p.Status }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class FiscalPeriodDetailConfiguration : IEntityTypeConfiguration<FiscalPeriodDetail>
{
    public void Configure(EntityTypeBuilder<FiscalPeriodDetail> builder)
    {
        builder.ToTable("FiscalPeriodDetails");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(d => d.NameEn).IsRequired().HasMaxLength(200);
        builder.HasIndex(d => new { d.FiscalPeriodId, d.PeriodNumber }).IsUnique();
        builder.HasIndex(d => d.TenantId);
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
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.CreatedBy).HasMaxLength(200);
        builder.Property(c => c.UpdatedBy).HasMaxLength(200);
        builder.Property(c => c.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.Ignore(c => c.IsActive);

        builder.HasQueryFilter(c => !c.IsDeleted);
        builder.HasIndex(c => new { c.TenantId, c.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(c => new { c.TenantId, c.NameAr }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(c => new { c.TenantId, c.Number }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(c => new { c.TenantId, c.BranchId }).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(c => c.ParentCostCenterId).HasFilter("[IsDeleted] = 0 AND [ParentCostCenterId] IS NOT NULL");
        builder.HasIndex(c => new { c.TenantId, c.CostCenterType }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class CostCenterAllowedAccountConfiguration : IEntityTypeConfiguration<CostCenterAllowedAccount>
{
    public void Configure(EntityTypeBuilder<CostCenterAllowedAccount> builder)
    {
        builder.ToTable("CostCenterAllowedAccounts");
        builder.HasKey(x => new { x.CostCenterId, x.ChartOfAccountId });
        builder.HasIndex(x => x.ChartOfAccountId);
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
        builder.Property(j => j.VoucherType).HasConversion<byte>();
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
        builder.Property(l => l.ExchangeRate).HasPrecision(18, 6);
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
        builder.HasIndex(l => l.AnalyticalAccountId).HasFilter("[AnalyticalAccountId] IS NOT NULL");
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

public sealed class DocumentTypeConfiguration : IEntityTypeConfiguration<DocumentType>
{
    public void Configure(EntityTypeBuilder<DocumentType> builder)
    {
        builder.ToTable("DocumentTypes");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Code).IsRequired().HasMaxLength(30);
        builder.Property(d => d.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(d => d.NameEn).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Description).HasMaxLength(500);
        builder.Property(d => d.Prefix).IsRequired().HasMaxLength(20);
        builder.Property(d => d.Suffix).HasMaxLength(20);
        builder.Property(d => d.CreatedBy).HasMaxLength(200);
        builder.Property(d => d.UpdatedBy).HasMaxLength(200);
        builder.Property(d => d.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasMany(d => d.LifecycleStages)
            .WithOne()
            .HasForeignKey(s => s.DocumentTypeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(d => d.LifecycleStages).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasQueryFilter(d => !d.IsDeleted);
        builder.HasIndex(d => new { d.TenantId, d.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(d => new { d.TenantId, d.NameAr }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(d => new { d.TenantId, d.Module }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class DocumentTypeLifecycleStageConfiguration : IEntityTypeConfiguration<DocumentTypeLifecycleStage>
{
    public void Configure(EntityTypeBuilder<DocumentTypeLifecycleStage> builder)
    {
        builder.ToTable("DocumentTypeLifecycleStages");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Code).IsRequired().HasMaxLength(40);
        builder.Property(s => s.NameAr).IsRequired().HasMaxLength(100);
        builder.Property(s => s.NameEn).IsRequired().HasMaxLength(100);
        builder.HasIndex(s => new { s.DocumentTypeId, s.Code }).IsUnique();
        builder.HasIndex(s => s.DocumentTypeId);
    }
}

public sealed class BankConfiguration : IEntityTypeConfiguration<Bank>
{
    public void Configure(EntityTypeBuilder<Bank> builder)
    {
        builder.ToTable("Banks");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(b => b.NameEn).HasMaxLength(200);
        builder.Property(b => b.Code).HasMaxLength(30);
        builder.Property(b => b.SwiftCode).HasMaxLength(20);
        builder.Property(b => b.DefaultIban).HasMaxLength(50);
        builder.Property(b => b.DeactivationReason).HasMaxLength(500);
        builder.Property(b => b.CreatedBy).HasMaxLength(200);
        builder.Property(b => b.UpdatedBy).HasMaxLength(200);
        builder.Property(b => b.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasMany(b => b.Accounts).WithOne().HasForeignKey(a => a.BankId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(b => b.Accounts).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasQueryFilter(b => !b.IsDeleted);
        builder.HasIndex(b => new { b.TenantId, b.NameAr }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(b => new { b.TenantId, b.Number }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(b => new { b.TenantId, b.ChartOfAccountId }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(b => new { b.TenantId, b.CompanyId, b.BranchId }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class BankAccountDetailConfiguration : IEntityTypeConfiguration<BankAccountDetail>
{
    public void Configure(EntityTypeBuilder<BankAccountDetail> builder)
    {
        builder.ToTable("BankAccountDetails");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.AccountNumber).IsRequired().HasMaxLength(50);
        builder.Property(a => a.Iban).HasMaxLength(50);
        builder.Property(a => a.MinBalance).HasPrecision(18, 4);
        builder.Property(a => a.MaxBalance).HasPrecision(18, 4);
        builder.Property(a => a.MinTransaction).HasPrecision(18, 4);
        builder.Property(a => a.MaxTransaction).HasPrecision(18, 4);
        builder.Property(a => a.DailyTransferLimit).HasPrecision(18, 4);
        builder.HasIndex(a => a.BankId);
        builder.HasIndex(a => a.CurrencyId);
    }
}

public sealed class CashBoxConfiguration : IEntityTypeConfiguration<CashBox>
{
    public void Configure(EntityTypeBuilder<CashBox> builder)
    {
        builder.ToTable("CashBoxes");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Code).IsRequired().HasMaxLength(30);
        builder.Property(b => b.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(b => b.NameEn).HasMaxLength(200);
        builder.Property(b => b.LocationName).HasMaxLength(200);
        builder.Property(b => b.Description).HasMaxLength(1000);
        builder.Property(b => b.OpeningBalance).HasPrecision(18, 4);
        builder.Property(b => b.CurrentBalance).HasPrecision(18, 4);
        builder.Property(b => b.MinBalance).HasPrecision(18, 4);
        builder.Property(b => b.MaxBalance).HasPrecision(18, 4);
        builder.Property(b => b.CreatedBy).HasMaxLength(200);
        builder.Property(b => b.UpdatedBy).HasMaxLength(200);
        builder.Property(b => b.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasMany(b => b.AuthorizedUsers).WithOne().HasForeignKey(u => u.CashBoxId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(b => b.Devices).WithOne().HasForeignKey(d => d.CashBoxId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(b => b.AuthorizedUsers).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(b => b.Devices).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasQueryFilter(b => !b.IsDeleted);
        builder.HasIndex(b => new { b.TenantId, b.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(b => new { b.TenantId, b.NameAr }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(b => new { b.TenantId, b.Number }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(b => new { b.TenantId, b.ChartOfAccountId }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(b => new { b.TenantId, b.CompanyId, b.BranchId }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class CashBoxUserConfiguration : IEntityTypeConfiguration<CashBoxUser>
{
    public void Configure(EntityTypeBuilder<CashBoxUser> builder)
    {
        builder.ToTable("CashBoxUsers");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.RoleName).HasMaxLength(100);
        builder.HasIndex(u => u.CashBoxId);
        builder.HasIndex(u => new { u.CashBoxId, u.UserId }).IsUnique();
    }
}

public sealed class CashBoxDeviceConfiguration : IEntityTypeConfiguration<CashBoxDevice>
{
    public void Configure(EntityTypeBuilder<CashBoxDevice> builder)
    {
        builder.ToTable("CashBoxDevices");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Label).HasMaxLength(200);
        builder.Property(d => d.DeviceRole).HasConversion<int>();
        builder.HasIndex(d => d.CashBoxId);
    }
}

public sealed class TaxRegistrationProfileConfiguration : IEntityTypeConfiguration<TaxRegistrationProfile>
{
    public void Configure(EntityTypeBuilder<TaxRegistrationProfile> builder)
    {
        builder.ToTable("TaxRegistrationProfiles");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.VatNumber).IsRequired().HasMaxLength(30);
        builder.Property(p => p.BranchVatNumber).HasMaxLength(30);
        builder.Property(p => p.TaxOffice).HasMaxLength(200);
        builder.Property(p => p.ActivityCode).HasMaxLength(50);
        builder.Property(p => p.ActivityNameAr).HasMaxLength(200);
        builder.Property(p => p.ActivityNameEn).HasMaxLength(200);
        builder.Property(p => p.Notes).HasMaxLength(1000);
        builder.Property(p => p.DefaultTaxRate).HasPrecision(9, 4);
        builder.Property(p => p.TaxpayerType).HasConversion<int>();
        builder.Property(p => p.Status).HasConversion<int>();
        builder.Property(p => p.CreatedBy).HasMaxLength(200);
        builder.Property(p => p.UpdatedBy).HasMaxLength(200);
        builder.Property(p => p.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.Ignore(p => p.CurrentCertificate);

        builder.HasMany(p => p.Certificates).WithOne().HasForeignKey(c => c.TaxRegistrationProfileId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.Certificates).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasQueryFilter(p => !p.IsDeleted);
        builder.HasIndex(p => new { p.TenantId, p.VatNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(p => new { p.TenantId, p.Number }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(p => new { p.TenantId, p.CompanyId, p.BranchId }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class TaxRegistrationCertificateConfiguration : IEntityTypeConfiguration<TaxRegistrationCertificate>
{
    public void Configure(EntityTypeBuilder<TaxRegistrationCertificate> builder)
    {
        builder.ToTable("TaxRegistrationCertificates");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.FileName).IsRequired().HasMaxLength(260);
        builder.Property(c => c.StoragePath).IsRequired().HasMaxLength(500);
        builder.Property(c => c.ContentType).HasMaxLength(100);
        builder.Property(c => c.DocumentNumber).HasMaxLength(50);
        builder.Property(c => c.Notes).HasMaxLength(1000);
        builder.HasIndex(c => c.TaxRegistrationProfileId);
        builder.HasIndex(c => new { c.TaxRegistrationProfileId, c.Version }).IsUnique();
    }
}

public sealed class TaxCodeConfiguration : IEntityTypeConfiguration<TaxCode>
{
    public void Configure(EntityTypeBuilder<TaxCode> builder)
    {
        builder.ToTable("TaxCodes");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Code).IsRequired().HasMaxLength(20);
        builder.Property(t => t.NameAr).IsRequired().HasMaxLength(150);
        builder.Property(t => t.NameEn).HasMaxLength(150);
        builder.Property(t => t.AppliesTo).HasConversion<int>();
        builder.Property(t => t.CalculationMethod).HasConversion<int>();
        builder.Property(t => t.CreatedBy).HasMaxLength(200);
        builder.Property(t => t.UpdatedBy).HasMaxLength(200);
        builder.Property(t => t.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasMany(t => t.Rates).WithOne().HasForeignKey(r => r.TaxCodeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(t => t.Rates)
            .HasField("_rates")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasQueryFilter(t => !t.IsDeleted);
        builder.HasIndex(t => new { t.TenantId, t.Number }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(t => new { t.TenantId, t.CompanyId, t.BranchId, t.Code })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(t => new { t.TenantId, t.CompanyId }).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(t => t.IsActive).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class TaxCodeRateConfiguration : IEntityTypeConfiguration<TaxCodeRate>
{
    public void Configure(EntityTypeBuilder<TaxCodeRate> builder)
    {
        builder.ToTable("TaxCodeRates");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Rate).HasPrecision(9, 2);
        builder.HasIndex(r => r.TaxCodeId);
        builder.HasIndex(r => new { r.TaxCodeId, r.FromDate });
    }
}

public sealed class NotificationReasonConfiguration : IEntityTypeConfiguration<NotificationReason>
{
    public void Configure(EntityTypeBuilder<NotificationReason> builder)
    {
        builder.ToTable("NotificationReasons");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Code).IsRequired().HasMaxLength(30);
        builder.Property(r => r.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(r => r.NameEn).HasMaxLength(200);
        builder.Property(r => r.NoteType).HasConversion<int>();
        builder.Property(r => r.PartyType).HasConversion<int>();
        builder.Property(r => r.CreatedBy).HasMaxLength(200);
        builder.Property(r => r.UpdatedBy).HasMaxLength(200);
        builder.Property(r => r.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(r => !r.IsDeleted);
        builder.HasIndex(r => new { r.TenantId, r.Number }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(r => new { r.TenantId, r.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(r => new { r.TenantId, r.NoteType }).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(r => new { r.TenantId, r.PartyType }).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(r => r.IsActive).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class FinancialOpeningBalanceConfiguration : IEntityTypeConfiguration<FinancialOpeningBalance>
{
    public void Configure(EntityTypeBuilder<FinancialOpeningBalance> builder)
    {
        builder.ToTable("FinancialOpeningBalances");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.DocumentNumber).IsRequired().HasMaxLength(40);
        builder.Property(d => d.Description).HasMaxLength(500);
        builder.Property(d => d.Status).HasConversion<int>();
        builder.Property(d => d.CreatedBy).HasMaxLength(200);
        builder.Property(d => d.UpdatedBy).HasMaxLength(200);
        builder.Property(d => d.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasMany(d => d.Lines).WithOne().HasForeignKey(l => l.FinancialOpeningBalanceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(d => d.Lines)
            .HasField("_lines")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasQueryFilter(d => !d.IsDeleted);
        builder.HasIndex(d => new { d.TenantId, d.Number }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(d => new { d.TenantId, d.DocumentNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(d => new { d.TenantId, d.CompanyId, d.FiscalPeriodId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0 AND [Status] = 2");
        builder.HasIndex(d => d.Status).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class FinancialOpeningBalanceLineConfiguration : IEntityTypeConfiguration<FinancialOpeningBalanceLine>
{
    public void Configure(EntityTypeBuilder<FinancialOpeningBalanceLine> builder)
    {
        builder.ToTable("FinancialOpeningBalanceLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Debit).HasPrecision(18, 2);
        builder.Property(l => l.Credit).HasPrecision(18, 2);
        builder.Property(l => l.Currency).IsRequired().HasMaxLength(3);
        builder.Property(l => l.Description).HasMaxLength(300);
        builder.HasIndex(l => l.FinancialOpeningBalanceId);
        builder.HasIndex(l => new { l.FinancialOpeningBalanceId, l.LineNumber }).IsUnique();
    }
}

public sealed class ReceiptVoucherConfiguration : IEntityTypeConfiguration<ReceiptVoucher>
{
    public void Configure(EntityTypeBuilder<ReceiptVoucher> builder)
    {
        builder.ToTable("ReceiptVouchers");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.DocumentNumber).IsRequired().HasMaxLength(40);
        builder.Property(d => d.Currency).IsRequired().HasMaxLength(3);
        builder.Property(d => d.ExchangeRate).HasPrecision(18, 6);
        builder.Property(d => d.PartyName).HasMaxLength(200);
        builder.Property(d => d.Reference).HasMaxLength(100);
        builder.Property(d => d.ChequeNumber).HasMaxLength(50);
        builder.Property(d => d.Description).HasMaxLength(500);
        builder.Property(d => d.Notes).HasMaxLength(1000);
        builder.Property(d => d.ReceiptMethod).HasConversion<byte>();
        builder.Property(d => d.PartyType).HasConversion<byte>();
        builder.Property(d => d.Status).HasConversion<int>();
        builder.Property(d => d.CreatedBy).HasMaxLength(200);
        builder.Property(d => d.UpdatedBy).HasMaxLength(200);
        builder.Property(d => d.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasMany(d => d.Lines).WithOne().HasForeignKey(l => l.ReceiptVoucherId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(d => d.Lines)
            .HasField("_lines")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasQueryFilter(d => !d.IsDeleted);
        builder.HasIndex(d => new { d.TenantId, d.Number }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(d => new { d.TenantId, d.DocumentNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(d => new { d.TenantId, d.VoucherDate }).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(d => d.Status).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(d => d.ReceiptMethod).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class ReceiptVoucherLineConfiguration : IEntityTypeConfiguration<ReceiptVoucherLine>
{
    public void Configure(EntityTypeBuilder<ReceiptVoucherLine> builder)
    {
        builder.ToTable("ReceiptVoucherLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Currency).IsRequired().HasMaxLength(3);
        builder.Property(l => l.ExchangeRate).HasPrecision(18, 6);
        builder.Property(l => l.Amount).HasPrecision(18, 2);
        builder.Property(l => l.Description).HasMaxLength(300);
        builder.Ignore(l => l.AmountInBase);
        builder.HasIndex(l => l.ReceiptVoucherId);
        builder.HasIndex(l => new { l.ReceiptVoucherId, l.LineNumber }).IsUnique();
    }
}

public sealed class FinancialNoteConfiguration : IEntityTypeConfiguration<FinancialNote>
{
    public void Configure(EntityTypeBuilder<FinancialNote> builder)
    {
        builder.ToTable("FinancialNotes");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.DocumentNumber).IsRequired().HasMaxLength(40);
        builder.Property(d => d.Currency).IsRequired().HasMaxLength(3);
        builder.Property(d => d.ExchangeRate).HasPrecision(18, 6);
        builder.Property(d => d.PartyName).HasMaxLength(200);
        builder.Property(d => d.ReferenceNumber).HasMaxLength(100);
        builder.Property(d => d.Description).HasMaxLength(500);
        builder.Property(d => d.Notes).HasMaxLength(1000);
        builder.Property(d => d.NoteKind).HasConversion<byte>();
        builder.Property(d => d.PartyType).HasConversion<int>();
        builder.Property(d => d.ReferenceType).HasConversion<byte>();
        builder.Property(d => d.Status).HasConversion<int>();
        builder.Property(d => d.CreatedBy).HasMaxLength(200);
        builder.Property(d => d.UpdatedBy).HasMaxLength(200);
        builder.Property(d => d.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasMany(d => d.Lines).WithOne().HasForeignKey(l => l.FinancialNoteId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(d => d.Lines)
            .HasField("_lines")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasQueryFilter(d => !d.IsDeleted);
        builder.HasIndex(d => new { d.TenantId, d.Number }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(d => new { d.TenantId, d.DocumentNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(d => new { d.TenantId, d.NoteDate }).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(d => d.Status).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(d => d.NoteKind).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class FinancialNoteLineConfiguration : IEntityTypeConfiguration<FinancialNoteLine>
{
    public void Configure(EntityTypeBuilder<FinancialNoteLine> builder)
    {
        builder.ToTable("FinancialNoteLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Currency).IsRequired().HasMaxLength(3);
        builder.Property(l => l.ExchangeRate).HasPrecision(18, 6);
        builder.Property(l => l.Amount).HasPrecision(18, 2);
        builder.Property(l => l.Description).HasMaxLength(300);
        builder.Ignore(l => l.AmountInBase);
        builder.HasIndex(l => l.FinancialNoteId);
        builder.HasIndex(l => new { l.FinancialNoteId, l.LineNumber }).IsUnique();
        builder.HasIndex(l => l.NotificationReasonId);
    }
}

public sealed class GeneralLedgerSettingConfiguration : IEntityTypeConfiguration<GeneralLedgerSetting>
{
    public void Configure(EntityTypeBuilder<GeneralLedgerSetting> builder)
    {
        builder.ToTable("GeneralLedgerSettings");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.ClosingMethod).HasConversion<int>();
        builder.Property(s => s.CreatedBy).HasMaxLength(200);
        builder.Property(s => s.UpdatedBy).HasMaxLength(200);
        builder.Property(s => s.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        builder.HasQueryFilter(s => !s.IsDeleted);
        builder.HasIndex(s => s.TenantId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(s => s.CompanyId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(s => s.BranchId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(s => new { s.TenantId, s.Number }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(s => new { s.TenantId, s.CompanyId, s.BranchId }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("Currencies");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code).IsRequired().HasMaxLength(3);
        builder.Property(c => c.NameAr).IsRequired().HasMaxLength(200);
        builder.Property(c => c.NameEn).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Symbol).HasMaxLength(10);
        builder.Property(c => c.SubUnitNameAr).HasMaxLength(50);
        builder.Property(c => c.SubUnitNameEn).HasMaxLength(50);
        builder.Property(c => c.CurrentExchangeRate).HasPrecision(18, 6);
        builder.Property(c => c.LastExchangeRateBy).HasMaxLength(200);
        builder.Property(c => c.CreatedBy).HasMaxLength(200);
        builder.Property(c => c.UpdatedBy).HasMaxLength(200);
        builder.Property(c => c.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.Ignore(c => c.IsActive);
        builder.Ignore(c => c.IsForeignCurrency);

        builder.HasQueryFilter(c => !c.IsDeleted);
        builder.HasIndex(c => new { c.TenantId, c.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(c => new { c.TenantId, c.NameAr }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(c => new { c.TenantId, c.Number }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(c => new { c.TenantId, c.IsCompanyCurrency })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0 AND [IsCompanyCurrency] = 1");
    }
}

public sealed class CurrencyExchangeRateConfiguration : IEntityTypeConfiguration<CurrencyExchangeRate>
{
    public void Configure(EntityTypeBuilder<CurrencyExchangeRate> builder)
    {
        builder.ToTable("CurrencyExchangeRates");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Rate).HasPrecision(18, 6);
        builder.Property(r => r.ChangeReason).HasMaxLength(500);
        builder.Property(r => r.CreatedBy).HasMaxLength(200);
        builder.Property(r => r.UpdatedBy).HasMaxLength(200);
        builder.Property(r => r.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.Ignore(r => r.IsOpen);

        builder.HasQueryFilter(r => !r.IsDeleted);
        builder.HasIndex(r => new { r.TenantId, r.Number }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(r => new { r.TenantId, r.CurrencyId, r.StartDate })
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(r => r.CurrencyId).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(r => new { r.TenantId, r.CurrencyId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0 AND [EndDate] IS NULL");
    }
}
