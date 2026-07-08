using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Finance;

/// <summary>AccountingTransaction — سجل مصدر الترحيل (Aggregate Root)</summary>
public sealed class AccountingTransaction : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public PostingSource SourceModule { get; private set; }
    public Guid SourceDocumentId { get; private set; }
    public Guid JournalEntryId { get; private set; }
    public string ReferenceNumber { get; private set; }
    public DateTimeOffset PostedAt { get; private set; }

    private AccountingTransaction() { ReferenceNumber = string.Empty; }

    public static AccountingTransaction Create(
        Guid tenantId, PostingSource source, Guid sourceDocumentId,
        Guid journalEntryId, string referenceNumber)
    {
        return new AccountingTransaction
        {
            TenantId = tenantId,
            SourceModule = source,
            SourceDocumentId = sourceDocumentId,
            JournalEntryId = journalEntryId,
            ReferenceNumber = referenceNumber,
            PostedAt = DateTimeOffset.UtcNow
        };
    }
}
