using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Invoicing.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Invoicing.Commands;

// Invoice
public record CreateInvoiceCommand(Guid TenantId, Guid UserId, CreateInvoiceDto Dto) : IRequest<Result<InvoiceDto>>;
public record FinalizeInvoiceCommand(Guid Id, Guid UserId) : IRequest<Result<InvoiceDto>>;
public record CancelInvoiceCommand(Guid Id, Guid UserId, CancelInvoiceDto Dto) : IRequest<Result>;
public record PrintInvoiceCommand(Guid Id, PrintInvoiceDto Dto) : IRequest<Result<PrintInvoiceResultDto>>;

// Tax
public record CreateTaxRateCommand(Guid TenantId, CreateTaxRateDto Dto) : IRequest<Result<TaxRateDto>>;
public record UpdateTaxRateCommand(Guid Id, UpdateTaxRateDto Dto) : IRequest<Result>;
public record DeleteTaxRateCommand(Guid Id) : IRequest<Result>;
public record CreateTaxGroupCommand(Guid TenantId, CreateTaxGroupDto Dto) : IRequest<Result<TaxGroupDto>>;
public record UpdateTaxGroupCommand(Guid Id, UpdateTaxGroupDto Dto) : IRequest<Result>;
public record AddTaxGroupRateCommand(Guid TaxGroupId, AddTaxGroupRateDto Dto) : IRequest<Result>;
public record RemoveTaxGroupRateCommand(Guid TaxGroupId, Guid TaxRateId) : IRequest<Result>;

// Credit Note
public record CreateCreditNoteCommand(Guid TenantId, CreateCreditNoteDto Dto) : IRequest<Result<CreditNoteDto>>;
public record IssueCreditNoteCommand(Guid Id) : IRequest<Result<CreditNoteDto>>;
public record CancelCreditNoteCommand(Guid Id, CancelCreditNoteDto Dto) : IRequest<Result>;

// Debit Note
public record CreateDebitNoteCommand(Guid TenantId, CreateDebitNoteDto Dto) : IRequest<Result<DebitNoteDto>>;
public record IssueDebitNoteCommand(Guid Id) : IRequest<Result<DebitNoteDto>>;
public record CancelDebitNoteCommand(Guid Id, CancelDebitNoteDto Dto) : IRequest<Result>;
