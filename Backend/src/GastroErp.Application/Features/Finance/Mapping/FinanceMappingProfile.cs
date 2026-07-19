using AutoMapper;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Domain.Entities.Finance;

namespace GastroErp.Application.Features.Finance.Mapping;

public class FinanceMappingProfile : Profile
{
    public FinanceMappingProfile()
    {
        CreateMap<ChartOfAccount, AccountDto>();
        CreateMap<AccountingSettings, AccountingSettingsDto>();
        CreateMap<FiscalPeriodDetail, FiscalPeriodDetailDto>();
        CreateMap<FiscalPeriod, FiscalPeriodDto>()
            .ForMember(d => d.StatusCode, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.PeriodPolicyCode, o => o.MapFrom(s => s.PeriodPolicy.ToString()))
            .ForMember(d => d.Details, o => o.MapFrom(s => s.Details.OrderBy(x => x.PeriodNumber)));
        CreateMap<JournalEntry, JournalDto>()
            .ForMember(d => d.TotalDebit, o => o.MapFrom(s => s.Lines.Sum(l => l.Debit)))
            .ForMember(d => d.TotalCredit, o => o.MapFrom(s => s.Lines.Sum(l => l.Credit)));
        CreateMap<JournalEntryLine, JournalLineDetailDto>()
            .ForMember(d => d.AccountNumber, o => o.Ignore())
            .ForMember(d => d.AccountName, o => o.Ignore());
    }
}
