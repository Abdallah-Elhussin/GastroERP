using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Application.Features.Finance.Services;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Entities.Finance;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainDoc = GastroErp.Domain.Entities.Finance.ReceiptVoucher;

namespace GastroErp.Application.Features.Finance.Commands;

internal static class ReceiptVoucherMapper
{
    public static ReceiptVoucherDto ToDto(
        DomainDoc doc,
        string? companyName,
        string? branchName,
        int? fiscalYear,
        string? cashBoxName,
        string? bankName,
        string? journalNumber,
        IReadOnlyDictionary<Guid, (string Number, string NameAr)> accounts,
        IReadOnlyDictionary<Guid, string> costCenters)
    {
        var lines = doc.Lines
            .OrderBy(l => l.LineNumber)
            .Select(l =>
            {
                string? accNo = null, accName = null;
                if (accounts.TryGetValue(l.ChartOfAccountId, out var acc))
                {
                    accNo = acc.Number;
                    accName = acc.NameAr;
                }

                string? ccName = null;
                if (l.CostCenterId is Guid ccId)
                    costCenters.TryGetValue(ccId, out ccName);

                string? analyticalName = null;
                if (l.AnalyticalAccountId is Guid anId && accounts.TryGetValue(anId, out var an))
                    analyticalName = an.NameAr;

                return new ReceiptVoucherLineDto(
                    l.Id, l.ChartOfAccountId, accNo, accName,
                    l.CostCenterId, ccName, l.AnalyticalAccountId, analyticalName,
                    l.Currency, l.ExchangeRate, l.Amount, l.AmountInBase, l.Description);
            })
            .ToList();

        return new ReceiptVoucherDto(
            doc.Id, doc.Number, doc.DocumentNumber, doc.CompanyId, companyName,
            doc.BranchId, branchName, doc.VoucherDate, doc.FiscalPeriodId, fiscalYear,
            doc.ReceiptMethod, doc.CashBoxId, cashBoxName, doc.BankId, bankName,
            doc.PartyType, doc.PartyId, doc.PartyName, doc.Currency, doc.ExchangeRate,
            doc.CostCenterId, null, doc.Reference, doc.ChequeNumber, doc.ChequeDate,
            doc.Description, doc.Notes, doc.Status, doc.JournalEntryId, journalNumber,
            lines.Count, doc.TotalAmount, doc.TotalAmountInBase, doc.CreatedAt, doc.PostedAt,
            doc.ApprovedAt, lines);
    }

    public static void ApplyLines(DomainDoc doc, IReadOnlyList<ReceiptVoucherLineDto>? lines)
    {
        if (lines is null || lines.Count == 0)
        {
            doc.ReplaceLines([]);
            return;
        }

        doc.ReplaceLines(lines.Select(l => (
            l.ChartOfAccountId,
            l.Amount,
            l.CostCenterId,
            l.Description,
            (string?)l.Currency,
            (decimal?)l.ExchangeRate,
            l.AnalyticalAccountId)));
    }
}

