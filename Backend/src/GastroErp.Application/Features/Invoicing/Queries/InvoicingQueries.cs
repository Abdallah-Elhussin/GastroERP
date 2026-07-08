using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Invoicing.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Invoicing.Queries;

public record GetInvoicesQuery(Guid TenantId, InvoiceFilterDto Filter) : IRequest<PagedResult<InvoiceDto>>;
public record GetInvoiceByIdQuery(Guid Id) : IRequest<Result<InvoiceDetailDto>>;
public record GetInvoicesByOrderQuery(Guid OrderId) : IRequest<Result<IReadOnlyList<InvoiceDto>>>;

public record GetTaxRatesQuery(Guid TenantId) : IRequest<Result<IReadOnlyList<TaxRateDto>>>;
public record GetTaxRateByIdQuery(Guid Id) : IRequest<Result<TaxRateDto>>;
public record GetTaxGroupsQuery(Guid TenantId) : IRequest<Result<IReadOnlyList<TaxGroupDto>>>;
public record GetTaxGroupByIdQuery(Guid Id) : IRequest<Result<TaxGroupDto>>;

public record GetCreditNoteByIdQuery(Guid Id) : IRequest<Result<CreditNoteDto>>;
public record GetCreditNotesByInvoiceQuery(Guid InvoiceId) : IRequest<Result<IReadOnlyList<CreditNoteDto>>>;

public record GetDebitNoteByIdQuery(Guid Id) : IRequest<Result<DebitNoteDto>>;
public record GetDebitNotesByInvoiceQuery(Guid InvoiceId) : IRequest<Result<IReadOnlyList<DebitNoteDto>>>;

// Reporting
public record GetDailySalesReportQuery(Guid TenantId, Guid BranchId, DateOnly Date) : IRequest<Result<DailySalesReportDto>>;
public record GetVatSummaryQuery(Guid TenantId, Guid BranchId, DateTimeOffset From, DateTimeOffset To) : IRequest<Result<IReadOnlyList<VatSummaryDto>>>;
public record GetInvoiceRegisterQuery(Guid TenantId, Guid? BranchId, DateTimeOffset From, DateTimeOffset To) : IRequest<Result<IReadOnlyList<InvoiceRegisterDto>>>;
public record GetOutstandingInvoicesQuery(Guid TenantId, Guid? BranchId) : IRequest<Result<IReadOnlyList<OutstandingInvoiceDto>>>;
public record GetTaxReportQuery(Guid TenantId, Guid BranchId, DateTimeOffset From, DateTimeOffset To) : IRequest<Result<TaxReportDto>>;
