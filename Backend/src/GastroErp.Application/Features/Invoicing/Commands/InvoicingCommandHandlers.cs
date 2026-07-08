using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Invoicing.Commands;
using GastroErp.Application.Features.Invoicing.DTOs;
using GastroErp.Application.Features.Invoicing.Services;
using GastroErp.Domain.Entities.Invoicing;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Invoicing.Commands;

public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, Result<InvoiceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IInvoiceNumberGenerator _numberGenerator;
    private readonly IInvoiceGenerationService _generationService;

    public CreateInvoiceCommandHandler(
        IApplicationDbContext context, IMapper mapper,
        IInvoiceNumberGenerator numberGenerator, IInvoiceGenerationService generationService)
        => (_context, _mapper, _numberGenerator, _generationService) = (context, mapper, numberGenerator, generationService);

    public async Task<Result<InvoiceDto>> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        if (request.Dto.SalesOrderId.HasValue)
        {
            var genResult = await _generationService.GenerateFromOrderAsync(
                request.Dto.SalesOrderId.Value, request.Dto.InvoiceType, cancellationToken);
            if (!genResult.IsSuccess) return Result<InvoiceDto>.Failure(genResult.ErrorCode!, genResult.ErrorMessage!);
            var invoice = genResult.Data!;
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<InvoiceDto>.Success(_mapper.Map<InvoiceDto>(invoice));
        }

        var branch = await _context.Branches.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.Dto.BranchId, cancellationToken);
        if (branch is null) return Result<InvoiceDto>.Failure("NotFound", "Branch not found.");

        var invoiceNumber = await _numberGenerator.GenerateAsync(
            request.TenantId, request.Dto.BranchId, request.Dto.InvoiceType, cancellationToken);

        var manual = Invoice.CreateDraft(
            request.TenantId, branch.CompanyId, request.Dto.BranchId, invoiceNumber,
            request.Dto.InvoiceType, customerId: request.Dto.CustomerId,
            customerName: request.Dto.CustomerName, notes: request.Dto.Notes);

        _context.Invoices.Add(manual);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<InvoiceDto>.Success(_mapper.Map<InvoiceDto>(manual));
    }
}

public class FinalizeInvoiceCommandHandler : IRequestHandler<FinalizeInvoiceCommand, Result<InvoiceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IFiscalValidationService _fiscalValidation;

    public FinalizeInvoiceCommandHandler(
        IApplicationDbContext context, IMapper mapper, IFiscalValidationService fiscalValidation)
        => (_context, _mapper, _fiscalValidation) = (context, mapper, fiscalValidation);

    public async Task<Result<InvoiceDto>> Handle(FinalizeInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Lines).Include(i => i.TaxLines)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (invoice is null) return Result<InvoiceDto>.Failure("NotFound", "Invoice not found.");

        var company = await _context.Companies.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == invoice.CompanyId, cancellationToken);

        var validation = _fiscalValidation.ValidateForFinalization(invoice, company?.TaxNumber);
        if (!validation.IsSuccess) return Result<InvoiceDto>.Failure(validation.ErrorCode!, validation.ErrorMessage!);

        invoice.Finalize(request.UserId);
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<InvoiceDto>.Success(_mapper.Map<InvoiceDto>(invoice));
    }
}

public class CancelInvoiceCommandHandler : IRequestHandler<CancelInvoiceCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public CancelInvoiceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(CancelInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (invoice is null) return Result.Failure("NotFound", "Invoice not found.");

        invoice.Cancel(request.Dto.Reason, request.UserId, request.Dto.AuthorizedBy);
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class PrintInvoiceCommandHandler : IRequestHandler<PrintInvoiceCommand, Result<PrintInvoiceResultDto>>
{
    private readonly IReceiptPrintingService _printingService;
    private readonly IApplicationDbContext _context;

    public PrintInvoiceCommandHandler(IReceiptPrintingService printingService, IApplicationDbContext context)
        => (_printingService, _context) = (printingService, context);

    public async Task<Result<PrintInvoiceResultDto>> Handle(PrintInvoiceCommand request, CancellationToken cancellationToken)
    {
        var result = await _printingService.PrintInvoiceAsync(request.Id, request.Dto.PrinterName, cancellationToken);
        if (result.IsSuccess) await _context.SaveChangesAsync(cancellationToken);
        return result;
    }
}

public class CreateTaxRateCommandHandler : IRequestHandler<CreateTaxRateCommand, Result<TaxRateDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateTaxRateCommandHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<TaxRateDto>> Handle(CreateTaxRateCommand request, CancellationToken cancellationToken)
    {
        var rate = TaxRate.Create(
            request.TenantId, request.Dto.Code, request.Dto.NameAr, request.Dto.TaxType,
            request.Dto.CalculationMethod, request.Dto.Rate, request.Dto.IsInclusive,
            request.Dto.NameEn, request.Dto.FixedAmount, request.Dto.Description);

        _context.TaxRates.Add(rate);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<TaxRateDto>.Success(_mapper.Map<TaxRateDto>(rate));
    }
}

