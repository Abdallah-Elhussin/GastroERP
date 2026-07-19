using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.Commands;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Entities.Finance;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Finance.Commands;

internal static class CostCenterMapper
{
    public static string TypeCode(CostCenterType type) => type switch
    {
        CostCenterType.Operational => "operational",
        CostCenterType.Administrative => "administration",
        CostCenterType.Production => "production",
        CostCenterType.Service => "service",
        CostCenterType.Branch => "branch",
        CostCenterType.Project => "project",
        _ => "other"
    };

    public static CostCenterDto ToDto(CostCenter c, int linkedCount, IReadOnlyList<Guid> allowedIds) =>
        new(c.Id, c.Number, c.BranchId, c.DepartmentId, c.ParentCostCenterId, c.Code, c.NameAr, c.NameEn,
            c.Description, c.CostCenterType, TypeCode(c.CostCenterType), c.Status, c.IsActive, c.IsSystem, c.SortOrder,
            linkedCount, c.UseInPurchases, c.UseInInventory, c.UseInProduction, c.UseInSales,
            c.UseInPayroll, c.UseInAssets, c.UseInMaintenance, c.UseInJournals, allowedIds);
}

public sealed class CreateCostCenterCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateCostCenterCommand, Result<CostCenterDto>>
{
    public async Task<Result<CostCenterDto>> Handle(CreateCostCenterCommand request, CancellationToken cancellationToken)
    {
        var branchId = request.Dto.BranchId ?? request.CurrentBranchId
            ?? await context.Branches.AsNoTracking()
                .Where(b => b.TenantId == request.TenantId)
                .Select(b => (Guid?)b.Id)
                .FirstOrDefaultAsync(cancellationToken);

        if (branchId is null || branchId == Guid.Empty)
            return Result<CostCenterDto>.Failure(ErrorCodes.RequiredField, "Branch is required to create a cost center.");

        var nameAr = request.Dto.NameAr.Trim();
        if (await context.CostCenters.AnyAsync(c => c.TenantId == request.TenantId && c.NameAr == nameAr, cancellationToken))
            return Result<CostCenterDto>.Failure(ErrorCodes.CostCenterNameDuplicate, "Arabic name already exists.");

        if (request.Dto.ParentCostCenterId is Guid parentId)
        {
            var parentOk = await context.CostCenters.AnyAsync(
                c => c.Id == parentId && c.TenantId == request.TenantId, cancellationToken);
            if (!parentOk)
                return Result<CostCenterDto>.Failure(ErrorCodes.CostCenterInvalidParent, "Parent cost center not found.");
        }

        var nextNumber = await context.CostCenters
            .Where(c => c.TenantId == request.TenantId)
            .Select(c => (int?)c.Number)
            .MaxAsync(cancellationToken) ?? 0;
        nextNumber++;

        var code = string.IsNullOrWhiteSpace(request.Dto.Code)
            ? $"CC-{nextNumber:D3}"
            : request.Dto.Code.Trim().ToUpperInvariant();

        if (await context.CostCenters.AnyAsync(c => c.TenantId == request.TenantId && c.Code == code, cancellationToken))
            return Result<CostCenterDto>.Failure(ErrorCodes.CostCenterCodeDuplicate, "Cost center code already exists.");

        try
        {
            var center = CostCenter.Create(
                request.TenantId, branchId.Value, nextNumber, code, nameAr, request.Dto.CostCenterType,
                request.Dto.ParentCostCenterId, request.Dto.DepartmentId, request.Dto.NameEn,
                request.Dto.Description, request.Dto.SortOrder == 0 ? nextNumber : request.Dto.SortOrder);

            center.Update(
                center.NameAr, center.NameEn, center.CostCenterType, center.ParentCostCenterId, center.DepartmentId,
                center.Description, center.SortOrder,
                request.Dto.UseInPurchases, request.Dto.UseInInventory, request.Dto.UseInProduction,
                request.Dto.UseInSales, request.Dto.UseInPayroll, request.Dto.UseInAssets,
                request.Dto.UseInMaintenance, request.Dto.UseInJournals);

            context.CostCenters.Add(center);
            await context.SaveChangesAsync(cancellationToken);

            var allowed = await ReplaceAllowedAccountsAsync(context, center.Id, request.Dto.AllowedAccountIds, cancellationToken);
            return Result<CostCenterDto>.Success(CostCenterMapper.ToDto(center, allowed.Count, allowed));
        }
        catch (BusinessException ex)
        {
            return Result<CostCenterDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }

    internal static async Task<IReadOnlyList<Guid>> ReplaceAllowedAccountsAsync(
        IApplicationDbContext context, Guid costCenterId, IReadOnlyList<Guid>? accountIds, CancellationToken ct)
    {
        var existing = await context.CostCenterAllowedAccounts
            .Where(a => a.CostCenterId == costCenterId)
            .ToListAsync(ct);
        if (existing.Count > 0)
            context.CostCenterAllowedAccounts.RemoveRange(existing);

        var ids = (accountIds ?? Array.Empty<Guid>()).Where(id => id != Guid.Empty).Distinct().ToList();
        foreach (var accountId in ids)
            context.CostCenterAllowedAccounts.Add(CostCenterAllowedAccount.Create(costCenterId, accountId));

        await context.SaveChangesAsync(ct);
        return ids;
    }
}

public sealed class UpdateCostCenterCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateCostCenterCommand, Result<CostCenterDto>>
{
    public async Task<Result<CostCenterDto>> Handle(UpdateCostCenterCommand request, CancellationToken cancellationToken)
    {
        var center = await context.CostCenters.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (center is null)
            return Result<CostCenterDto>.Failure(ErrorCodes.CostCenterNotFound, "Cost center not found.");

        var nameAr = request.Dto.NameAr.Trim();
        if (await context.CostCenters.AnyAsync(
                c => c.TenantId == center.TenantId && c.NameAr == nameAr && c.Id != center.Id, cancellationToken))
            return Result<CostCenterDto>.Failure(ErrorCodes.CostCenterNameDuplicate, "Arabic name already exists.");

        if (request.Dto.ParentCostCenterId is Guid parentId)
        {
            if (parentId == center.Id)
                return Result<CostCenterDto>.Failure(ErrorCodes.CostCenterInvalidParent, "Cost center cannot be its own parent.");

            var parentOk = await context.CostCenters.AnyAsync(
                c => c.Id == parentId && c.TenantId == center.TenantId, cancellationToken);
            if (!parentOk)
                return Result<CostCenterDto>.Failure(ErrorCodes.CostCenterInvalidParent, "Parent cost center not found.");

            // prevent cycles
            var cursorId = parentId;
            while (true)
            {
                var up = await context.CostCenters.AsNoTracking()
                    .Where(c => c.Id == cursorId)
                    .Select(c => c.ParentCostCenterId)
                    .FirstOrDefaultAsync(cancellationToken);
                if (up is null) break;
                if (up == center.Id)
                    return Result<CostCenterDto>.Failure(ErrorCodes.CostCenterInvalidParent, "Reparent would create a cycle.");
                cursorId = up.Value;
            }
        }

        try
        {
            center.Update(
                request.Dto.NameAr, request.Dto.NameEn, request.Dto.CostCenterType, request.Dto.ParentCostCenterId,
                request.Dto.DepartmentId, request.Dto.Description, request.Dto.SortOrder,
                request.Dto.UseInPurchases, request.Dto.UseInInventory, request.Dto.UseInProduction,
                request.Dto.UseInSales, request.Dto.UseInPayroll, request.Dto.UseInAssets,
                request.Dto.UseInMaintenance, request.Dto.UseInJournals);
            context.CostCenters.Update(center);
            await context.SaveChangesAsync(cancellationToken);

            var allowed = await CreateCostCenterCommandHandler.ReplaceAllowedAccountsAsync(
                context, center.Id, request.Dto.AllowedAccountIds, cancellationToken);
            return Result<CostCenterDto>.Success(CostCenterMapper.ToDto(center, allowed.Count, allowed));
        }
        catch (BusinessException ex)
        {
            return Result<CostCenterDto>.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed class ActivateCostCenterCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ActivateCostCenterCommand, Result>
{
    public async Task<Result> Handle(ActivateCostCenterCommand request, CancellationToken cancellationToken)
    {
        var center = await context.CostCenters.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (center is null) return Result.Failure(ErrorCodes.CostCenterNotFound, "Cost center not found.");
        center.Activate();
        context.CostCenters.Update(center);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class DeactivateCostCenterCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeactivateCostCenterCommand, Result>
{
    public async Task<Result> Handle(DeactivateCostCenterCommand request, CancellationToken cancellationToken)
    {
        var center = await context.CostCenters.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (center is null) return Result.Failure(ErrorCodes.CostCenterNotFound, "Cost center not found.");
        center.Deactivate();
        context.CostCenters.Update(center);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class DeleteCostCenterCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteCostCenterCommand, Result>
{
    public async Task<Result> Handle(DeleteCostCenterCommand request, CancellationToken cancellationToken)
    {
        var center = await context.CostCenters.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (center is null) return Result.Failure(ErrorCodes.CostCenterNotFound, "Cost center not found.");

        try { center.EnsureCanDelete(); }
        catch (BusinessException ex)
        { return Result.Failure(ex.ErrorCode, "System cost centers cannot be deleted."); }

        if (await context.CostCenters.AnyAsync(c => c.ParentCostCenterId == center.Id, cancellationToken))
            return Result.Failure(ErrorCodes.CostCenterHasChildren, "Cost center has child centers.");

        if (await CostCenterUsageGuard.IsInUseAsync(context, center.Id, cancellationToken))
            return Result.Failure(ErrorCodes.CostCenterInUse, "Cost center is in use. Deactivate it instead.");

        var links = await context.CostCenterAllowedAccounts.Where(a => a.CostCenterId == center.Id).ToListAsync(cancellationToken);
        if (links.Count > 0)
            context.CostCenterAllowedAccounts.RemoveRange(links);

        center.SoftDelete(null);
        context.CostCenters.Update(center);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal static class CostCenterUsageGuard
{
    public static async Task<bool> IsInUseAsync(IApplicationDbContext context, Guid costCenterId, CancellationToken ct)
    {
        if (await context.JournalEntryLines.AnyAsync(l => l.CostCenterId == costCenterId, ct))
            return true;
        if (await context.IssueDestinations.AnyAsync(d => d.DefaultCostCenterId == costCenterId, ct))
            return true;
        if (await context.OpeningBalances.AnyAsync(o => o.CostCenterId == costCenterId, ct))
            return true;
        if (await context.InventoryValuationGroups.AnyAsync(v => v.CostCenterId == costCenterId, ct))
            return true;

        return false;
    }
}
