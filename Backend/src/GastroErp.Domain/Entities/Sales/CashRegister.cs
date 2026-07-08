using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Sales;

namespace GastroErp.Domain.Entities.Sales;

/// <summary>CashRegister — الخزينة (Aggregate Root)</summary>
public sealed class CashRegister : AuditableBaseEntity, ITenantEntity, IBranchEntity
{
    public Guid TenantId { get; private set; }
    public Guid BranchId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string Code { get; private set; }
    public bool IsActive { get; private set; }
    public RegisterStatus Status { get; private set; }
    public decimal OpeningBalance { get; private set; }
    public decimal ClosingBalance { get; private set; }
    public decimal ExpectedBalance { get; private set; }
    public decimal ActualBalance { get; private set; }
    public decimal Difference { get; private set; }
    public decimal CurrentBalance { get; private set; }
    public decimal DefaultOpeningFloat { get; private set; }
    public Guid? OpenedBy { get; private set; }
    public DateTimeOffset? OpenedAt { get; private set; }
    public Guid? ClosedBy { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }

    private CashRegister()
    {
        NameAr = string.Empty;
        Code = string.Empty;
    }

    public static CashRegister Create(Guid tenantId, Guid branchId, string nameAr, string code,
        string? nameEn = null, decimal defaultOpeningFloat = 0)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (branchId == Guid.Empty) throw new ArgumentException("BranchId cannot be empty.", nameof(branchId));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code cannot be empty.", nameof(code));

        return new CashRegister
        {
            TenantId = tenantId,
            BranchId = branchId,
            NameAr = nameAr,
            NameEn = nameEn,
            Code = code.ToUpperInvariant(),
            DefaultOpeningFloat = defaultOpeningFloat,
            IsActive = true,
            Status = RegisterStatus.Closed,
            CurrentBalance = 0
        };
    }

    public void Open(decimal openingBalance, Guid openedBy)
    {
        if (!IsActive) throw new BusinessException(ErrorCodes.RegisterClosed);
        if (Status == RegisterStatus.Open) throw new BusinessException(ErrorCodes.RegisterAlreadyOpen);
        if (openingBalance < 0) throw new BusinessException(ErrorCodes.InvalidPaymentAmount);

        Status = RegisterStatus.Open;
        OpeningBalance = openingBalance;
        CurrentBalance = openingBalance;
        ExpectedBalance = openingBalance;
        OpenedBy = openedBy;
        OpenedAt = DateTimeOffset.UtcNow;
        ClosedBy = null;
        ClosedAt = null;

        RaiseDomainEvent(new RegisterOpenedEvent(Id, BranchId, openedBy));
    }

    public void Suspend()
    {
        if (Status != RegisterStatus.Open) throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        Status = RegisterStatus.Suspended;
    }

    public void Resume()
    {
        if (Status != RegisterStatus.Suspended) throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        Status = RegisterStatus.Open;
    }

    public void Close(decimal actualBalance, Guid closedBy)
    {
        if (Status is not (RegisterStatus.Open or RegisterStatus.Suspended))
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);

        ActualBalance = actualBalance;
        ClosingBalance = actualBalance;
        Difference = actualBalance - ExpectedBalance;
        CurrentBalance = actualBalance;
        Status = RegisterStatus.Closed;
        ClosedBy = closedBy;
        ClosedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new RegisterClosedEvent(Id, actualBalance, Difference));
    }

    public void RecordMovement(decimal amount, bool isInflow)
    {
        if (Status != RegisterStatus.Open) throw new BusinessException(ErrorCodes.RegisterClosed);
        CurrentBalance = isInflow ? CurrentBalance + amount : CurrentBalance - amount;
        ExpectedBalance = CurrentBalance;
    }

    public void UpdateInfo(string nameAr, string? nameEn)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        NameAr = nameAr;
        NameEn = nameEn;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
