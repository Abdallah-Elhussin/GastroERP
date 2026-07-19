using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Entities.Finance;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainCashBox = GastroErp.Domain.Entities.Finance.CashBox;

namespace GastroErp.Application.Features.Finance.Commands;

internal static class CashBoxMapper
{
    public static CashBoxDto ToDto(
        DomainCashBox box,
        string? companyName,
        string? branchName,
        string? accountNumber,
        string? accountName,
        string? currencyCode,
        string? posDeviceName,
        string? currentUserName,
        IReadOnlyDictionary<Guid, string>? userNames = null,
        IReadOnlyDictionary<Guid, string>? deviceNames = null)
    {
        var users = box.AuthorizedUsers
            .Select(u => new CashBoxUserDto(
                u.Id, u.UserId, userNames?.GetValueOrDefault(u.UserId),
                u.RoleName, u.IsDefault, u.IsManager, u.IsCustodian))
            .ToList();

        var devices = box.Devices
            .Select(d => new CashBoxDeviceDto(
                d.Id, d.DeviceId,
                d.DeviceId is Guid did ? deviceNames?.GetValueOrDefault(did) : null,
                d.DeviceRole, d.Label))
            .ToList();

        return new CashBoxDto(
            box.Id, box.Number, box.Code, box.NameAr, box.NameEn,
            box.CompanyId, companyName, box.BranchId, branchName,
            box.LocationName, box.PosDeviceId, posDeviceName,
            box.ChartOfAccountId, accountNumber, accountName,
            box.CurrencyId, currencyCode,
            box.OpeningBalance, box.OpeningDate, box.Description,
            box.IsActive, box.AllowReceive, box.AllowPay, box.AllowDeposit, box.AllowWithdraw,
            box.AllowTransfer, box.RequireShiftBeforeUse, box.AllowNegativeBalance,
            box.MinBalance, box.MaxBalance,
            box.CurrentBalance, box.CurrentUserId, currentUserName,
            box.LastOpenedAt, box.LastClosedAt, box.LastMovementAt, box.LastCountAt,
            box.IsOpen, box.HasHadMovement, box.IsSystem, box.SortOrder,
            box.CreatedAt, box.CreatedBy, box.UpdatedAt, box.UpdatedBy,
            users, devices);
    }

    public static void ApplyChildren(DomainCashBox box, UpsertCashBoxDto dto)
    {
        if (dto.AuthorizedUsers is not null)
        {
            box.ReplaceAuthorizedUsers(dto.AuthorizedUsers.Select(u =>
                CashBoxUser.Create(box.Id, u.UserId, u.RoleName, u.IsDefault, u.IsManager, u.IsCustodian)));
        }

        if (dto.Devices is not null)
        {
            box.ReplaceDevices(dto.Devices.Select(d =>
                CashBoxDevice.Create(box.Id, d.DeviceRole, d.DeviceId, d.Label)));
        }
    }
}

internal static class CashBoxGlGuard
{
    public static async Task<Result?> ValidateGlAsync(
        IApplicationDbContext context, Guid tenantId, Guid chartOfAccountId, Guid? excludeId, CancellationToken ct)
    {
        var account = await context.ChartOfAccounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == chartOfAccountId && a.TenantId == tenantId, ct);
        if (account is null || !account.IsActive || account.IsSummaryAccount || !account.IsPostingAllowed
            || account.AccountType != AccountType.Asset)
            return Result.Failure(ErrorCodes.CashBoxGlAccountInvalid,
                "GL account must be an active posting asset account.");

        if (account.AccountClassificationId is Guid classId)
        {
            var code = await context.AccountClassifications.AsNoTracking()
                .Where(c => c.Id == classId)
                .Select(c => c.Code)
                .FirstOrDefaultAsync(ct);
            if (!string.IsNullOrWhiteSpace(code)
                && !string.Equals(code, "cash", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(code, "bank", StringComparison.OrdinalIgnoreCase))
                return Result.Failure(ErrorCodes.CashBoxGlAccountInvalid,
                    "GL account classification should be Cash.");
        }

        var taken = await context.CashBoxes.AnyAsync(
            b => b.TenantId == tenantId && b.ChartOfAccountId == chartOfAccountId
                 && (excludeId == null || b.Id != excludeId), ct);
        if (taken)
            return Result.Failure(ErrorCodes.CashBoxGlAccountDuplicate,
                "This GL account is already linked to another cash box.");

        return null;
    }
}

