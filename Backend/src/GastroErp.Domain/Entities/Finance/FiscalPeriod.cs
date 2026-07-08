using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Finance;

namespace GastroErp.Domain.Entities.Finance;

/// <summary>FiscalPeriod — فترة مالية (Aggregate Root)</summary>
public sealed class FiscalPeriod : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public int FiscalYear { get; private set; }
    public string Name { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public FiscalPeriodStatus Status { get; private set; }

    private FiscalPeriod() { Name = string.Empty; }

    public static FiscalPeriod Create(
        Guid tenantId, int fiscalYear, string name, DateOnly startDate, DateOnly endDate)
    {
        if (endDate <= startDate) throw new BusinessException(ErrorCodes.InvalidFiscalPeriodDates);

        return new FiscalPeriod
        {
            TenantId = tenantId,
            FiscalYear = fiscalYear,
            Name = name,
            StartDate = startDate,
            EndDate = endDate,
            Status = FiscalPeriodStatus.Open
        };
    }

    public bool ContainsDate(DateOnly date) => date >= StartDate && date <= EndDate;

    public void Close()
    {
        if (Status != FiscalPeriodStatus.Open)
            throw new BusinessException(ErrorCodes.FiscalPeriodNotOpen);
        Status = FiscalPeriodStatus.Closed;
        RaiseDomainEvent(new FiscalPeriodClosedEvent(Id, TenantId, FiscalYear));
    }

    public void Lock()
    {
        if (Status == FiscalPeriodStatus.Locked)
            throw new BusinessException(ErrorCodes.FiscalPeriodAlreadyLocked);
        Status = FiscalPeriodStatus.Locked;
    }

    public void Reopen()
    {
        if (Status == FiscalPeriodStatus.Locked)
            throw new BusinessException(ErrorCodes.FiscalPeriodLocked);
        Status = FiscalPeriodStatus.Open;
    }

    public void EnsureAcceptsPostings()
    {
        if (Status != FiscalPeriodStatus.Open)
            throw new BusinessException(ErrorCodes.FiscalPeriodClosed);
    }
}
