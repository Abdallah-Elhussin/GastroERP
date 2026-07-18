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
        CreateMap<FiscalPeriod, FiscalPeriodDto>();
        CreateMap<CostCenter, CostCenterDto>();
        CreateMap<JournalEntry, JournalDto>()
            .ForMember(d => d.TotalDebit, o => o.MapFrom(s => s.Lines.Sum(l => l.Debit)))
            .ForMember(d => d.TotalCredit, o => o.MapFrom(s => s.Lines.Sum(l => l.Credit)));
        CreateMap<JournalEntryLine, JournalLineDetailDto>()
            .ForMember(d => d.AccountNumber, o => o.Ignore())
            .ForMember(d => d.AccountName, o => o.Ignore());
    }
}
