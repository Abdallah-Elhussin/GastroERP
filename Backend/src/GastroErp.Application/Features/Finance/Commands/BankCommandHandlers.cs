using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Entities.Finance;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainBank = GastroErp.Domain.Entities.Finance.Bank;

namespace GastroErp.Application.Features.Finance.Commands;

internal static class BankMapper
{
    public static BankDto ToDto(
        DomainBank bank,
        string? companyName,
        string? branchName,
        string? accountNumber,
        string? accountName,
        string? currencyCode,
        IReadOnlyDictionary<Guid, string>? currencyCodes = null)
    {
        var accounts = bank.Accounts
            .OrderBy(a => a.SortOrder)
            .Select(a => new BankAccountDetailDto(
                a.Id, a.CurrencyId, currencyCodes?.GetValueOrDefault(a.CurrencyId),
                a.AccountNumber, a.Iban, a.MinBalance, a.MaxBalance, a.MinTransaction, a.MaxTransaction,
                a.DailyTransferLimit, a.AllowExceedLimits, a.AllowWithdraw, a.AllowDeposit, a.AllowTransfer,
                a.IsDefault, a.IsActive, a.SortOrder))
            .ToList();

        return new BankDto(
            bank.Id, bank.Number, bank.NameAr, bank.NameEn, bank.Code, bank.SwiftCode, bank.DefaultIban,
            bank.CompanyId, companyName, bank.BranchId, branchName,
            bank.ChartOfAccountId, accountNumber, accountName,
            bank.BaseCurrencyId, currencyCode,
            bank.IsActive, bank.DeactivatedAt, bank.DeactivationReason, bank.IsSystem, bank.SortOrder,
            accounts.Count, accounts);
    }

    public static void ApplyAccounts(DomainBank bank, IReadOnlyList<BankAccountDetailDto>? accounts)
    {
        if (accounts is null) return;
        var list = accounts.Select((a, i) => BankAccountDetail.Create(
            bank.Id, a.CurrencyId, a.AccountNumber, a.Iban,
            a.MinBalance, a.MaxBalance, a.MinTransaction, a.MaxTransaction, a.DailyTransferLimit,
            a.AllowExceedLimits, a.AllowWithdraw, a.AllowDeposit, a.AllowTransfer,
            a.IsDefault, a.IsActive, a.SortOrder == 0 ? i : a.SortOrder)).ToList();
        bank.ReplaceAccounts(list);
    }
}

internal static class BankGlGuard
{
    public static async Task<Result?> ValidateGlAsync(
        IApplicationDbContext context, Guid tenantId, Guid chartOfAccountId, Guid? excludeBankId, CancellationToken ct)
    {
        var account = await context.ChartOfAccounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == chartOfAccountId && a.TenantId == tenantId, ct);
        if (account is null || !account.IsActive || account.IsSummaryAccount || !account.IsPostingAllowed
            || account.AccountType != AccountType.Asset)
            return Result.Failure(ErrorCodes.BankGlAccountInvalid,
                "GL account must be an active posting asset account.");

        // Prefer classification "bank" when present; otherwise allow any posting asset.
        if (account.AccountClassificationId is Guid classId)
        {
            var code = await context.AccountClassifications.AsNoTracking()
                .Where(c => c.Id == classId)
                .Select(c => c.Code)
                .FirstOrDefaultAsync(ct);
            if (!string.IsNullOrWhiteSpace(code)
                && !string.Equals(code, "bank", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(code, "cash", StringComparison.OrdinalIgnoreCase))
                return Result.Failure(ErrorCodes.BankGlAccountInvalid,
                    "GL account classification should be Bank or Cash.");
        }

        var taken = await context.Banks.AnyAsync(
            b => b.TenantId == tenantId && b.ChartOfAccountId == chartOfAccountId
                 && (excludeBankId == null || b.Id != excludeBankId), ct);
        if (taken)
            return Result.Failure(ErrorCodes.BankGlAccountDuplicate,
                "This GL account is already linked to another bank.");

        return null;
    }
}