public sealed class CreateCashBoxCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateCashBoxCommand, Result<CashBoxDto>>
{
    public async Task<Result<CashBoxDto>> Handle(CreateCashBoxCommand request, CancellationToken cancellationToken)
    {
        var nameAr = request.Dto.NameAr.Trim();
        if (await context.CashBoxes.AnyAsync(b => b.TenantId == request.TenantId && b.NameAr == nameAr, cancellationToken))
            return Result<CashBoxDto>.Failure(ErrorCodes.CashBoxNameDuplicate, "Cash box name already exists.");

        var companyOk = await context.Companies.AnyAsync(
            c => c.Id == request.Dto.CompanyId && c.TenantId == request.TenantId, cancellationToken);
        var branchOk = await context.Branches.AnyAsync(
            b => b.Id == request.Dto.BranchId && b.TenantId == request.TenantId
                 && b.CompanyId == request.Dto.CompanyId, cancellationToken);
        if (!companyOk || !branchOk)
            return Result<CashBoxDto>.Failure(ErrorCodes.RequiredField, "Company and branch are required.");

        if (!await context.Currencies.AnyAsync(
                c => c.Id == request.Dto.CurrencyId && c.TenantId == request.TenantId, cancellationToken))
            return Result<CashBoxDto>.Failure(ErrorCodes.CurrencyNotFound, "Currency not found.");

        var glError = await CashBoxGlGuard.ValidateGlAsync(
            context, request.TenantId, request.Dto.ChartOfAccountId, null, cancellationToken);
        if (glError is not null)
            return Result<CashBoxDto>.Failure(glError.ErrorCode!, glError.ErrorMessage ?? "Invalid GL account.");

        try
        {
            var next = await context.CashBoxes.Where(b => b.TenantId == request.TenantId)
                .Select(b => (int?)b.Number).MaxAsync(cancellationToken) ?? 0;
            var number = next + 1;
            var code = $"CASH-{number:D4}";

            var box = DomainCashBox.Create(
                request.TenantId, number, code, nameAr,
                request.Dto.CompanyId, request.Dto.BranchId,
                request.Dto.ChartOfAccountId, request.Dto.CurrencyId,
                request.Dto.NameEn, request.Dto.LocationName, request.Dto.PosDeviceId,
                request.Dto.OpeningBalance, request.Dto.OpeningDate, request.Dto.Description,
                request.Dto.SortOrder == 0 ? number : request.Dto.SortOrder);

            box.SetOperatingFlags(
                request.Dto.AllowReceive, request.Dto.AllowPay, request.Dto.AllowDeposit,
                request.Dto.AllowWithdraw, request.Dto.AllowTransfer,
                request.Dto.RequireShiftBeforeUse, request.Dto.AllowNegativeBalance);
            box.SetLimits(request.Dto.MinBalance, request.Dto.MaxBalance);
            if (!request.Dto.IsActive) box.Deactivate();

            CashBoxMapper.ApplyChildren(box, request.Dto);
            context.CashBoxes.Add(box);
            await context.SaveChangesAsync(cancellationToken);
            return Result<CashBoxDto>.Success(await LoadDtoAsync(context, box.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<CashBoxDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }

    internal static async Task<CashBoxDto> LoadDtoAsync(IApplicationDbContext context, Guid id, CancellationToken ct)
    {
        var box = await context.CashBoxes.AsNoTracking()
            .Include(b => b.AuthorizedUsers)
            .Include(b => b.Devices)
            .FirstAsync(b => b.Id == id, ct);
        return await EnrichAsync(context, box, ct);
    }

    internal static async Task<CashBoxDto> EnrichAsync(IApplicationDbContext context, DomainCashBox box, CancellationToken ct)
    {
        var company = await context.Companies.AsNoTracking()
            .Where(c => c.Id == box.CompanyId).Select(c => c.NameAr).FirstOrDefaultAsync(ct);
        var branch = await context.Branches.AsNoTracking()
            .Where(b => b.Id == box.BranchId).Select(b => b.NameAr).FirstOrDefaultAsync(ct);
        var account = await context.ChartOfAccounts.AsNoTracking()
            .Where(a => a.Id == box.ChartOfAccountId)
            .Select(a => new { a.AccountNumber, a.NameAr }).FirstOrDefaultAsync(ct);
        var currency = await context.Currencies.AsNoTracking()
            .Where(c => c.Id == box.CurrencyId).Select(c => c.Code).FirstOrDefaultAsync(ct);

        string? posName = null;
        if (box.PosDeviceId is Guid posId)
            posName = await context.Devices.AsNoTracking()
                .Where(d => d.Id == posId).Select(d => d.NameAr).FirstOrDefaultAsync(ct);

        string? currentUserName = null;
        if (box.CurrentUserId is Guid cuid)
            currentUserName = await context.AppUsers.AsNoTracking()
                .Where(u => u.Id == cuid)
                .Select(u => (u.FirstName + " " + u.LastName).Trim())
                .FirstOrDefaultAsync(ct);

        var userIds = box.AuthorizedUsers.Select(u => u.UserId).Distinct().ToList();
        var userNames = userIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await context.AppUsers.AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => (u.FirstName + " " + u.LastName).Trim(), ct);

        var deviceIds = box.Devices.Where(d => d.DeviceId.HasValue).Select(d => d.DeviceId!.Value).Distinct().ToList();
        var deviceNames = deviceIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await context.Devices.AsNoTracking()
                .Where(d => deviceIds.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id, d => d.NameAr, ct);

        return CashBoxMapper.ToDto(box, company, branch, account?.AccountNumber, account?.NameAr,
            currency, posName, currentUserName, userNames, deviceNames);
    }
}

public sealed class UpdateCashBoxCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateCashBoxCommand, Result<CashBoxDto>>
{
    public async Task<Result<CashBoxDto>> Handle(UpdateCashBoxCommand request, CancellationToken cancellationToken)
    {
        var box = await context.CashBoxes
            .Include(b => b.AuthorizedUsers)
            .Include(b => b.Devices)
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
        if (box is null)
            return Result<CashBoxDto>.Failure(ErrorCodes.CashBoxNotFound, "Cash box not found.");

        var nameAr = request.Dto.NameAr.Trim();
        if (await context.CashBoxes.AnyAsync(
                b => b.TenantId == box.TenantId && b.NameAr == nameAr && b.Id != box.Id, cancellationToken))
            return Result<CashBoxDto>.Failure(ErrorCodes.CashBoxNameDuplicate, "Cash box name already exists.");

        if (request.Dto.ChartOfAccountId != box.ChartOfAccountId)
        {
            if (box.HasHadMovement)
                return Result<CashBoxDto>.Failure(ErrorCodes.CashBoxGlAccountLocked,
                    "Cannot change GL account after movements.");
            var glError = await CashBoxGlGuard.ValidateGlAsync(
                context, box.TenantId, request.Dto.ChartOfAccountId, box.Id, cancellationToken);
            if (glError is not null)
                return Result<CashBoxDto>.Failure(glError.ErrorCode!, glError.ErrorMessage ?? "Invalid GL account.");
        }

        try
        {
            box.Update(request.Dto.NameAr, request.Dto.NameEn, request.Dto.CompanyId, request.Dto.BranchId,
                request.Dto.LocationName, request.Dto.PosDeviceId, request.Dto.CurrencyId,
                request.Dto.OpeningDate, request.Dto.Description, request.Dto.SortOrder);

            if (!box.HasHadMovement)
                box.SetOpeningBalance(request.Dto.OpeningBalance);

            if (!box.HasHadMovement && request.Dto.ChartOfAccountId != box.ChartOfAccountId)
                box.ChangeChartOfAccount(request.Dto.ChartOfAccountId);

            box.SetOperatingFlags(
                request.Dto.AllowReceive, request.Dto.AllowPay, request.Dto.AllowDeposit,
                request.Dto.AllowWithdraw, request.Dto.AllowTransfer,
                request.Dto.RequireShiftBeforeUse, request.Dto.AllowNegativeBalance);
            box.SetLimits(request.Dto.MinBalance, request.Dto.MaxBalance);

            if (request.Dto.IsActive) box.Activate();
            else box.Deactivate();

            CashBoxMapper.ApplyChildren(box, request.Dto);
            context.CashBoxes.Update(box);
            await context.SaveChangesAsync(cancellationToken);
            return Result<CashBoxDto>.Success(await CreateCashBoxCommandHandler.LoadDtoAsync(context, box.Id, cancellationToken));
        }
        catch (BusinessException ex)
        {
            return Result<CashBoxDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class ActivateCashBoxCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ActivateCashBoxCommand, Result>
{
    public async Task<Result> Handle(ActivateCashBoxCommand request, CancellationToken cancellationToken)
    {
        var box = await context.CashBoxes.FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
        if (box is null) return Result.Failure(ErrorCodes.CashBoxNotFound, "Cash box not found.");
        box.Activate();
        context.CashBoxes.Update(box);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class DeactivateCashBoxCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeactivateCashBoxCommand, Result>
{
    public async Task<Result> Handle(DeactivateCashBoxCommand request, CancellationToken cancellationToken)
    {
        var box = await context.CashBoxes.FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
        if (box is null) return Result.Failure(ErrorCodes.CashBoxNotFound, "Cash box not found.");
        try
        {
            box.Deactivate();
            context.CashBoxes.Update(box);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class DeleteCashBoxCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteCashBoxCommand, Result>
{
    public async Task<Result> Handle(DeleteCashBoxCommand request, CancellationToken cancellationToken)
    {
        var box = await context.CashBoxes
            .Include(b => b.AuthorizedUsers)
            .Include(b => b.Devices)
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
        if (box is null) return Result.Failure(ErrorCodes.CashBoxNotFound, "Cash box not found.");

        try { box.EnsureCanDelete(); }
        catch (BusinessException ex)
        { return Result.Failure(ex.ErrorCode, ex.Message); }

        if (await context.JournalEntryLines.AnyAsync(l => l.ChartOfAccountId == box.ChartOfAccountId, cancellationToken))
            return Result.Failure(ErrorCodes.CashBoxInUse, "Cash box is in use. Deactivate it instead.");

        box.SoftDelete(null);
        context.CashBoxes.Update(box);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