public class UpdateTaxRateCommandHandler : IRequestHandler<UpdateTaxRateCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateTaxRateCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateTaxRateCommand request, CancellationToken cancellationToken)
    {
        var rate = await _context.TaxRates.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (rate is null) return Result.Failure("NotFound", "Tax rate not found.");

        rate.Update(request.Dto.NameAr, request.Dto.TaxType, request.Dto.CalculationMethod,
            request.Dto.Rate, request.Dto.IsInclusive, request.Dto.NameEn,
            request.Dto.FixedAmount, request.Dto.Description);

        _context.TaxRates.Update(rate);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class DeleteTaxRateCommandHandler : IRequestHandler<DeleteTaxRateCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteTaxRateCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(DeleteTaxRateCommand request, CancellationToken cancellationToken)
    {
        var rate = await _context.TaxRates.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (rate is null) return Result.Failure("NotFound", "Tax rate not found.");

        rate.Deactivate();
        _context.TaxRates.Update(rate);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class CreateTaxGroupCommandHandler : IRequestHandler<CreateTaxGroupCommand, Result<TaxGroupDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateTaxGroupCommandHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<TaxGroupDto>> Handle(CreateTaxGroupCommand request, CancellationToken cancellationToken)
    {
        var group = TaxGroup.Create(request.TenantId, request.Dto.NameAr, request.Dto.NameEn, request.Dto.Description);
        _context.TaxGroups.Add(group);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<TaxGroupDto>.Success(_mapper.Map<TaxGroupDto>(group));
    }
}

public class UpdateTaxGroupCommandHandler : IRequestHandler<UpdateTaxGroupCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateTaxGroupCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateTaxGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _context.TaxGroups.FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
        if (group is null) return Result.Failure("NotFound", "Tax group not found.");

        group.Update(request.Dto.NameAr, request.Dto.NameEn, request.Dto.Description);
        _context.TaxGroups.Update(group);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class AddTaxGroupRateCommandHandler : IRequestHandler<AddTaxGroupRateCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public AddTaxGroupRateCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(AddTaxGroupRateCommand request, CancellationToken cancellationToken)
    {
        var group = await _context.TaxGroups
            .Include(g => g.Rates)
            .FirstOrDefaultAsync(g => g.Id == request.TaxGroupId, cancellationToken);
        if (group is null) return Result.Failure("NotFound", "Tax group not found.");

        var rateExists = await _context.TaxRates.AnyAsync(r => r.Id == request.Dto.TaxRateId, cancellationToken);
        if (!rateExists) return Result.Failure("NotFound", "Tax rate not found.");

        group.AddRate(request.Dto.TaxRateId, request.Dto.SortOrder);
        _context.TaxGroups.Update(group);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class RemoveTaxGroupRateCommandHandler : IRequestHandler<RemoveTaxGroupRateCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RemoveTaxGroupRateCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(RemoveTaxGroupRateCommand request, CancellationToken cancellationToken)
    {
        var group = await _context.TaxGroups
            .Include(g => g.Rates)
            .FirstOrDefaultAsync(g => g.Id == request.TaxGroupId, cancellationToken);
        if (group is null) return Result.Failure("NotFound", "Tax group not found.");

        group.RemoveRate(request.TaxRateId);
        _context.TaxGroups.Update(group);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class CreateCreditNoteCommandHandler : IRequestHandler<CreateCreditNoteCommand, Result<CreditNoteDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IInvoiceNumberGenerator _numberGenerator;

    public CreateCreditNoteCommandHandler(
        IApplicationDbContext context, IMapper mapper, IInvoiceNumberGenerator numberGenerator)
        => (_context, _mapper, _numberGenerator) = (context, mapper, numberGenerator);

    public async Task<Result<CreditNoteDto>> Handle(CreateCreditNoteCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices.FirstOrDefaultAsync(
            i => i.Id == request.Dto.OriginalInvoiceId, cancellationToken);
        if (invoice is null) return Result<CreditNoteDto>.Failure("NotFound", "Invoice not found.");

        var number = await _numberGenerator.GenerateAsync(
            request.TenantId, request.Dto.BranchId, InvoiceType.Credit, cancellationToken);

        var creditNote = CreditNote.CreateDraft(
            request.TenantId, request.Dto.BranchId, number,
            request.Dto.OriginalInvoiceId, request.Dto.CreditType, request.Dto.Reason, invoice.Currency);

        var lineNum = 1;
        foreach (var line in request.Dto.Lines)
        {
            creditNote.AddLine(
                line.InvoiceLineId, lineNum++, line.ProductId, line.ProductNameAr, line.ProductNameEn,
                line.Quantity, line.UnitPrice, line.TaxAmount);
        }

        _context.CreditNotes.Add(creditNote);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<CreditNoteDto>.Success(_mapper.Map<CreditNoteDto>(creditNote));
    }
}

public class IssueCreditNoteCommandHandler : IRequestHandler<IssueCreditNoteCommand, Result<CreditNoteDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IFiscalValidationService _fiscalValidation;

    public IssueCreditNoteCommandHandler(
        IApplicationDbContext context, IMapper mapper, IFiscalValidationService fiscalValidation)
        => (_context, _mapper, _fiscalValidation) = (context, mapper, fiscalValidation);

    public async Task<Result<CreditNoteDto>> Handle(IssueCreditNoteCommand request, CancellationToken cancellationToken)
    {
        var creditNote = await _context.CreditNotes
            .Include(c => c.Lines)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (creditNote is null) return Result<CreditNoteDto>.Failure("NotFound", "Credit note not found.");

        var invoice = await _context.Invoices.FirstOrDefaultAsync(
            i => i.Id == creditNote.OriginalInvoiceId, cancellationToken);
        if (invoice is null) return Result<CreditNoteDto>.Failure("NotFound", "Invoice not found.");

        var validation = _fiscalValidation.ValidateCreditNote(creditNote, invoice);
        if (!validation.IsSuccess) return Result<CreditNoteDto>.Failure(validation.ErrorCode!, validation.ErrorMessage!);

        creditNote.Issue(invoice.GrandTotal, invoice.CreditedAmount);
        invoice.RecordCredit(creditNote.TotalAmount);

        _context.CreditNotes.Update(creditNote);
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<CreditNoteDto>.Success(_mapper.Map<CreditNoteDto>(creditNote));
    }
}

public class CancelCreditNoteCommandHandler : IRequestHandler<CancelCreditNoteCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public CancelCreditNoteCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(CancelCreditNoteCommand request, CancellationToken cancellationToken)
    {
        var creditNote = await _context.CreditNotes.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (creditNote is null) return Result.Failure("NotFound", "Credit note not found.");

        creditNote.Cancel(request.Dto.Reason);
        _context.CreditNotes.Update(creditNote);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class CreateDebitNoteCommandHandler : IRequestHandler<CreateDebitNoteCommand, Result<DebitNoteDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IInvoiceNumberGenerator _numberGenerator;

    public CreateDebitNoteCommandHandler(
        IApplicationDbContext context, IMapper mapper, IInvoiceNumberGenerator numberGenerator)
        => (_context, _mapper, _numberGenerator) = (context, mapper, numberGenerator);

    public async Task<Result<DebitNoteDto>> Handle(CreateDebitNoteCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices.FirstOrDefaultAsync(
            i => i.Id == request.Dto.OriginalInvoiceId, cancellationToken);
        if (invoice is null) return Result<DebitNoteDto>.Failure("NotFound", "Invoice not found.");

        var number = await _numberGenerator.GenerateAsync(
            request.TenantId, request.Dto.BranchId, InvoiceType.Debit, cancellationToken);

        var debitNote = DebitNote.CreateDraft(
            request.TenantId, request.Dto.BranchId, number,
            request.Dto.OriginalInvoiceId, request.Dto.Reason, invoice.Currency);

        var lineNum = 1;
        foreach (var line in request.Dto.Lines)
        {
            debitNote.AddLine(lineNum++, line.DescriptionAr, line.DescriptionEn,
                line.Quantity, line.UnitPrice, line.TaxAmount);
        }

        _context.DebitNotes.Add(debitNote);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<DebitNoteDto>.Success(_mapper.Map<DebitNoteDto>(debitNote));
    }
}

public class IssueDebitNoteCommandHandler : IRequestHandler<IssueDebitNoteCommand, Result<DebitNoteDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IFiscalValidationService _fiscalValidation;

    public IssueDebitNoteCommandHandler(
        IApplicationDbContext context, IMapper mapper, IFiscalValidationService fiscalValidation)
        => (_context, _mapper, _fiscalValidation) = (context, mapper, fiscalValidation);

    public async Task<Result<DebitNoteDto>> Handle(IssueDebitNoteCommand request, CancellationToken cancellationToken)
    {
        var debitNote = await _context.DebitNotes
            .Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (debitNote is null) return Result<DebitNoteDto>.Failure("NotFound", "Debit note not found.");

        var invoice = await _context.Invoices.FirstOrDefaultAsync(
            i => i.Id == debitNote.OriginalInvoiceId, cancellationToken);
        if (invoice is null) return Result<DebitNoteDto>.Failure("NotFound", "Invoice not found.");

        var validation = _fiscalValidation.ValidateDebitNote(debitNote, invoice);
        if (!validation.IsSuccess) return Result<DebitNoteDto>.Failure(validation.ErrorCode!, validation.ErrorMessage!);

        debitNote.Issue();
        _context.DebitNotes.Update(debitNote);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<DebitNoteDto>.Success(_mapper.Map<DebitNoteDto>(debitNote));
    }
}

public class CancelDebitNoteCommandHandler : IRequestHandler<CancelDebitNoteCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public CancelDebitNoteCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(CancelDebitNoteCommand request, CancellationToken cancellationToken)
    {
        var debitNote = await _context.DebitNotes.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (debitNote is null) return Result.Failure("NotFound", "Debit note not found.");

        debitNote.Cancel(request.Dto.Reason);
        _context.DebitNotes.Update(debitNote);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
