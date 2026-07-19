using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Finance;

namespace GastroErp.Domain.Entities.Finance;

/// <summary>FiscalPeriod — السنة المالية / الفترة المالية (Aggregate Root)</summary>
public sealed class FiscalPeriod : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public int FiscalYear { get; private set; }
    public byte StartMonth { get; private set; }
    public string Name { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public string? Notes { get; private set; }
    public FiscalPeriodPolicy PeriodPolicy { get; private set; }
    public FiscalPeriodStatus Status { get; private set; }

    private readonly List<FiscalPeriodDetail> _details = [];
    public IReadOnlyCollection<FiscalPeriodDetail> Details => _details.AsReadOnly();

    private FiscalPeriod()
    {
        Name = string.Empty;
        StartMonth = 1;
        PeriodPolicy = FiscalPeriodPolicy.Monthly;
    }

    public static FiscalPeriod Create(
        Guid tenantId,
        int fiscalYear,
        byte startMonth,
        string? notes = null,
        FiscalPeriodPolicy periodPolicy = FiscalPeriodPolicy.Monthly)
    {
        if (fiscalYear is < 2000 or > 2100)
            throw new BusinessException(ErrorCodes.InvalidFiscalPeriodDates);
        if (startMonth is < 1 or > 12)
            throw new BusinessException(ErrorCodes.InvalidFiscalPeriodDates);

        var (start, end) = CalculateYearRange(fiscalYear, startMonth);

        return new FiscalPeriod
        {
            TenantId = tenantId,
            FiscalYear = fiscalYear,
            StartMonth = startMonth,
            Name = $"FY {fiscalYear}",
            StartDate = start,
            EndDate = end,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            PeriodPolicy = periodPolicy,
            Status = FiscalPeriodStatus.Open
        };
    }

    public static (DateOnly Start, DateOnly End) CalculateYearRange(int fiscalYear, byte startMonth)
    {
        var start = new DateOnly(fiscalYear, startMonth, 1);
        var endMonth = startMonth == 1 ? 12 : startMonth - 1;
        var endYear = startMonth == 1 ? fiscalYear : fiscalYear + 1;
        var end = new DateOnly(endYear, endMonth, DateTime.DaysInMonth(endYear, endMonth));
        return (start, end);
    }

    public void Update(byte startMonth, string? notes)
    {
        if (Status == FiscalPeriodStatus.Locked)
            throw new BusinessException(ErrorCodes.FiscalPeriodLocked);
        if (startMonth is < 1 or > 12)
            throw new BusinessException(ErrorCodes.InvalidFiscalPeriodDates);

        StartMonth = startMonth;
        var (start, end) = CalculateYearRange(FiscalYear, startMonth);
        StartDate = start;
        EndDate = end;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public void GenerateMonthlyDetails()
    {
        if (Status == FiscalPeriodStatus.Locked)
            throw new BusinessException(ErrorCodes.FiscalPeriodLocked);
        if (PeriodPolicy != FiscalPeriodPolicy.Monthly)
            throw new BusinessException(ErrorCodes.InvalidFiscalPeriodDates);

        _details.Clear();
        var cursor = StartDate;
        for (var i = 1; i <= 12; i++)
        {
            var monthEnd = new DateOnly(
                cursor.Year,
                cursor.Month,
                DateTime.DaysInMonth(cursor.Year, cursor.Month));
            if (monthEnd > EndDate)
                monthEnd = EndDate;

            _details.Add(FiscalPeriodDetail.Create(
                Id,
                TenantId,
                i,
                $"الفترة المالية {i}",
                $"Financial Period {i}",
                cursor,
                monthEnd));

            cursor = monthEnd.AddDays(1);
            if (cursor > EndDate)
                break;
        }
    }

    public void SetDetailStatus(Guid detailId, FiscalPeriodStatus status)
    {
        if (Status == FiscalPeriodStatus.Locked)
            throw new BusinessException(ErrorCodes.FiscalPeriodLocked);

        var detail = _details.FirstOrDefault(d => d.Id == detailId)
            ?? throw new BusinessException(ErrorCodes.FiscalPeriodNotFound);
        detail.SetStatus(status);
    }

    public bool ContainsDate(DateOnly date) => date >= StartDate && date <= EndDate;

    public void Close()
    {
        if (Status != FiscalPeriodStatus.Open)
            throw new BusinessException(ErrorCodes.FiscalPeriodNotOpen);
        Status = FiscalPeriodStatus.Closed;
        foreach (var d in _details.Where(x => x.Status == FiscalPeriodStatus.Open))
            d.SetStatus(FiscalPeriodStatus.Closed);
        RaiseDomainEvent(new FiscalPeriodClosedEvent(Id, TenantId, FiscalYear));
    }

    public void Lock()
    {
        if (Status == FiscalPeriodStatus.Locked)
            throw new BusinessException(ErrorCodes.FiscalPeriodAlreadyLocked);
        Status = FiscalPeriodStatus.Locked;
        foreach (var d in _details)
            d.SetStatus(FiscalPeriodStatus.Locked);
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

/// <summary>FiscalPeriodDetail — فترة تفصيلية داخل السنة المالية</summary>
public sealed class FiscalPeriodDetail
{
    public Guid Id { get; private set; }
    public Guid FiscalPeriodId { get; private set; }
    public Guid TenantId { get; private set; }
    public int PeriodNumber { get; private set; }
    public string NameAr { get; private set; }
    public string NameEn { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public FiscalPeriodStatus Status { get; private set; }

    private FiscalPeriodDetail()
    {
        NameAr = string.Empty;
        NameEn = string.Empty;
    }

    public static FiscalPeriodDetail Create(
        Guid fiscalPeriodId,
        Guid tenantId,
        int periodNumber,
        string nameAr,
        string nameEn,
        DateOnly startDate,
        DateOnly endDate)
    {
        return new FiscalPeriodDetail
        {
            Id = Guid.NewGuid(),
            FiscalPeriodId = fiscalPeriodId,
            TenantId = tenantId,
            PeriodNumber = periodNumber,
            NameAr = nameAr,
            NameEn = nameEn,
            StartDate = startDate,
            EndDate = endDate,
            Status = FiscalPeriodStatus.Open
        };
    }

    public void SetStatus(FiscalPeriodStatus status) => Status = status;

    public void Rename(string nameAr, string nameEn)
    {
        if (!string.IsNullOrWhiteSpace(nameAr)) NameAr = nameAr.Trim();
        if (!string.IsNullOrWhiteSpace(nameEn)) NameEn = nameEn.Trim();
    }
}
