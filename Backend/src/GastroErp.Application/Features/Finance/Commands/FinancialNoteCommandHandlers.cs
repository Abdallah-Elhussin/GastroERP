using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Application.Features.Finance.Services;
using GastroErp.Application.Features.Inventory.Services;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Entities.Finance;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainDoc = GastroErp.Domain.Entities.Finance.FinancialNote;

namespace GastroErp.Application.Features.Finance.Commands;

internal static class FinancialNoteMapper
{
    public static FinancialNoteDto ToDto(
        DomainDoc doc,
        string? companyName,
        string? branchName,
        int? fiscalYear,
        string? mainAccNo,
        string? mainAccName,
        string? journalNumber,
        IReadOnlyDictionary<Guid, (string Number, string NameAr)> accounts,
        IReadOnlyDictionary<Guid, string> costCenters,
        IReadOnlyDictionary<Guid, (string Code, string NameAr)> reasons)
    {
        var lines = doc.Lines.OrderBy(l => l.LineNumber).Select(l =>
        {
            string? offsetNo = null, offsetName = null;
            if (accounts.TryGetValue(l.OffsetAccountId, out var acc))
            {
                offsetNo = acc.Number;
                offsetName = acc.NameAr;
            }

            string? reasonCode = null, reasonName = null;
            if (reasons.TryGetValue(l.NotificationReasonId, out var reason))
            {
                reasonCode = reason.Code;
                reasonName = reason.NameAr;
            }

            string? ccName = null;
            if (l.CostCenterId is Guid ccId)
                costCenters.TryGetValue(ccId, out ccName);

            string? analyticalName = null;
            if (l.AnalyticalAccountId is Guid anId && accounts.TryGetValue(anId, out var an))
                analyticalName = an.NameAr;

            return new FinancialNoteLineDto(
                l.Id, l.NotificationReasonId, reasonCode, reasonName,
                l.OffsetAccountId, offsetNo, offsetName, l.CostCenterId, ccName,
                l.AnalyticalAccountId, analyticalName, l.Currency, l.ExchangeRate,
                l.Amount, l.AmountInBase, l.Description);
        }).ToList();

        return new FinancialNoteDto(
            doc.Id, doc.Number, doc.DocumentNumber, doc.NoteKind, doc.CompanyId, companyName,
            doc.BranchId, branchName, doc.NoteDate, doc.FiscalPeriodId, fiscalYear,
            doc.PartyType, doc.PartyId, doc.PartyName, doc.MainAccountId, mainAccNo, mainAccName,
            doc.Currency, doc.ExchangeRate, doc.ReferenceType, doc.ReferenceDocumentId, doc.ReferenceNumber,
            doc.Description, doc.Notes, doc.Status, doc.JournalEntryId, journalNumber,
            lines.Count, doc.TotalAmount, doc.TotalAmountInBase, doc.CreatedAt, doc.PostedAt,
            doc.ApprovedAt, lines);
    }

    public static void ApplyLines(DomainDoc doc, IReadOnlyList<FinancialNoteLineDto>? lines)
    {
        if (lines is null || lines.Count == 0)
        {
            doc.ReplaceLines([]);
            return;
        }

        doc.ReplaceLines(lines.Select(l => (
            l.NotificationReasonId,
            l.OffsetAccountId,
            l.Amount,
            l.CostCenterId,
            l.AnalyticalAccountId,
            (string?)l.Currency,
            (decimal?)l.ExchangeRate,
            l.Description)));
    }
}

