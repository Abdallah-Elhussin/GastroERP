using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Entities.Finance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Finance.Commands;

internal static class CurrencyMapper
{
    public static CurrencyDto ToDto(Currency c) =>
        new(c.Id, c.Number, c.Code, c.NameAr, c.NameEn, c.Symbol, c.DecimalPlaces,
            c.SubUnitNameAr, c.SubUnitNameEn, c.CurrentExchangeRate, c.IsCompanyCurrency,
            c.IsForeignCurrency, c.Status, c.IsActive, c.IsSystem, c.SortOrder,
            c.LastExchangeRateAt, c.LastExchangeRateBy);

    public static CurrencyExchangeRateDto ToRateDto(CurrencyExchangeRate r, string code, string nameAr) =>
        new(r.Id, r.Number, r.CurrencyId, code, nameAr, r.Rate, r.StartDate, r.EndDate, r.IsActive, r.IsOpen,
            r.ChangeReason, r.CreatedBy, r.CreatedAt, r.UpdatedBy, r.UpdatedAt);
}

public sealed class CreateCurrencyCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateCurrencyCommand, Result<CurrencyDto>>
{
    public async Task<Result<CurrencyDto>> Handle(CreateCurrencyCommand request, CancellationToken cancellationToken)
    {
        var code = request.Dto.Code.Trim().ToUpperInvariant();
        if (await context.Currencies.AnyAsync(c => c.TenantId == request.TenantId && c.Code == code, cancellationToken))
            return Result<CurrencyDto>.Failure(ErrorCodes.CurrencyCodeDuplicate, "Currency code already exists.");

        var nameAr = request.Dto.NameAr.Trim();
        if (await context.Currencies.AnyAsync(c => c.TenantId == request.TenantId && c.NameAr == nameAr, cancellationToken))
            return Result<CurrencyDto>.Failure(ErrorCodes.CurrencyNameDuplicate, "Arabic name already exists.");

        if (request.Dto.IsCompanyCurrency
            && await context.Currencies.AnyAsync(c => c.TenantId == request.TenantId && c.IsCompanyCurrency, cancellationToken))
            return Result<CurrencyDto>.Failure(ErrorCodes.CurrencyCompanyAlreadySet, "Company currency is already set.");

        var nextNumber = await context.Currencies
            .Where(c => c.TenantId == request.TenantId)
            .Select(c => (int?)c.Number)
            .MaxAsync(cancellationToken) ?? 0;
        nextNumber++;

        try
        {
            var currency = Currency.Create(
                request.TenantId,
                nextNumber,
                code,
                nameAr,
                request.Dto.NameEn,
                request.Dto.CurrentExchangeRate,
                request.Dto.IsCompanyCurrency,
                request.Dto.Symbol,
                request.Dto.DecimalPlaces,
                request.Dto.SubUnitNameAr,
                request.Dto.SubUnitNameEn,
                request.Dto.SortOrder == 0 ? nextNumber : request.Dto.SortOrder,
                isSystem: false,
                rateUpdatedBy: request.UserName);

            if (!request.Dto.IsActive)
            {
                if (currency.IsCompanyCurrency)
                    return Result<CurrencyDto>.Failure(
                        ErrorCodes.CurrencyCompanyCannotDeactivate,
                        "Company currency must remain active.");
                currency.Deactivate();
            }

            context.Currencies.Add(currency);

            if (!currency.IsCompanyCurrency)
            {
                var rateNumber = await NextRateNumberAsync(context, request.TenantId, cancellationToken);
                context.CurrencyExchangeRates.Add(CurrencyExchangeRate.Create(
                    request.TenantId, currency.Id, rateNumber, currency.CurrentExchangeRate,
                    DateOnly.FromDateTime(DateTime.UtcNow), endDate: null, isActive: true,
                    changeReason: "Initial rate"));
            }

            await context.SaveChangesAsync(cancellationToken);
            return Result<CurrencyDto>.Success(CurrencyMapper.ToDto(currency));
        }
        catch (BusinessException ex)
        {
            return Result<CurrencyDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }

    internal static async Task<int> NextRateNumberAsync(IApplicationDbContext context, Guid tenantId, CancellationToken ct)
    {
        var max = await context.CurrencyExchangeRates
            .Where(r => r.TenantId == tenantId)
            .Select(r => (int?)r.Number)
            .MaxAsync(ct) ?? 0;
        return max + 1;
    }
}

public sealed class UpdateCurrencyCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateCurrencyCommand, Result<CurrencyDto>>
{
    public async Task<Result<CurrencyDto>> Handle(UpdateCurrencyCommand request, CancellationToken cancellationToken)
    {
        var currency = await context.Currencies.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (currency is null)
            return Result<CurrencyDto>.Failure(ErrorCodes.CurrencyNotFound, "Currency not found.");

        var nameAr = request.Dto.NameAr.Trim();
        if (await context.Currencies.AnyAsync(
                c => c.TenantId == currency.TenantId && c.NameAr == nameAr && c.Id != currency.Id, cancellationToken))
            return Result<CurrencyDto>.Failure(ErrorCodes.CurrencyNameDuplicate, "Arabic name already exists.");

        try
        {
            currency.Update(
                request.Dto.NameAr, request.Dto.NameEn, request.Dto.Symbol, request.Dto.DecimalPlaces,
                request.Dto.SubUnitNameAr, request.Dto.SubUnitNameEn, request.Dto.SortOrder);

            // سعر الصرف الحالي يُدار عبر شاشة معامل التحويل — لا تُنشأ فترات من هنا إلا إذا طُلب تغيير يدوي نادر.
            if (request.Dto.CurrentExchangeRate is decimal rate
                && rate != currency.CurrentExchangeRate
                && !currency.IsCompanyCurrency)
            {
                currency.SetExchangeRate(rate, request.UserName);
                await ExchangeRatePeriodHelper.CreateOrExtendAsync(
                    context, currency, rate, DateOnly.FromDateTime(DateTime.UtcNow),
                    request.UserName, "Updated from currency screen", cancellationToken);
            }

            if (request.Dto.IsActive && !currency.IsActive)
                currency.Activate();
            else if (!request.Dto.IsActive && currency.IsActive)
                currency.Deactivate();

            context.Currencies.Update(currency);
            await context.SaveChangesAsync(cancellationToken);
            return Result<CurrencyDto>.Success(CurrencyMapper.ToDto(currency));
        }
        catch (BusinessException ex)
        {
            return Result<CurrencyDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class ActivateCurrencyCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ActivateCurrencyCommand, Result>
{
    public async Task<Result> Handle(ActivateCurrencyCommand request, CancellationToken cancellationToken)
    {
        var currency = await context.Currencies.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (currency is null) return Result.Failure(ErrorCodes.CurrencyNotFound, "Currency not found.");
        currency.Activate();
        context.Currencies.Update(currency);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class DeactivateCurrencyCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeactivateCurrencyCommand, Result>
{
    public async Task<Result> Handle(DeactivateCurrencyCommand request, CancellationToken cancellationToken)
    {
        var currency = await context.Currencies.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (currency is null) return Result.Failure(ErrorCodes.CurrencyNotFound, "Currency not found.");
        try
        {
            currency.Deactivate();
            context.Currencies.Update(currency);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class DeleteCurrencyCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteCurrencyCommand, Result>
{
    public async Task<Result> Handle(DeleteCurrencyCommand request, CancellationToken cancellationToken)
    {
        var currency = await context.Currencies.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (currency is null) return Result.Failure(ErrorCodes.CurrencyNotFound, "Currency not found.");

        try { currency.EnsureCanDelete(); }
        catch (BusinessException ex)
        { return Result.Failure(ex.ErrorCode, ex.Message); }

        if (await CurrencyUsageGuard.IsInUseAsync(context, currency, cancellationToken))
            return Result.Failure(ErrorCodes.CurrencyInUse, "Currency is in use. Deactivate it instead.");

        var rates = await context.CurrencyExchangeRates
            .Where(r => r.CurrencyId == currency.Id)
            .ToListAsync(cancellationToken);
        if (rates.Count > 0)
            context.CurrencyExchangeRates.RemoveRange(rates);

        currency.SoftDelete(null);
        context.Currencies.Update(currency);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class SetCompanyCurrencyCommandHandler(IApplicationDbContext context)
    : IRequestHandler<SetCompanyCurrencyCommand, Result<CurrencyDto>>
{
    public async Task<Result<CurrencyDto>> Handle(SetCompanyCurrencyCommand request, CancellationToken cancellationToken)
    {
        var currency = await context.Currencies
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.TenantId == request.TenantId, cancellationToken);
        if (currency is null)
            return Result<CurrencyDto>.Failure(ErrorCodes.CurrencyNotFound, "Currency not found.");

        var previous = await context.Currencies
            .Where(c => c.TenantId == request.TenantId && c.IsCompanyCurrency && c.Id != currency.Id)
            .ToListAsync(cancellationToken);

        foreach (var prev in previous)
        {
            prev.ClearCompanyCurrencyFlag();
            context.Currencies.Update(prev);
        }

        currency.MarkAsCompanyCurrency();
        if (!currency.IsActive)
            currency.Activate();

        context.Currencies.Update(currency);
        await context.SaveChangesAsync(cancellationToken);
        return Result<CurrencyDto>.Success(CurrencyMapper.ToDto(currency));
    }
}

public sealed class CreateCurrencyExchangeRateCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateCurrencyExchangeRateCommand, Result<CurrencyExchangeRateDto>>
{
    public async Task<Result<CurrencyExchangeRateDto>> Handle(
        CreateCurrencyExchangeRateCommand request, CancellationToken cancellationToken)
    {
        var currency = await context.Currencies
            .FirstOrDefaultAsync(c => c.Id == request.Dto.CurrencyId && c.TenantId == request.TenantId, cancellationToken);
        if (currency is null)
            return Result<CurrencyExchangeRateDto>.Failure(ErrorCodes.CurrencyNotFound, "Currency not found.");
        if (currency.IsCompanyCurrency)
            return Result<CurrencyExchangeRateDto>.Failure(
                ErrorCodes.CurrencyExchangeRateInvalid, "Company currency rate is always 1 and needs no period rows.");

        try
        {
            var start = request.Dto.StartDate;
            var end = request.Dto.EndDate;

            if (request.Dto.AutoClosePreviousOpen && end is null)
            {
                var openRows = await context.CurrencyExchangeRates
                    .Where(r => r.TenantId == request.TenantId && r.CurrencyId == currency.Id && r.EndDate == null)
                    .ToListAsync(cancellationToken);

                foreach (var open in openRows)
                {
                    var closeDate = start.AddDays(-1);
                    if (closeDate < open.StartDate)
                        return Result<CurrencyExchangeRateDto>.Failure(
                            ErrorCodes.CurrencyExchangeRateInvalidPeriod,
                            "New start date is on or before the open period start. Close or adjust the previous period first.");
                    open.Close(closeDate);
                    context.CurrencyExchangeRates.Update(open);
                }
            }
            else if (end is null)
            {
                var hasOpen = await context.CurrencyExchangeRates.AnyAsync(
                    r => r.TenantId == request.TenantId && r.CurrencyId == currency.Id && r.EndDate == null,
                    cancellationToken);
                if (hasOpen)
                    return Result<CurrencyExchangeRateDto>.Failure(
                        ErrorCodes.CurrencyExchangeRateOpenExists,
                        "An open exchange rate period already exists for this currency.");
            }

            var existing = await context.CurrencyExchangeRates
                .Where(r => r.TenantId == request.TenantId && r.CurrencyId == currency.Id)
                .ToListAsync(cancellationToken);

            if (existing.Any(r => CurrencyExchangeRate.PeriodsOverlap(r.StartDate, r.EndDate, start, end)))
                return Result<CurrencyExchangeRateDto>.Failure(
                    ErrorCodes.CurrencyExchangeRateOverlap,
                    "Exchange rate period overlaps an existing period for this currency.");

            var number = await CreateCurrencyCommandHandler.NextRateNumberAsync(context, request.TenantId, cancellationToken);
            var rate = CurrencyExchangeRate.Create(
                request.TenantId, currency.Id, number, request.Dto.Rate, start, end,
                request.Dto.IsActive, request.Dto.ChangeReason);

            context.CurrencyExchangeRates.Add(rate);

            if (rate.IsOpen && rate.IsActive)
                currency.SetExchangeRate(rate.Rate, request.UserName);

            context.Currencies.Update(currency);
            await context.SaveChangesAsync(cancellationToken);
            return Result<CurrencyExchangeRateDto>.Success(CurrencyMapper.ToRateDto(rate, currency.Code, currency.NameAr));
        }
        catch (BusinessException ex)
        {
            return Result<CurrencyExchangeRateDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class UpdateCurrencyExchangeRateCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateCurrencyExchangeRateCommand, Result<CurrencyExchangeRateDto>>
{
    public async Task<Result<CurrencyExchangeRateDto>> Handle(
        UpdateCurrencyExchangeRateCommand request, CancellationToken cancellationToken)
    {
        var rate = await context.CurrencyExchangeRates.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (rate is null)
            return Result<CurrencyExchangeRateDto>.Failure(ErrorCodes.CurrencyExchangeRateNotFound, "Exchange rate not found.");

        var currency = await context.Currencies.FirstOrDefaultAsync(c => c.Id == rate.CurrencyId, cancellationToken);
        if (currency is null)
            return Result<CurrencyExchangeRateDto>.Failure(ErrorCodes.CurrencyNotFound, "Currency not found.");

        if (await ExchangeRateUsageGuard.IsInUseAsync(context, rate, currency.Code, cancellationToken))
            return Result<CurrencyExchangeRateDto>.Failure(
                ErrorCodes.CurrencyExchangeRateInUse,
                "Exchange rate is in use. Deactivate it instead of editing.");

        try
        {
            var siblings = await context.CurrencyExchangeRates
                .Where(r => r.TenantId == rate.TenantId && r.CurrencyId == rate.CurrencyId && r.Id != rate.Id)
                .ToListAsync(cancellationToken);

            if (siblings.Any(r => CurrencyExchangeRate.PeriodsOverlap(
                    r.StartDate, r.EndDate, request.Dto.StartDate, request.Dto.EndDate)))
                return Result<CurrencyExchangeRateDto>.Failure(
                    ErrorCodes.CurrencyExchangeRateOverlap,
                    "Exchange rate period overlaps an existing period for this currency.");

            if (request.Dto.EndDate is null
                && siblings.Any(r => r.EndDate is null))
                return Result<CurrencyExchangeRateDto>.Failure(
                    ErrorCodes.CurrencyExchangeRateOpenExists,
                    "An open exchange rate period already exists for this currency.");

            rate.Update(request.Dto.Rate, request.Dto.StartDate, request.Dto.EndDate, request.Dto.ChangeReason);
            if (request.Dto.IsActive) rate.Activate();
            else rate.Deactivate();

            if (rate.IsOpen && rate.IsActive)
                currency.SetExchangeRate(rate.Rate, request.UserName);

            context.CurrencyExchangeRates.Update(rate);
            context.Currencies.Update(currency);
            await context.SaveChangesAsync(cancellationToken);
            return Result<CurrencyExchangeRateDto>.Success(CurrencyMapper.ToRateDto(rate, currency.Code, currency.NameAr));
        }
        catch (BusinessException ex)
        {
            return Result<CurrencyExchangeRateDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class ActivateCurrencyExchangeRateCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ActivateCurrencyExchangeRateCommand, Result>
{
    public async Task<Result> Handle(ActivateCurrencyExchangeRateCommand request, CancellationToken cancellationToken)
    {
        var rate = await context.CurrencyExchangeRates.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (rate is null) return Result.Failure(ErrorCodes.CurrencyExchangeRateNotFound, "Exchange rate not found.");
        rate.Activate();
        context.CurrencyExchangeRates.Update(rate);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class DeactivateCurrencyExchangeRateCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeactivateCurrencyExchangeRateCommand, Result>
{
    public async Task<Result> Handle(DeactivateCurrencyExchangeRateCommand request, CancellationToken cancellationToken)
    {
        var rate = await context.CurrencyExchangeRates.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (rate is null) return Result.Failure(ErrorCodes.CurrencyExchangeRateNotFound, "Exchange rate not found.");
        rate.Deactivate();
        context.CurrencyExchangeRates.Update(rate);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class DeleteCurrencyExchangeRateCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteCurrencyExchangeRateCommand, Result>
{
    public async Task<Result> Handle(DeleteCurrencyExchangeRateCommand request, CancellationToken cancellationToken)
    {
        var rate = await context.CurrencyExchangeRates.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (rate is null)
            return Result.Failure(ErrorCodes.CurrencyExchangeRateNotFound, "Exchange rate not found.");

        var currency = await context.Currencies.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == rate.CurrencyId, cancellationToken);
        var code = currency?.Code ?? string.Empty;

        if (await ExchangeRateUsageGuard.IsInUseAsync(context, rate, code, cancellationToken))
            return Result.Failure(ErrorCodes.CurrencyExchangeRateInUse, "Exchange rate is in use. Deactivate it instead.");

        rate.SoftDelete(null);
        context.CurrencyExchangeRates.Update(rate);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal static class ExchangeRatePeriodHelper
{
    public static async Task CreateOrExtendAsync(
        IApplicationDbContext context,
        Currency currency,
        decimal rate,
        DateOnly start,
        string? userName,
        string? reason,
        CancellationToken ct)
    {
        var openRows = await context.CurrencyExchangeRates
            .Where(r => r.TenantId == currency.TenantId && r.CurrencyId == currency.Id && r.EndDate == null)
            .ToListAsync(ct);

        foreach (var open in openRows)
        {
            var closeDate = start.AddDays(-1);
            if (closeDate >= open.StartDate)
            {
                open.Close(closeDate);
                context.CurrencyExchangeRates.Update(open);
            }
        }

        var number = await CreateCurrencyCommandHandler.NextRateNumberAsync(context, currency.TenantId, ct);
        context.CurrencyExchangeRates.Add(CurrencyExchangeRate.Create(
            currency.TenantId, currency.Id, number, rate, start, null, true, reason));
        currency.SetExchangeRate(rate, userName);
    }
}

internal static class CurrencyUsageGuard
{
    public static async Task<bool> IsInUseAsync(IApplicationDbContext context, Currency currency, CancellationToken ct)
    {
        var code = currency.Code;
        var id = currency.Id;

        if (await context.JournalEntryLines.AnyAsync(l => l.Currency == code, ct))
            return true;
        if (await context.ChartOfAccounts.AnyAsync(a => a.Currency == code, ct))
            return true;
        if (await context.SalesOrders.AnyAsync(o => o.Currency == code, ct))
            return true;
        if (await context.Payments.AnyAsync(p => p.Currency == code, ct))
            return true;
        if (await context.DebitNotes.AnyAsync(d => d.Currency == code, ct))
            return true;
        if (await context.ProductPriceHistories.AnyAsync(p => p.Currency == code, ct))
            return true;
        if (await context.ProductPrices.AnyAsync(p => p.CurrencyId == id, ct))
            return true;
        if (await context.Branches.AnyAsync(b => b.CurrencyId == id, ct))
            return true;
        if (await context.OrganizationSettings.AnyAsync(s => s.DefaultCurrencyId == id, ct))
            return true;
        if (await context.TenantCurrencies.AnyAsync(t => t.TenantId == currency.TenantId && t.CurrencyCode == code, ct))
            return true;
        if (await context.InventorySettings.AnyAsync(s => s.DefaultCurrencyCode == code, ct))
            return true;

        return false;
    }
}

internal static class ExchangeRateUsageGuard
{
    public static async Task<bool> IsInUseAsync(
        IApplicationDbContext context, CurrencyExchangeRate rate, string currencyCode, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            return false;

        var start = rate.StartDate;
        var end = rate.EndDate ?? DateOnly.MaxValue;

        var journalInUse = await (
            from line in context.JournalEntryLines.AsNoTracking()
            join journal in context.JournalEntries.AsNoTracking() on line.JournalEntryId equals journal.Id
            where line.Currency == currencyCode
                  && journal.PostingDate >= start
                  && journal.PostingDate <= end
            select line.Id).AnyAsync(ct);

        if (journalInUse)
            return true;

        // أي حركة بيع بنفس العملة تعتبر استخداماً محافظاً للسجل التاريخي المفتوح/المغلق.
        if (rate.IsOpen && await context.SalesOrders.AsNoTracking().AnyAsync(o => o.Currency == currencyCode, ct))
            return true;

        return false;
    }
}