public sealed class CreateReceiptVoucherCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateReceiptVoucherCommand, Result<ReceiptVoucherDto>>
{
    public async Task<Result<ReceiptVoucherDto>> Handle(
        CreateReceiptVoucherCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var validation = await ReceiptVoucherValidation.ValidateRefsAsync(context, request.TenantId, dto, cancellationToken);
        if (validation is not null) return validation;

        var period = await context.FiscalPeriods.AsNoTracking()
            .FirstAsync(p => p.Id == dto.FiscalPeriodId, cancellationToken);

        try
        {
            var next = await context.ReceiptVouchers
                .Where(d => d.TenantId == request.TenantId)
                .Select(d => (int?)d.Number).MaxAsync(cancellationToken) ?? 0;
            var number = next + 1;
            var docNumber = $"RV-{period.FiscalYear}-{number:D4}";

            var doc = DomainDoc.Create(
                request.TenantId, number, docNumber, dto.CompanyId, dto.BranchId,
                dto.VoucherDate, dto.FiscalPeriodId, dto.ReceiptMethod, dto.PartyType,
                dto.CashBoxId, dto.BankId, dto.PartyId, dto.PartyName, dto.Currency,
                dto.ExchangeRate, dto.CostCenterId, dto.Reference, dto.Description,
                dto.Notes, dto.ChequeNumber, dto.ChequeDate);

            ReceiptVoucherMapper.ApplyLines(doc, dto.Lines);
            context.ReceiptVouchers.Add(doc);
            await context.SaveChangesAsync(cancellationToken);
            return Result<ReceiptVoucherDto>.Success(
                await LoadDtoAsync(context, doc.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<ReceiptVoucherDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }

    internal static async Task<ReceiptVoucherDto> LoadDtoAsync(
        IApplicationDbContext context, Guid id, CancellationToken ct)
    {
        var doc = await context.ReceiptVouchers.AsNoTracking()
            .Include(d => d.Lines)
            .FirstAsync(d => d.Id == id, ct);
        return await EnrichAsync(context, doc, ct);
    }

    internal static async Task<ReceiptVoucherDto> EnrichAsync(
        IApplicationDbContext context, DomainDoc doc, CancellationToken ct)
    {
        var company = await context.Companies.AsNoTracking()
            .Where(c => c.Id == doc.CompanyId).Select(c => c.NameAr).FirstOrDefaultAsync(ct);
        var branch = await context.Branches.AsNoTracking()
            .Where(b => b.Id == doc.BranchId).Select(b => b.NameAr).FirstOrDefaultAsync(ct);
        var fiscalYear = await context.FiscalPeriods.AsNoTracking()
            .Where(p => p.Id == doc.FiscalPeriodId).Select(p => (int?)p.FiscalYear).FirstOrDefaultAsync(ct);
        var cashBox = doc.CashBoxId is Guid cb
            ? await context.CashBoxes.AsNoTracking().Where(c => c.Id == cb).Select(c => c.NameAr).FirstOrDefaultAsync(ct)
            : null;
        var bank = doc.BankId is Guid bk
            ? await context.Banks.AsNoTracking().Where(b => b.Id == bk).Select(b => b.NameAr).FirstOrDefaultAsync(ct)
            : null;
        var journalNumber = doc.JournalEntryId is Guid jid
            ? await context.JournalEntries.AsNoTracking().Where(j => j.Id == jid).Select(j => j.EntryNumber).FirstOrDefaultAsync(ct)
            : null;

        var accountIds = doc.Lines.Select(l => l.ChartOfAccountId)
            .Concat(doc.Lines.Where(l => l.AnalyticalAccountId.HasValue).Select(l => l.AnalyticalAccountId!.Value))
            .Distinct().ToList();
        var accounts = await context.ChartOfAccounts.AsNoTracking()
            .Where(a => accountIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => (a.AccountNumber, a.NameAr), ct);

        var ccIds = doc.Lines.Where(l => l.CostCenterId.HasValue).Select(l => l.CostCenterId!.Value).Distinct().ToList();
        var costCenters = await context.CostCenters.AsNoTracking()
            .Where(c => ccIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.NameAr, ct);

        return ReceiptVoucherMapper.ToDto(
            doc, company, branch, fiscalYear, cashBox, bank, journalNumber, accounts, costCenters);
    }
}

public sealed class UpdateReceiptVoucherCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateReceiptVoucherCommand, Result<ReceiptVoucherDto>>
{
    public async Task<Result<ReceiptVoucherDto>> Handle(
        UpdateReceiptVoucherCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.ReceiptVouchers
            .Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (doc is null)
            return Result<ReceiptVoucherDto>.Failure(ErrorCodes.ReceiptVoucherNotFound, "Receipt voucher not found.");

        var validation = await ReceiptVoucherValidation.ValidateRefsAsync(
            context, doc.TenantId, request.Dto, cancellationToken);
        if (validation is not null) return validation;

        try
        {
            var dto = request.Dto;
            doc.Update(
                dto.VoucherDate, dto.FiscalPeriodId, dto.ReceiptMethod, dto.PartyType,
                dto.CashBoxId, dto.BankId, dto.PartyId, dto.PartyName, dto.Currency,
                dto.ExchangeRate, dto.CostCenterId, dto.Reference, dto.Description,
                dto.Notes, dto.ChequeNumber, dto.ChequeDate);
            ReceiptVoucherMapper.ApplyLines(doc, dto.Lines);
            context.ReceiptVouchers.Update(doc);
            await context.SaveChangesAsync(cancellationToken);
            return Result<ReceiptVoucherDto>.Success(
                await CreateReceiptVoucherCommandHandler.LoadDtoAsync(context, doc.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<ReceiptVoucherDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class SubmitReceiptVoucherCommandHandler(IApplicationDbContext context)
    : IRequestHandler<SubmitReceiptVoucherCommand, Result<ReceiptVoucherDto>>
{
    public async Task<Result<ReceiptVoucherDto>> Handle(
        SubmitReceiptVoucherCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.ReceiptVouchers.Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (doc is null)
            return Result<ReceiptVoucherDto>.Failure(ErrorCodes.ReceiptVoucherNotFound, "Receipt voucher not found.");
        try
        {
            doc.Submit();
            await context.SaveChangesAsync(cancellationToken);
            return Result<ReceiptVoucherDto>.Success(
                await CreateReceiptVoucherCommandHandler.LoadDtoAsync(context, doc.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<ReceiptVoucherDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class ApproveReceiptVoucherCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ApproveReceiptVoucherCommand, Result<ReceiptVoucherDto>>
{
    public async Task<Result<ReceiptVoucherDto>> Handle(
        ApproveReceiptVoucherCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.ReceiptVouchers.Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (doc is null)
            return Result<ReceiptVoucherDto>.Failure(ErrorCodes.ReceiptVoucherNotFound, "Receipt voucher not found.");
        try
        {
            doc.Approve(request.UserId);
            await context.SaveChangesAsync(cancellationToken);
            return Result<ReceiptVoucherDto>.Success(
                await CreateReceiptVoucherCommandHandler.LoadDtoAsync(context, doc.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<ReceiptVoucherDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class PostReceiptVoucherCommandHandler(
    IApplicationDbContext context,
    IJournalPostingService postingService)
    : IRequestHandler<PostReceiptVoucherCommand, Result<ReceiptVoucherDto>>
{
    public async Task<Result<ReceiptVoucherDto>> Handle(
        PostReceiptVoucherCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.ReceiptVouchers
            .Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (doc is null)
            return Result<ReceiptVoucherDto>.Failure(ErrorCodes.ReceiptVoucherNotFound, "Receipt voucher not found.");

        if (doc.Status is not (ReceiptVoucherStatus.Approved or ReceiptVoucherStatus.Draft))
            return Result<ReceiptVoucherDto>.Failure(
                ErrorCodes.ReceiptVoucherNotEditable, "Only approved or draft vouchers can be posted.");

        var period = await context.FiscalPeriods.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == doc.FiscalPeriodId, cancellationToken);
        if (period is null || period.Status != FiscalPeriodStatus.Open)
            return Result<ReceiptVoucherDto>.Failure(
                ErrorCodes.FiscalPeriodClosed, "Fiscal period must be open to post.");

        Guid destinationAccountId;
        if (doc.CashBoxId is Guid cashBoxId)
        {
            var cashBox = await context.CashBoxes.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == cashBoxId && c.TenantId == doc.TenantId, cancellationToken);
            if (cashBox is null)
                return Result<ReceiptVoucherDto>.Failure(ErrorCodes.ReceiptVoucherCashBoxRequired, "Cash box not found.");
            destinationAccountId = cashBox.ChartOfAccountId;
        }
        else if (doc.BankId is Guid bankId)
        {
            var bank = await context.Banks.AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == bankId && b.TenantId == doc.TenantId, cancellationToken);
            if (bank is null)
                return Result<ReceiptVoucherDto>.Failure(ErrorCodes.ReceiptVoucherBankRequired, "Bank not found.");
            destinationAccountId = bank.ChartOfAccountId;
        }
        else
        {
            return Result<ReceiptVoucherDto>.Failure(
                ErrorCodes.ReceiptVoucherDestinationInvalid, "Cash box or bank is required.");
        }

        var journalLines = new List<JournalLineDto>
        {
            new(null, destinationAccountId, doc.CostCenterId, doc.TotalAmountInBase, 0,
                doc.Description ?? $"Receipt {doc.DocumentNumber}")
        };

        journalLines.AddRange(doc.Lines.Select(l =>
            new JournalLineDto(null, l.ChartOfAccountId, l.CostCenterId, 0, l.AmountInBase, l.Description)));

        var journalDto = new CreateJournalDto(
            doc.VoucherDate,
            doc.Description ?? $"Receipt voucher {doc.DocumentNumber}",
            PostingSource.Receipt,
            doc.BranchId,
            doc.CompanyId,
            doc.Reference ?? doc.DocumentNumber,
            doc.Id,
            journalLines);

        var posted = await postingService.CreateAndPostAsync(
            doc.TenantId, request.UserId, journalDto, cancellationToken);
        if (!posted.IsSuccess)
            return Result<ReceiptVoucherDto>.Failure(
                posted.ErrorCode!, posted.ErrorMessage ?? "Failed to post receipt journal.");

        try
        {
            doc.MarkPosted(posted.Data!.Id, request.UserId);
            context.AccountingTransactions.Add(AccountingTransaction.Create(
                doc.TenantId, PostingSource.Receipt, doc.Id,
                posted.Data.Id, doc.DocumentNumber));
            await context.SaveChangesAsync(cancellationToken);
            return Result<ReceiptVoucherDto>.Success(
                await CreateReceiptVoucherCommandHandler.LoadDtoAsync(context, doc.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<ReceiptVoucherDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class ReverseReceiptVoucherCommandHandler(
    IApplicationDbContext context,
    IJournalPostingService postingService)
    : IRequestHandler<ReverseReceiptVoucherCommand, Result<ReceiptVoucherDto>>
{
    public async Task<Result<ReceiptVoucherDto>> Handle(
        ReverseReceiptVoucherCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.ReceiptVouchers
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (doc is null)
            return Result<ReceiptVoucherDto>.Failure(ErrorCodes.ReceiptVoucherNotFound, "Receipt voucher not found.");
        if (doc.JournalEntryId is not Guid journalId)
            return Result<ReceiptVoucherDto>.Failure(
                ErrorCodes.ReceiptVoucherNotPosted, "Receipt voucher is not linked to a journal.");

        var reversed = await postingService.ReverseAsync(journalId, request.UserId, cancellationToken);
        if (!reversed.IsSuccess)
            return Result<ReceiptVoucherDto>.Failure(
                reversed.ErrorCode!, reversed.ErrorMessage ?? "Failed to reverse journal.");

        try
        {
            doc.MarkReversed();
            await context.SaveChangesAsync(cancellationToken);
            return Result<ReceiptVoucherDto>.Success(
                await CreateReceiptVoucherCommandHandler.LoadDtoAsync(context, doc.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<ReceiptVoucherDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class CancelReceiptVoucherCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CancelReceiptVoucherCommand, Result<ReceiptVoucherDto>>
{
    public async Task<Result<ReceiptVoucherDto>> Handle(
        CancelReceiptVoucherCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.ReceiptVouchers
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (doc is null)
            return Result<ReceiptVoucherDto>.Failure(ErrorCodes.ReceiptVoucherNotFound, "Receipt voucher not found.");
        try
        {
            doc.Cancel(request.UserId);
            await context.SaveChangesAsync(cancellationToken);
            return Result<ReceiptVoucherDto>.Success(
                await CreateReceiptVoucherCommandHandler.LoadDtoAsync(context, doc.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<ReceiptVoucherDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class DeleteReceiptVoucherCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteReceiptVoucherCommand, Result>
{
    public async Task<Result> Handle(DeleteReceiptVoucherCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.ReceiptVouchers
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (doc is null)
            return Result.Failure(ErrorCodes.ReceiptVoucherNotFound, "Receipt voucher not found.");
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

internal static class ReceiptVoucherValidation
{
    public static async Task<Result<ReceiptVoucherDto>?> ValidateRefsAsync(
        IApplicationDbContext context,
        Guid tenantId,
        UpsertReceiptVoucherDto dto,
        CancellationToken ct)
    {
        var companyOk = await context.Companies.AnyAsync(
            c => c.Id == dto.CompanyId && c.TenantId == tenantId, ct);
        if (!companyOk)
            return Result<ReceiptVoucherDto>.Failure(ErrorCodes.RequiredField, "Company is required.");

        var branchOk = await context.Branches.AnyAsync(
            b => b.Id == dto.BranchId && b.TenantId == tenantId && b.CompanyId == dto.CompanyId, ct);
        if (!branchOk)
            return Result<ReceiptVoucherDto>.Failure(ErrorCodes.RequiredField, "Invalid branch.");

        var periodOk = await context.FiscalPeriods.AnyAsync(
            p => p.Id == dto.FiscalPeriodId && p.TenantId == tenantId, ct);
        if (!periodOk)
            return Result<ReceiptVoucherDto>.Failure(ErrorCodes.RequiredField, "Fiscal period is required.");

        if (dto.CashBoxId is Guid cashId)
        {
            var ok = await context.CashBoxes.AnyAsync(
                c => c.Id == cashId && c.TenantId == tenantId, ct);
            if (!ok)
                return Result<ReceiptVoucherDto>.Failure(ErrorCodes.ReceiptVoucherCashBoxRequired, "Invalid cash box.");
        }

        if (dto.BankId is Guid bankId)
        {
            var ok = await context.Banks.AnyAsync(
                b => b.Id == bankId && b.TenantId == tenantId, ct);
            if (!ok)
                return Result<ReceiptVoucherDto>.Failure(ErrorCodes.ReceiptVoucherBankRequired, "Invalid bank.");
        }

        if (dto.Lines is { Count: > 0 })
        {
            var ids = dto.Lines.Select(l => l.ChartOfAccountId).Distinct().ToList();
            var count = await context.ChartOfAccounts.CountAsync(
                a => ids.Contains(a.Id) && a.TenantId == tenantId, ct);
            if (count != ids.Count)
                return Result<ReceiptVoucherDto>.Failure(ErrorCodes.RequiredField, "One or more accounts are invalid.");
        }

        return null;
    }
}