public sealed class CreateFinancialNoteCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateFinancialNoteCommand, Result<FinancialNoteDto>>
{
    public async Task<Result<FinancialNoteDto>> Handle(
        CreateFinancialNoteCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var validation = await FinancialNoteValidation.ValidateAsync(context, request.TenantId, dto, cancellationToken);
        if (validation is not null) return validation;

        var period = await context.FiscalPeriods.AsNoTracking()
            .FirstAsync(p => p.Id == dto.FiscalPeriodId, cancellationToken);
        var prefix = dto.NoteKind == FinancialNoteKind.Debit ? "DN" : "CN";

        try
        {
            var next = await context.FinancialNotes
                .Where(d => d.TenantId == request.TenantId)
                .Select(d => (int?)d.Number).MaxAsync(cancellationToken) ?? 0;
            var number = next + 1;
            var docNumber = $"{prefix}-{period.FiscalYear}-{number:D4}";

            var doc = DomainDoc.Create(
                request.TenantId, number, docNumber, dto.NoteKind, dto.CompanyId, dto.BranchId,
                dto.NoteDate, dto.FiscalPeriodId, dto.PartyType, dto.MainAccountId, dto.Currency,
                dto.ExchangeRate, dto.PartyId, dto.PartyName, dto.ReferenceType,
                dto.ReferenceDocumentId, dto.ReferenceNumber, dto.Description, dto.Notes);

            FinancialNoteMapper.ApplyLines(doc, dto.Lines);
            context.FinancialNotes.Add(doc);
            await context.SaveChangesAsync(cancellationToken);
            return Result<FinancialNoteDto>.Success(await LoadDtoAsync(context, doc.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<FinancialNoteDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }

    internal static async Task<FinancialNoteDto> LoadDtoAsync(
        IApplicationDbContext context, Guid id, CancellationToken ct)
    {
        var doc = await context.FinancialNotes.AsNoTracking()
            .Include(d => d.Lines).FirstAsync(d => d.Id == id, ct);
        return await EnrichAsync(context, doc, ct);
    }

    internal static async Task<FinancialNoteDto> EnrichAsync(
        IApplicationDbContext context, DomainDoc doc, CancellationToken ct)
    {
        var company = await context.Companies.AsNoTracking()
            .Where(c => c.Id == doc.CompanyId).Select(c => c.NameAr).FirstOrDefaultAsync(ct);
        var branch = await context.Branches.AsNoTracking()
            .Where(b => b.Id == doc.BranchId).Select(b => b.NameAr).FirstOrDefaultAsync(ct);
        var fiscalYear = await context.FiscalPeriods.AsNoTracking()
            .Where(p => p.Id == doc.FiscalPeriodId).Select(p => (int?)p.FiscalYear).FirstOrDefaultAsync(ct);
        var main = await context.ChartOfAccounts.AsNoTracking()
            .Where(a => a.Id == doc.MainAccountId)
            .Select(a => new { a.AccountNumber, a.NameAr }).FirstOrDefaultAsync(ct);
        var journalNumber = doc.JournalEntryId is Guid jid
            ? await context.JournalEntries.AsNoTracking().Where(j => j.Id == jid).Select(j => j.EntryNumber).FirstOrDefaultAsync(ct)
            : null;

        var accountIds = doc.Lines.Select(l => l.OffsetAccountId)
            .Concat(doc.Lines.Where(l => l.AnalyticalAccountId.HasValue).Select(l => l.AnalyticalAccountId!.Value))
            .Append(doc.MainAccountId).Distinct().ToList();
        var accounts = await context.ChartOfAccounts.AsNoTracking()
            .Where(a => accountIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => (a.AccountNumber, a.NameAr), ct);

        var ccIds = doc.Lines.Where(l => l.CostCenterId.HasValue).Select(l => l.CostCenterId!.Value).Distinct().ToList();
        var costCenters = await context.CostCenters.AsNoTracking()
            .Where(c => ccIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.NameAr, ct);

        var reasonIds = doc.Lines.Select(l => l.NotificationReasonId).Distinct().ToList();
        var reasons = await context.NotificationReasons.AsNoTracking()
            .Where(r => reasonIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, r => (r.Code, r.NameAr), ct);

        return FinancialNoteMapper.ToDto(
            doc, company, branch, fiscalYear, main?.AccountNumber, main?.NameAr, journalNumber,
            accounts, costCenters, reasons);
    }
}

public sealed class UpdateFinancialNoteCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateFinancialNoteCommand, Result<FinancialNoteDto>>
{
    public async Task<Result<FinancialNoteDto>> Handle(
        UpdateFinancialNoteCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.FinancialNotes.Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (doc is null)
            return Result<FinancialNoteDto>.Failure(ErrorCodes.FinancialNoteNotFound, "Financial note not found.");

        var validation = await FinancialNoteValidation.ValidateAsync(
            context, doc.TenantId, request.Dto, cancellationToken);
        if (validation is not null) return validation;

        try
        {
            var dto = request.Dto;
            doc.Update(
                dto.CompanyId, dto.BranchId, dto.NoteKind, dto.NoteDate, dto.FiscalPeriodId, dto.PartyType,
                dto.MainAccountId, dto.Currency, dto.ExchangeRate, dto.PartyId, dto.PartyName,
                dto.ReferenceType, dto.ReferenceDocumentId, dto.ReferenceNumber, dto.Description, dto.Notes);
            FinancialNoteMapper.ApplyLines(doc, dto.Lines);
            await context.SaveChangesAsync(cancellationToken);
            return Result<FinancialNoteDto>.Success(
                await CreateFinancialNoteCommandHandler.LoadDtoAsync(context, doc.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<FinancialNoteDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class SubmitFinancialNoteCommandHandler(IApplicationDbContext context)
    : IRequestHandler<SubmitFinancialNoteCommand, Result<FinancialNoteDto>>
{
    public async Task<Result<FinancialNoteDto>> Handle(
        SubmitFinancialNoteCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.FinancialNotes.Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (doc is null)
            return Result<FinancialNoteDto>.Failure(ErrorCodes.FinancialNoteNotFound, "Financial note not found.");
        try
        {
            doc.Submit();
            await context.SaveChangesAsync(cancellationToken);
            return Result<FinancialNoteDto>.Success(
                await CreateFinancialNoteCommandHandler.LoadDtoAsync(context, doc.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<FinancialNoteDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class ApproveFinancialNoteCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ApproveFinancialNoteCommand, Result<FinancialNoteDto>>
{
    public async Task<Result<FinancialNoteDto>> Handle(
        ApproveFinancialNoteCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.FinancialNotes.Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (doc is null)
            return Result<FinancialNoteDto>.Failure(ErrorCodes.FinancialNoteNotFound, "Financial note not found.");
        try
        {
            doc.Approve(request.UserId);
            await context.SaveChangesAsync(cancellationToken);
            return Result<FinancialNoteDto>.Success(
                await CreateFinancialNoteCommandHandler.LoadDtoAsync(context, doc.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<FinancialNoteDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class PostFinancialNoteCommandHandler(
    IApplicationDbContext context,
    IJournalPostingService postingService)
    : IRequestHandler<PostFinancialNoteCommand, Result<FinancialNoteDto>>
{
    public async Task<Result<FinancialNoteDto>> Handle(
        PostFinancialNoteCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.FinancialNotes.Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (doc is null)
            return Result<FinancialNoteDto>.Failure(ErrorCodes.FinancialNoteNotFound, "Financial note not found.");

        if (doc.Status is not (FinancialNoteStatus.Approved or FinancialNoteStatus.Draft))
            return Result<FinancialNoteDto>.Failure(
                ErrorCodes.FinancialNoteNotEditable, "Only approved or draft notes can be posted.");

        var period = await context.FiscalPeriods.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == doc.FiscalPeriodId, cancellationToken);
        if (period is null || period.Status != FiscalPeriodStatus.Open)
            return Result<FinancialNoteDto>.Failure(ErrorCodes.FiscalPeriodClosed, "Fiscal period must be open to post.");

        var journalLines = new List<JournalLineDto>();
        if (doc.NoteKind == FinancialNoteKind.Debit)
        {
            journalLines.Add(new JournalLineDto(null, doc.MainAccountId, null, doc.TotalAmountInBase, 0,
                doc.Description ?? $"Debit note {doc.DocumentNumber}"));
            journalLines.AddRange(doc.Lines.Select(l =>
                new JournalLineDto(null, l.OffsetAccountId, l.CostCenterId, 0, l.AmountInBase, l.Description)));
        }
        else
        {
            journalLines.AddRange(doc.Lines.Select(l =>
                new JournalLineDto(null, l.OffsetAccountId, l.CostCenterId, l.AmountInBase, 0, l.Description)));
            journalLines.Add(new JournalLineDto(null, doc.MainAccountId, null, 0, doc.TotalAmountInBase,
                doc.Description ?? $"Credit note {doc.DocumentNumber}"));
        }

        var journalDto = new CreateJournalDto(
            doc.NoteDate,
            doc.Description ?? $"Financial note {doc.DocumentNumber}",
            doc.PostingSource,
            doc.BranchId,
            doc.CompanyId,
            doc.ReferenceNumber ?? doc.DocumentNumber,
            doc.Id,
            journalLines);

        var posted = await postingService.CreateAndPostAsync(
            doc.TenantId, request.UserId, journalDto, cancellationToken);
        if (!posted.IsSuccess)
            return Result<FinancialNoteDto>.Failure(
                posted.ErrorCode!, posted.ErrorMessage ?? "Failed to post financial note journal.");

        try
        {
            doc.MarkPosted(posted.Data!.Id, request.UserId);
            context.AccountingTransactions.Add(AccountingTransaction.Create(
                doc.TenantId, doc.PostingSource, doc.Id, posted.Data.Id, doc.DocumentNumber));
            await ApplyPurchaseInvoiceSettlementAsync(context, doc, apply: true, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return Result<FinancialNoteDto>.Success(
                await CreateFinancialNoteCommandHandler.LoadDtoAsync(context, doc.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<FinancialNoteDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }

    private static async Task ApplyPurchaseInvoiceSettlementAsync(
        IApplicationDbContext context,
        DomainDoc doc,
        bool apply,
        CancellationToken cancellationToken)
    {
        if (doc.ReferenceType != FinancialNoteReferenceType.PurchaseInvoice
            || doc.ReferenceDocumentId is not Guid invoiceId
            || doc.PartyType != NotificationPartyType.Supplier)
            return;

        // Credit note to supplier reduces AP → settle invoice; reverse undoes it.
        if (doc.NoteKind != FinancialNoteKind.Credit)
            return;

        if (apply)
            await PurchaseInvoiceSettlement.ApplyAsync(
                context, doc.TenantId, invoiceId, doc.TotalAmountInBase, cancellationToken);
        else
            await PurchaseInvoiceSettlement.ReverseAsync(
                context, doc.TenantId, invoiceId, doc.TotalAmountInBase, cancellationToken);
    }
}

public sealed class ReverseFinancialNoteCommandHandler(
    IApplicationDbContext context,
    IJournalPostingService postingService)
    : IRequestHandler<ReverseFinancialNoteCommand, Result<FinancialNoteDto>>
{
    public async Task<Result<FinancialNoteDto>> Handle(
        ReverseFinancialNoteCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.FinancialNotes.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (doc is null)
            return Result<FinancialNoteDto>.Failure(ErrorCodes.FinancialNoteNotFound, "Financial note not found.");
        if (doc.JournalEntryId is not Guid journalId)
            return Result<FinancialNoteDto>.Failure(ErrorCodes.FinancialNoteNotPosted, "Note is not linked to a journal.");

        var reversed = await postingService.ReverseAsync(journalId, request.UserId, cancellationToken);
        if (!reversed.IsSuccess)
            return Result<FinancialNoteDto>.Failure(
                reversed.ErrorCode!, reversed.ErrorMessage ?? "Failed to reverse journal.");

        try
        {
            doc.MarkReversed();
            if (doc.ReferenceType == FinancialNoteReferenceType.PurchaseInvoice
                && doc.ReferenceDocumentId is Guid invoiceId
                && doc.PartyType == NotificationPartyType.Supplier
                && doc.NoteKind == FinancialNoteKind.Credit)
            {
                await PurchaseInvoiceSettlement.ReverseAsync(
                    context, doc.TenantId, invoiceId, doc.TotalAmountInBase, cancellationToken);
            }
            await context.SaveChangesAsync(cancellationToken);
            return Result<FinancialNoteDto>.Success(
                await CreateFinancialNoteCommandHandler.LoadDtoAsync(context, doc.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<FinancialNoteDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class CancelFinancialNoteCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CancelFinancialNoteCommand, Result<FinancialNoteDto>>
{
    public async Task<Result<FinancialNoteDto>> Handle(
        CancelFinancialNoteCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.FinancialNotes.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (doc is null)
            return Result<FinancialNoteDto>.Failure(ErrorCodes.FinancialNoteNotFound, "Financial note not found.");
        try
        {
            doc.Cancel(request.UserId);
            await context.SaveChangesAsync(cancellationToken);
            return Result<FinancialNoteDto>.Success(
                await CreateFinancialNoteCommandHandler.LoadDtoAsync(context, doc.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<FinancialNoteDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class DeleteFinancialNoteCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteFinancialNoteCommand, Result>
{
    public async Task<Result> Handle(DeleteFinancialNoteCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.FinancialNotes.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (doc is null)
            return Result.Failure(ErrorCodes.FinancialNoteNotFound, "Financial note not found.");
        try
        {
            doc.EnsureCanDelete();
            doc.SoftDelete(null);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

internal static class FinancialNoteValidation
{
    public static async Task<Result<FinancialNoteDto>?> ValidateAsync(
        IApplicationDbContext context, Guid tenantId, UpsertFinancialNoteDto dto, CancellationToken ct)
    {
        var companyOk = await context.Companies.AnyAsync(c => c.Id == dto.CompanyId && c.TenantId == tenantId, ct);
        if (!companyOk)
            return Result<FinancialNoteDto>.Failure(ErrorCodes.RequiredField, "Company is required.");

        var branchOk = await context.Branches.AnyAsync(
            b => b.Id == dto.BranchId && b.TenantId == tenantId && b.CompanyId == dto.CompanyId, ct);
        if (!branchOk)
            return Result<FinancialNoteDto>.Failure(ErrorCodes.RequiredField, "Invalid branch.");

        var periodOk = await context.FiscalPeriods.AnyAsync(p => p.Id == dto.FiscalPeriodId && p.TenantId == tenantId, ct);
        if (!periodOk)
            return Result<FinancialNoteDto>.Failure(ErrorCodes.RequiredField, "Fiscal period is required.");

        var mainOk = await context.ChartOfAccounts.AnyAsync(
            a => a.Id == dto.MainAccountId && a.TenantId == tenantId, ct);
        if (!mainOk)
            return Result<FinancialNoteDto>.Failure(ErrorCodes.FinancialNoteMainAccountRequired, "Invalid main account.");

        if (dto.Lines is { Count: > 0 })
        {
            var reasonIds = dto.Lines.Select(l => l.NotificationReasonId).Distinct().ToList();
            var noteType = dto.NoteKind == FinancialNoteKind.Debit
                ? NotificationNoteType.Debit
                : NotificationNoteType.Credit;

            var reasons = await context.NotificationReasons.AsNoTracking()
                .Where(r => reasonIds.Contains(r.Id) && r.TenantId == tenantId)
                .ToListAsync(ct);
            if (reasons.Count != reasonIds.Count)
                return Result<FinancialNoteDto>.Failure(ErrorCodes.FinancialNoteReasonInvalid, "Invalid notification reason.");

            if (reasons.Any(r => r.NoteType != noteType))
                return Result<FinancialNoteDto>.Failure(
                    ErrorCodes.FinancialNoteReasonInvalid, "Reason note type must match the financial note kind.");

            var offsetIds = dto.Lines.Select(l => l.OffsetAccountId).Distinct().ToList();
            var offsetCount = await context.ChartOfAccounts.CountAsync(
                a => offsetIds.Contains(a.Id) && a.TenantId == tenantId, ct);
            if (offsetCount != offsetIds.Count)
                return Result<FinancialNoteDto>.Failure(ErrorCodes.RequiredField, "One or more offset accounts are invalid.");
        }

        return null;
    }
}
