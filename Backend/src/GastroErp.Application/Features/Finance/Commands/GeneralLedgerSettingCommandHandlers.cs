using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Entities.Finance;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainGl = GastroErp.Domain.Entities.Finance.GeneralLedgerSetting;

namespace GastroErp.Application.Features.Finance.Commands;

internal static class GeneralLedgerSettingMapper
{
    public static GeneralLedgerSettingDto ToDto(DomainGl s, string? companyName, string? branchName)
        => new(
            s.Id, s.Number, s.CompanyId, companyName, s.BranchId, branchName,
            s.VoucherNumberLength, s.DecimalPlaces, s.ShowDateInReports,
            s.ShowPostingIndicator, s.AutoPostReceiptChecks, s.AutoPostPaymentChecks,
            s.UseBudgetPerCurrency, s.UseAnalyticalAccounts, s.AllowZeroEffectEntries, s.RequireJournalType,
            s.AllowManualTaxEntries, s.RequireReferenceNumber, s.ClosingMethod,
            s.ClosingMethod.ToString().ToUpperInvariant() switch
            {
                "SINGLESUMMARY" => "SINGLE_SUMMARY",
                "DIRECTTORETAINEDEARNINGS" => "DIRECT_TO_RETAINED_EARNINGS",
                "BYPROFITCENTER" => "BY_PROFIT_CENTER",
                "BYBRANCH" => "BY_BRANCH",
                _ => s.ClosingMethod.ToString().ToUpperInvariant()
            },
            s.IsSystem, s.CreatedAt, s.CreatedBy, s.UpdatedAt, s.UpdatedBy);
}

public sealed class CreateGeneralLedgerSettingCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateGeneralLedgerSettingCommand, Result<GeneralLedgerSettingDto>>
{
    public async Task<Result<GeneralLedgerSettingDto>> Handle(
        CreateGeneralLedgerSettingCommand request, CancellationToken cancellationToken)
    {
        var companyOk = await context.Companies.AnyAsync(
            c => c.Id == request.Dto.CompanyId && c.TenantId == request.TenantId, cancellationToken);
        var branchOk = await context.Branches.AnyAsync(
            b => b.Id == request.Dto.BranchId && b.TenantId == request.TenantId
                 && b.CompanyId == request.Dto.CompanyId, cancellationToken);
        if (!companyOk || !branchOk)
            return Result<GeneralLedgerSettingDto>.Failure(ErrorCodes.RequiredField, "Company and branch are required.");

        var exists = await context.GeneralLedgerSettings.AnyAsync(
            s => s.TenantId == request.TenantId
                 && s.CompanyId == request.Dto.CompanyId
                 && s.BranchId == request.Dto.BranchId, cancellationToken);
        if (exists)
            return Result<GeneralLedgerSettingDto>.Failure(
                ErrorCodes.GeneralLedgerSettingDuplicate,
                "A GL settings record already exists for this company and branch.");

        try
        {
            var next = await context.GeneralLedgerSettings.Where(s => s.TenantId == request.TenantId)
                .Select(s => (int?)s.Number).MaxAsync(cancellationToken) ?? 0;

            var setting = DomainGl.Create(
                request.TenantId, next + 1, request.Dto.CompanyId, request.Dto.BranchId,
                request.Dto.VoucherNumberLength, request.Dto.DecimalPlaces, request.Dto.ShowDateInReports,
                request.Dto.ShowPostingIndicator, request.Dto.AutoPostReceiptChecks,
                request.Dto.AutoPostPaymentChecks, request.Dto.UseBudgetPerCurrency,
                request.Dto.AllowZeroEffectEntries, request.Dto.RequireJournalType,
                request.Dto.AllowManualTaxEntries, request.Dto.RequireReferenceNumber,
                request.Dto.ClosingMethod, isSystem: false,
                useAnalyticalAccounts: request.Dto.UseAnalyticalAccounts);

            context.GeneralLedgerSettings.Add(setting);
            await context.SaveChangesAsync(cancellationToken);
            return Result<GeneralLedgerSettingDto>.Success(
                await LoadDtoAsync(context, setting.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<GeneralLedgerSettingDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }

    internal static async Task<GeneralLedgerSettingDto> LoadDtoAsync(
        IApplicationDbContext context, Guid id, CancellationToken ct)
    {
        var setting = await context.GeneralLedgerSettings.AsNoTracking().FirstAsync(s => s.Id == id, ct);
        return await EnrichAsync(context, setting, ct);
    }

    internal static async Task<GeneralLedgerSettingDto> EnrichAsync(
        IApplicationDbContext context, DomainGl setting, CancellationToken ct)
    {
        var company = await context.Companies.AsNoTracking()
            .Where(c => c.Id == setting.CompanyId).Select(c => c.NameAr).FirstOrDefaultAsync(ct);
        var branch = await context.Branches.AsNoTracking()
            .Where(b => b.Id == setting.BranchId).Select(b => b.NameAr).FirstOrDefaultAsync(ct);
        return GeneralLedgerSettingMapper.ToDto(setting, company, branch);
    }
}

public sealed class UpdateGeneralLedgerSettingCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateGeneralLedgerSettingCommand, Result<GeneralLedgerSettingDto>>
{
    public async Task<Result<GeneralLedgerSettingDto>> Handle(
        UpdateGeneralLedgerSettingCommand request, CancellationToken cancellationToken)
    {
        var setting = await context.GeneralLedgerSettings
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (setting is null)
            return Result<GeneralLedgerSettingDto>.Failure(
                ErrorCodes.GeneralLedgerSettingNotFound, "GL settings not found.");

        var branchOk = await context.Branches.AnyAsync(
            b => b.Id == request.Dto.BranchId && b.TenantId == setting.TenantId
                 && b.CompanyId == request.Dto.CompanyId, cancellationToken);
        if (!branchOk)
            return Result<GeneralLedgerSettingDto>.Failure(ErrorCodes.RequiredField, "Invalid company/branch.");

        var duplicate = await context.GeneralLedgerSettings.AnyAsync(
            s => s.TenantId == setting.TenantId
                 && s.CompanyId == request.Dto.CompanyId
                 && s.BranchId == request.Dto.BranchId
                 && s.Id != setting.Id, cancellationToken);
        if (duplicate)
            return Result<GeneralLedgerSettingDto>.Failure(
                ErrorCodes.GeneralLedgerSettingDuplicate,
                "A GL settings record already exists for this company and branch.");

        try
        {
            setting.Update(
                request.Dto.CompanyId, request.Dto.BranchId,
                request.Dto.VoucherNumberLength, request.Dto.DecimalPlaces, request.Dto.ShowDateInReports,
                request.Dto.ShowPostingIndicator, request.Dto.AutoPostReceiptChecks,
                request.Dto.AutoPostPaymentChecks, request.Dto.UseBudgetPerCurrency,
                request.Dto.AllowZeroEffectEntries, request.Dto.RequireJournalType,
                request.Dto.AllowManualTaxEntries, request.Dto.RequireReferenceNumber,
                request.Dto.ClosingMethod, request.Dto.UseAnalyticalAccounts);

            context.GeneralLedgerSettings.Update(setting);
            await context.SaveChangesAsync(cancellationToken);
            return Result<GeneralLedgerSettingDto>.Success(
                await CreateGeneralLedgerSettingCommandHandler.LoadDtoAsync(context, setting.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<GeneralLedgerSettingDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class DeleteGeneralLedgerSettingCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteGeneralLedgerSettingCommand, Result>
{
    public async Task<Result> Handle(DeleteGeneralLedgerSettingCommand request, CancellationToken cancellationToken)
    {
        var setting = await context.GeneralLedgerSettings
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (setting is null)
            return Result.Failure(ErrorCodes.GeneralLedgerSettingNotFound, "GL settings not found.");

        try { setting.EnsureCanDelete(); }
        catch (BusinessException ex)
        { return Result.Failure(ex.ErrorCode, ex.Message); }

        setting.SoftDelete(null);
        context.GeneralLedgerSettings.Update(setting);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
