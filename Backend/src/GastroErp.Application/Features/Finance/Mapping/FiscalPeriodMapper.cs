using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Domain.Entities.Finance;
using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Finance.Mapping;

internal static class FiscalPeriodMapper
{
    public static FiscalPeriodDto ToDto(FiscalPeriod period)
    {
        var details = period.Details
            .OrderBy(d => d.PeriodNumber)
            .Select(d => new FiscalPeriodDetailDto(
                d.Id,
                d.PeriodNumber,
                d.NameAr,
                d.NameEn,
                d.StartDate,
                d.EndDate,
                d.Status))
            .ToList();

        return new FiscalPeriodDto(
            period.Id,
            period.FiscalYear,
            period.StartMonth,
            period.Name,
            period.StartDate,
            period.EndDate,
            period.Notes,
            period.PeriodPolicy,
            period.PeriodPolicy.ToString().ToUpperInvariant(),
            period.Status,
            period.Status.ToString().ToUpperInvariant(),
            details,
            period.CreatedAt,
            period.UpdatedAt);
    }
}
