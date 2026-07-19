using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Finance;

/// <summary>Credit note vs debit note for accounting notification reasons.</summary>
public enum NotificationNoteType
{
    Debit = 1,
    Credit = 2
}

/// <summary>Party applicable to an accounting notification reason.</summary>
public enum NotificationPartyType
{
    Customer = 1,
    Supplier = 2,
    Employee = 3,
    SalesRep = 4,
    GeneralAccount = 5,
    Other = 6
}

/// <summary>
/// Master reason codes for credit/debit notes and financial adjustments
/// (مرتجعات، خصومات، فروقات أسعار، غرامات، …).
/// </summary>
public sealed class NotificationReason : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public int Number { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string NameAr { get; private set; } = string.Empty;
    public string? NameEn { get; private set; }
    public NotificationNoteType NoteType { get; private set; } = NotificationNoteType.Credit;
    public NotificationPartyType PartyType { get; private set; } = NotificationPartyType.Customer;
    public Guid CounterpartAccountId { get; private set; }
    public bool UsesTax { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool HasBeenUsed { get; private set; }

    private NotificationReason() { }

    public static NotificationReason Create(
        Guid tenantId,
        int number,
        string code,
        string nameAr,
        NotificationNoteType noteType,
        NotificationPartyType partyType,
        Guid counterpartAccountId,
        string? nameEn = null,
        bool usesTax = false,
        bool isActive = true)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId is required.", nameof(tenantId));
        if (number < 1) throw new ArgumentOutOfRangeException(nameof(number));
        if (counterpartAccountId == Guid.Empty)
            throw new BusinessException(ErrorCodes.RequiredField, "Counterpart account is required.");

        var entity = new NotificationReason
        {
            TenantId = tenantId,
            Number = number,
            NoteType = noteType,
            PartyType = partyType,
            CounterpartAccountId = counterpartAccountId,
            UsesTax = usesTax,
            IsActive = isActive
        };
        entity.SetCode(code);
        entity.SetNames(nameAr, nameEn);
        return entity;
    }

    public void Update(
        string code,
        string nameAr,
        string? nameEn,
        NotificationNoteType noteType,
        NotificationPartyType partyType,
        Guid counterpartAccountId,
        bool usesTax,
        bool isActive)
    {
        if (counterpartAccountId == Guid.Empty)
            throw new BusinessException(ErrorCodes.RequiredField, "Counterpart account is required.");

        SetCode(code);
        SetNames(nameAr, nameEn);
        NoteType = noteType;
        PartyType = partyType;
        CounterpartAccountId = counterpartAccountId;
        UsesTax = usesTax;
        IsActive = isActive;
    }

    public void SetActive(bool isActive) => IsActive = isActive;

    public void MarkAsUsed() => HasBeenUsed = true;

    public void EnsureCanDelete()
    {
        if (HasBeenUsed)
            throw new BusinessException(ErrorCodes.NotificationReasonInUse,
                "Notification reason is used in financial documents and cannot be deleted. Deactivate it instead.");
    }

    private void SetCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new BusinessException(ErrorCodes.RequiredField, "Reason code is required.");
        var value = code.Trim().ToUpperInvariant();
        if (value.Length > 30)
            throw new BusinessException(ErrorCodes.RequiredField, "Reason code max length is 30.");
        Code = value;
    }

    private void SetNames(string nameAr, string? nameEn)
    {
        if (string.IsNullOrWhiteSpace(nameAr))
            throw new BusinessException(ErrorCodes.RequiredField, "Reason name is required.");
        NameAr = nameAr.Trim();
        if (NameAr.Length > 200)
            throw new BusinessException(ErrorCodes.RequiredField, "Reason name max length is 200.");
        NameEn = string.IsNullOrWhiteSpace(nameEn) ? null : nameEn.Trim();
        if (NameEn is { Length: > 200 })
            throw new BusinessException(ErrorCodes.RequiredField, "English name max length is 200.");
    }
}
