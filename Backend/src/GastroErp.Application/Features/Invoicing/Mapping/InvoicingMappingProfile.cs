using AutoMapper;
using GastroErp.Application.Features.Invoicing.DTOs;
using GastroErp.Domain.Entities.Invoicing;

namespace GastroErp.Application.Features.Invoicing.Mapping;

public class InvoicingMappingProfile : Profile
{
    public InvoicingMappingProfile()
    {
        CreateMap<Invoice, InvoiceDto>()
            .ForMember(d => d.RemainingBalance, o => o.MapFrom(s => s.RemainingBalance));

        CreateMap<Invoice, InvoiceDetailDto>()
            .ForMember(d => d.RemainingBalance, o => o.MapFrom(s => s.RemainingBalance))
            .ForMember(d => d.Lines, o => o.MapFrom(s => s.Lines))
            .ForMember(d => d.TaxLines, o => o.MapFrom(s => s.TaxLines));

        CreateMap<InvoiceLine, InvoiceLineDto>();
        CreateMap<InvoiceTaxLine, InvoiceTaxLineDto>();

        CreateMap<TaxRate, TaxRateDto>();
        CreateMap<TaxGroup, TaxGroupDto>()
            .ForMember(d => d.Rates, o => o.MapFrom(s => s.Rates));
        CreateMap<TaxGroupRate, TaxGroupRateDto>();

        CreateMap<CreditNote, CreditNoteDto>()
            .ForMember(d => d.Lines, o => o.MapFrom(s => s.Lines));
        CreateMap<CreditNoteLine, CreditNoteLineDto>();

        CreateMap<DebitNote, DebitNoteDto>()
            .ForMember(d => d.Lines, o => o.MapFrom(s => s.Lines));
        CreateMap<DebitNoteLine, DebitNoteLineDto>();
    }
}
