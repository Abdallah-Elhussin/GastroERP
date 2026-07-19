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
using DomainDoc = GastroErp.Domain.Entities.Finance.FinancialOpeningBalance;

namespace GastroErp.Application.Features.Finance.Commands;

internal static class FinancialOpeningBalanceMapper
{
    public static FinancialOpeningBalanceDto ToDto(
        DomainDoc doc,
        string? companyName,
        string? branchName,
        int? fiscalYear,
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
                return new FinancialOpeningBalanceLineDto(
                    l.Id, l.ChartOfAccountId, accNo, accName,
                    l.CostCenterId, ccName, l.Debit, l.Credit, l.Currency, l.Description);
            })
            .ToList();

        return new FinancialOpeningBalanceDto(
            doc.Id, doc.Number, doc.DocumentNumber, doc.CompanyId, companyName,
            doc.BranchId, branchName, doc.OpeningDate, doc.FiscalPeriodId, fiscalYear,
            doc.Description, doc.Status, doc.EquityAccountId, doc.JournalEntryId, journalNumber,
            lines.Count, doc.TotalDebit, doc.TotalCredit, doc.CreatedAt, doc.PostedAt, lines);
    }
}

public sealed class CreateFinancialOpeningBalanceCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateFinancialOpeningBalanceCommand, Result<FinancialOpeningBalanceDto>>
{
    public async Task<Result<FinancialOpeningBalanceDto>> Handle(
        CreateFinancialOpeningBalanceCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var companyOk = await context.Companies.AnyAsync(
            c => c.Id == dto.CompanyId && c.TenantId == request.TenantId, cancellationToken);
        if (!companyOk)
            return Result<FinancialOpeningBalanceDto>.Failure(ErrorCodes.RequiredField, "Company is required.");

        var period = await context.FiscalPeriods.FirstOrDefaultAsync(
            p => p.Id == dto.FiscalPeriodId && p.TenantId == request.TenantId, cancellationToken);
        if (period is null)
            return Result<FinancialOpeningBalanceDto>.Failure(ErrorCodes.RequiredField, "Fiscal period is required.");

        if (dto.BranchId is Guid branchId)
        {
            var branchOk = await context.Branches.AnyAsync(
                b => b.Id == branchId && b.TenantId == request.TenantId && b.CompanyId == dto.CompanyId,
                cancellationToken);
            if (!branchOk)
                return Result<FinancialOpeningBalanceDto>.Failure(ErrorCodes.RequiredField, "Invalid branch.");
        }

        try
        {
            var next = await context.FinancialOpeningBalances
                .Where(d => d.TenantId == request.TenantId)
                .Select(d => (int?)d.Number).MaxAsync(cancellationToken) ?? 0;
            var number = next + 1;
            var docNumber = $"OB-{period.FiscalYear}-{number:D4}";

            var doc = DomainDoc.Create(
                request.TenantId, number, docNumber, dto.CompanyId, dto.OpeningDate,
                dto.FiscalPeriodId, dto.BranchId, dto.Description, dto.EquityAccountId);

            ApplyLines(doc, dto.Lines);
            context.FinancialOpeningBalances.Add(doc);
            await context.SaveChangesAsync(cancellationToken);
            return Result<FinancialOpeningBalanceDto>.Success(
                await LoadDtoAsync(context, doc.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<FinancialOpeningBalanceDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }

    internal static void ApplyLines(DomainDoc doc, IReadOnlyList<FinancialOpeningBalanceLineDto>? lines)
    {
        var list = lines ?? Array.Empty<FinancialOpeningBalanceLineDto>();
        doc.ReplaceLines(list.Select(l => (
            l.ChartOfAccountId,
            l.Debit,
            l.Credit,
            string.IsNullOrWhiteSpace(l.Currency) ? "SAR" : l.Currency,
            l.CostCenterId,
            l.Description)));
    }

    internal static async Task<FinancialOpeningBalanceDto> LoadDtoAsync(
        IApplicationDbContext context, Guid id, CancellationToken ct)
    {
        var doc = await context.FinancialOpeningBalances.AsNoTracking()
            .Include(d => d.Lines)
            .FirstAsync(d => d.Id == id, ct);
        return await EnrichAsync(context, doc, ct);
    }

    internal static async Task<FinancialOpeningBalanceDto> EnrichAsync(
        IApplicationDbContext context, DomainDoc doc, CancellationToken ct)
    {
        var company = await context.Companies.AsNoTracking()
            .Where(c => c.Id == doc.CompanyId).Select(c => c.NameAr).FirstOrDefaultAsync(ct);
        string? branch = null;
        if (doc.BranchId is Guid bid)
            branch = await context.Branches.AsNoTracking()
                .Where(b => b.Id == bid).Select(b => b.NameAr).FirstOrDefaultAsync(ct);

        var fiscalYear = await context.FiscalPeriods.AsNoTracking()
            .Where(p => p.Id == doc.FiscalPeriodId).Select(p => (int?)p.FiscalYear).FirstOrDefaultAsync(ct);

        string? journalNumber = null;
        if (doc.JournalEntryId is Guid jid)
            journalNumber = await context.JournalEntries.AsNoTracking()
                .Where(j => j.Id == jid).Select(j => j.EntryNumber).FirstOrDefaultAsync(ct);

        var accountIds = doc.Lines.Select(l => l.ChartOfAccountId).Distinct().ToList();
        var accounts = await context.ChartOfAccounts.AsNoTracking()
            .Where(a => accountIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => (a.AccountNumber, a.NameAr), ct);

        var ccIds = doc.Lines.Where(l => l.CostCenterId.HasValue).Select(l => l.CostCenterId!.Value).Distinct().ToList();
        var costCenters = ccIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await context.CostCenters.AsNoTracking()
                .Where(c => ccIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.NameAr, ct);

        return FinancialOpeningBalanceMapper.ToDto(
            doc, company, branch, fiscalYear, journalNumber, accounts, costCenters);
    }
}

public sealed class UpdateFinancialOpeningBalanceCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateFinancialOpeningBalanceCommand, Result<FinancialOpeningBalanceDto>>
{
    public async Task<Result<FinancialOpeningBalanceDto>> Handle(
        UpdateFinancialOpeningBalanceCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.FinancialOpeningBalances
            .Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (doc is null)
            return Result<FinancialOpeningBalanceDto>.Failure(
                ErrorCodes.OpeningBalanceNotFound, "Opening balance not found.");

        var dto = request.Dto;
        if (dto.CompanyId != doc.CompanyId)
            return Result<FinancialOpeningBalanceDto>.Failure(
                ErrorCodes.RequiredField, "Company cannot be changed.");

        try
        {
            doc.Update(dto.OpeningDate, dto.FiscalPeriodId, dto.BranchId, dto.Description, dto.EquityAccountId);
            var existing = doc.Lines.ToList();
            if (existing.Count > 0)
                context.FinancialOpeningBalanceLines.RemoveRange(existing);
            CreateFinancialOpeningBalanceCommandHandler.ApplyLines(doc, dto.Lines);
            await context.SaveChangesAsync(cancellationToken);
            return Result<FinancialOpeningBalanceDto>.Success(
                await CreateFinancialOpeningBalanceCommandHandler.LoadDtoAsync(context, doc.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<FinancialOpeningBalanceDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class PostFinancialOpeningBalanceCommandHandler(
    IApplicationDbContext context,
    IJournalPostingService postingService)
    : IRequestHandler<PostFinancialOpeningBalanceCommand, Result<FinancialOpeningBalanceDto>>
{
    public async Task<Result<FinancialOpeningBalanceDto>> Handle(
        PostFinancialOpeningBalanceCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.FinancialOpeningBalances
            .Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (doc is null)
            return Result<FinancialOpeningBalanceDto>.Failure(
                ErrorCodes.OpeningBalanceNotFound, "Opening balance not found.");

        if (doc.Status != FinancialOpeningBalanceStatus.Draft)
            return Result<FinancialOpeningBalanceDto>.Failure(
                ErrorCodes.OpeningBalanceNotEditable, "Only draft opening balances can be posted.");

        if (!doc.Lines.Any())
            return Result<FinancialOpeningBalanceDto>.Failure(
                ErrorCodes.JournalHasNoLines, "Opening balance must have lines.");

        var alreadyPosted = await context.FinancialOpeningBalances.AnyAsync(
            d => d.TenantId == doc.TenantId
                 && d.CompanyId == doc.CompanyId
                 && d.FiscalPeriodId == doc.FiscalPeriodId
                 && d.Status == FinancialOpeningBalanceStatus.Posted
                 && d.Id != doc.Id, cancellationToken);
        if (alreadyPosted)
            return Result<FinancialOpeningBalanceDto>.Failure(
                ErrorCodes.OpeningBalanceDuplicate,
                "A posted opening balance already exists for this company and fiscal period.");

        var equityAccountId = doc.EquityAccountId;
        if (equityAccountId is null)
        {
            equityAccountId = await context.AccountingSettings.AsNoTracking()
                .Where(s => s.TenantId == doc.TenantId)
                .Select(s => s.OpeningBalanceAccountId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var journalLines = doc.Lines
            .Select(l => new JournalLineDto(null, l.ChartOfAccountId, l.CostCenterId, l.Debit, l.Credit, l.Description))
            .ToList();

        var net = doc.NetDifference;
        if (net != 0)
        {
            if (equityAccountId is null || equityAccountId == Guid.Empty)
                return Result<FinancialOpeningBalanceDto>.Failure(
                    ErrorCodes.OpeningBalanceEquityRequired,
                    "Equity/opening balance account is required to balance the entry.");

            if (net > 0)
                journalLines.Add(new JournalLineDto(null, equityAccountId.Value, null, 0, net, "Opening balance equity"));
            else
                journalLines.Add(new JournalLineDto(null, equityAccountId.Value, null, Math.Abs(net), 0, "Opening balance equity"));
        }

        var journalDto = new CreateJournalDto(
            doc.OpeningDate,
            doc.Description ?? $"Opening balance {doc.DocumentNumber}",
            PostingSource.OpeningBalance,
            doc.BranchId,
            doc.CompanyId,
            doc.DocumentNumber,
            doc.Id,
            journalLines);

        var posted = await postingService.CreateAndPostAsync(
            doc.TenantId, request.UserId, journalDto, cancellationToken);
        if (!posted.IsSuccess)
            return Result<FinancialOpeningBalanceDto>.Failure(
                posted.ErrorCode!, posted.ErrorMessage ?? "Failed to post opening journal.");

        try
        {
            doc.MarkPosted(posted.Data!.Id, request.UserId);
            context.AccountingTransactions.Add(AccountingTransaction.Create(
                doc.TenantId, PostingSource.OpeningBalance, doc.Id,
                posted.Data.Id, doc.DocumentNumber));
            await context.SaveChangesAsync(cancellationToken);
            return Result<FinancialOpeningBalanceDto>.Success(
                await CreateFinancialOpeningBalanceCommandHandler.LoadDtoAsync(context, doc.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<FinancialOpeningBalanceDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class ReverseFinancialOpeningBalanceCommandHandler(
    IApplicationDbContext context,
    IJournalPostingService postingService)
    : IRequestHandler<ReverseFinancialOpeningBalanceCommand, Result<FinancialOpeningBalanceDto>>
{
    public async Task<Result<FinancialOpeningBalanceDto>> Handle(
        ReverseFinancialOpeningBalanceCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.FinancialOpeningBalances
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (doc is null)
            return Result<FinancialOpeningBalanceDto>.Failure(
                ErrorCodes.OpeningBalanceNotFound, "Opening balance not found.");
        if (doc.JournalEntryId is not Guid journalId)
            return Result<FinancialOpeningBalanceDto>.Failure(
                ErrorCodes.OpeningBalanceNotPosted, "Opening balance is not linked to a journal.");

        var reversed = await postingService.ReverseAsync(journalId, request.UserId, cancellationToken);
        if (!reversed.IsSuccess)
            return Result<FinancialOpeningBalanceDto>.Failure(
                reversed.ErrorCode!, reversed.ErrorMessage ?? "Failed to reverse journal.");

        try
        {
            doc.MarkReversed();
            await context.SaveChangesAsync(cancellationToken);
            return Result<FinancialOpeningBalanceDto>.Success(
                await CreateFinancialOpeningBalanceCommandHandler.LoadDtoAsync(context, doc.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<FinancialOpeningBalanceDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class DeleteFinancialOpeningBalanceCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteFinancialOpeningBalanceCommand, Result>
{
    public async Task<Result> Handle(
        DeleteFinancialOpeningBalanceCommand request, CancellationToken cancellationToken)
    {
        var doc = await context.FinancialOpeningBalances
            .Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (doc is null)
            return Result.Failure(ErrorCodes.OpeningBalanceNotFound, "Opening balance not found.");

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