public sealed class CreateBankCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateBankCommand, Result<BankDto>>
{
    public async Task<Result<BankDto>> Handle(CreateBankCommand request, CancellationToken cancellationToken)
    {
        var nameAr = request.Dto.NameAr.Trim();
        if (await context.Banks.AnyAsync(b => b.TenantId == request.TenantId && b.NameAr == nameAr, cancellationToken))
            return Result<BankDto>.Failure(ErrorCodes.BankNameDuplicate, "Bank name already exists.");

        var companyOk = await context.Companies.AnyAsync(
            c => c.Id == request.Dto.CompanyId && c.TenantId == request.TenantId, cancellationToken);
        var branchOk = await context.Branches.AnyAsync(
            b => b.Id == request.Dto.BranchId && b.TenantId == request.TenantId
                 && b.CompanyId == request.Dto.CompanyId, cancellationToken);
        if (!companyOk || !branchOk)
            return Result<BankDto>.Failure(ErrorCodes.RequiredField, "Company and branch are required.");

        if (!await context.Currencies.AnyAsync(
                c => c.Id == request.Dto.BaseCurrencyId && c.TenantId == request.TenantId, cancellationToken))
            return Result<BankDto>.Failure(ErrorCodes.CurrencyNotFound, "Base currency not found.");

        var glError = await BankGlGuard.ValidateGlAsync(
            context, request.TenantId, request.Dto.ChartOfAccountId, null, cancellationToken);
        if (glError is not null)
            return Result<BankDto>.Failure(glError.ErrorCode!, glError.ErrorMessage ?? "Invalid GL account.");

        try
        {
            var next = await context.Banks.Where(b => b.TenantId == request.TenantId)
                .Select(b => (int?)b.Number).MaxAsync(cancellationToken) ?? 0;

            var bank = DomainBank.Create(
                request.TenantId, next + 1, nameAr, request.Dto.CompanyId, request.Dto.BranchId,
                request.Dto.ChartOfAccountId, request.Dto.BaseCurrencyId,
                request.Dto.NameEn, request.Dto.Code, request.Dto.SwiftCode, request.Dto.DefaultIban,
                request.Dto.SortOrder == 0 ? next + 1 : request.Dto.SortOrder);

            if (!request.Dto.IsActive)
                bank.Deactivate(request.Dto.DeactivatedAt, request.Dto.DeactivationReason);

            BankMapper.ApplyAccounts(bank, request.Dto.Accounts);
            context.Banks.Add(bank);
            await context.SaveChangesAsync(cancellationToken);
            return Result<BankDto>.Success(await LoadDtoAsync(context, bank.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<BankDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }

    internal static async Task<BankDto> LoadDtoAsync(IApplicationDbContext context, Guid id, CancellationToken ct)
    {
        var bank = await context.Banks.AsNoTracking().Include(b => b.Accounts)
            .FirstAsync(b => b.Id == id, ct);
        return await EnrichAsync(context, bank, ct);
    }

    internal static async Task<BankDto> EnrichAsync(IApplicationDbContext context, DomainBank bank, CancellationToken ct)
    {
        var company = await context.Companies.AsNoTracking()
            .Where(c => c.Id == bank.CompanyId).Select(c => c.NameAr).FirstOrDefaultAsync(ct);
        var branch = await context.Branches.AsNoTracking()
            .Where(b => b.Id == bank.BranchId).Select(b => b.NameAr).FirstOrDefaultAsync(ct);
        var account = await context.ChartOfAccounts.AsNoTracking()
            .Where(a => a.Id == bank.ChartOfAccountId)
            .Select(a => new { a.AccountNumber, a.NameAr }).FirstOrDefaultAsync(ct);
        var currencyIds = bank.Accounts.Select(a => a.CurrencyId).Append(bank.BaseCurrencyId).Distinct().ToList();
        var codes = await context.Currencies.AsNoTracking()
            .Where(c => currencyIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Code, ct);

        return BankMapper.ToDto(bank, company, branch, account?.AccountNumber, account?.NameAr,
            codes.GetValueOrDefault(bank.BaseCurrencyId), codes);
    }
}

public sealed class UpdateBankCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateBankCommand, Result<BankDto>>
{
    public async Task<Result<BankDto>> Handle(UpdateBankCommand request, CancellationToken cancellationToken)
    {
        var bank = await context.Banks.Include(b => b.Accounts)
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
        if (bank is null)
            return Result<BankDto>.Failure(ErrorCodes.BankNotFound, "Bank not found.");

        var nameAr = request.Dto.NameAr.Trim();
        if (await context.Banks.AnyAsync(
                b => b.TenantId == bank.TenantId && b.NameAr == nameAr && b.Id != bank.Id, cancellationToken))
            return Result<BankDto>.Failure(ErrorCodes.BankNameDuplicate, "Bank name already exists.");

        var inUse = await BankUsageGuard.IsInUseAsync(context, bank, cancellationToken);
        if (inUse && request.Dto.ChartOfAccountId != bank.ChartOfAccountId)
            return Result<BankDto>.Failure(ErrorCodes.BankGlAccountLocked,
                "Cannot change GL account after the bank has been used.");

        if (request.Dto.ChartOfAccountId != bank.ChartOfAccountId)
        {
            var glError = await BankGlGuard.ValidateGlAsync(
                context, bank.TenantId, request.Dto.ChartOfAccountId, bank.Id, cancellationToken);
            if (glError is not null)
                return Result<BankDto>.Failure(glError.ErrorCode!, glError.ErrorMessage ?? "Invalid GL account.");
        }

        try
        {
            bank.Update(request.Dto.NameAr, request.Dto.NameEn, request.Dto.Code, request.Dto.SwiftCode,
                request.Dto.DefaultIban, request.Dto.CompanyId, request.Dto.BranchId,
                request.Dto.BaseCurrencyId, request.Dto.SortOrder);

            if (!inUse && request.Dto.ChartOfAccountId != bank.ChartOfAccountId)
                bank.ChangeChartOfAccount(request.Dto.ChartOfAccountId);

            if (request.Dto.IsActive) bank.Activate();
            else bank.Deactivate(request.Dto.DeactivatedAt, request.Dto.DeactivationReason);

            BankMapper.ApplyAccounts(bank, request.Dto.Accounts);
            context.Banks.Update(bank);
            await context.SaveChangesAsync(cancellationToken);
            return Result<BankDto>.Success(await CreateBankCommandHandler.LoadDtoAsync(context, bank.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<BankDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class ActivateBankCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ActivateBankCommand, Result>
{
    public async Task<Result> Handle(ActivateBankCommand request, CancellationToken cancellationToken)
    {
        var bank = await context.Banks.FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
        if (bank is null) return Result.Failure(ErrorCodes.BankNotFound, "Bank not found.");
        bank.Activate();
        context.Banks.Update(bank);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class DeactivateBankCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeactivateBankCommand, Result>
{
    public async Task<Result> Handle(DeactivateBankCommand request, CancellationToken cancellationToken)
    {
        var bank = await context.Banks.FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
        if (bank is null) return Result.Failure(ErrorCodes.BankNotFound, "Bank not found.");
        bank.Deactivate(request.DeactivatedAt, request.Reason);
        context.Banks.Update(bank);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class DeleteBankCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteBankCommand, Result>
{
    public async Task<Result> Handle(DeleteBankCommand request, CancellationToken cancellationToken)
    {
        var bank = await context.Banks.Include(b => b.Accounts)
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
        if (bank is null) return Result.Failure(ErrorCodes.BankNotFound, "Bank not found.");

        try { bank.EnsureCanDelete(); }
        catch (BusinessException ex)
        { return Result.Failure(ex.ErrorCode, ex.Message); }

        if (await BankUsageGuard.IsInUseAsync(context, bank, cancellationToken))
            return Result.Failure(ErrorCodes.BankInUse, "Bank is in use. Deactivate it instead.");

        bank.SoftDelete(null);
        context.Banks.Update(bank);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal static class BankUsageGuard
{
    public static async Task<bool> IsInUseAsync(IApplicationDbContext context, DomainBank bank, CancellationToken ct)
    {
        if (await context.JournalEntryLines.AnyAsync(l => l.ChartOfAccountId == bank.ChartOfAccountId, ct))
            return true;
        return false;
    }
}
