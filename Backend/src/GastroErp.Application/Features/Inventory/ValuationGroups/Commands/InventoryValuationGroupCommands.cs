using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.ValuationGroups.Dtos;
using GastroErp.Domain.Entities.Inventory.Catalog;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Inventory.ValuationGroups.Commands;

internal static class ValuationGroupMapper
{
    public static InventoryValuationGroupDto ToDto(InventoryValuationGroup e, string? costCenterNameAr = null) =>
        new(
            e.Id,
            e.TenantId,
            e.Code,
            e.NameAr,
            e.NameEn,
            e.Description,
            e.CostCenterId,
            costCenterNameAr,
            e.SortOrder,
            e.IsSystem,
            e.IsActive,
            e.CreatedAt.UtcDateTime);
}

internal static class ValuationGroupRules
{
    public static async Task<Result> EnsureUniqueAsync(
        IApplicationDbContext db,
        Guid tenantId,
        string code,
        string nameAr,
        Guid? excludeId,
        CancellationToken ct)
    {
        var q = db.InventoryValuationGroups.Where(x => x.TenantId == tenantId);
        if (excludeId.HasValue)
            q = q.Where(x => x.Id != excludeId.Value);

        if (await q.AnyAsync(x => x.Code == code, ct))
            return Result.Failure("DuplicateValuationGroupCode", "Valuation group code must be unique.");

        if (await q.AnyAsync(x => x.NameAr == nameAr.Trim(), ct))
            return Result.Failure("DuplicateValuationGroupName", "Valuation group name must be unique.");

        return Result.Success();
    }
}

public sealed record CreateInventoryValuationGroupCommand(CreateInventoryValuationGroupRequest Request)
    : IRequest<Result<InventoryValuationGroupDto>>;

public sealed class CreateInventoryValuationGroupCommandHandler(
    IApplicationDbContext db,
    ILogger<CreateInventoryValuationGroupCommandHandler> logger)
    : IRequestHandler<CreateInventoryValuationGroupCommand, Result<InventoryValuationGroupDto>>
{
    public async Task<Result<InventoryValuationGroupDto>> Handle(
        CreateInventoryValuationGroupCommand command,
        CancellationToken cancellationToken)
    {
        var r = command.Request;
        var code = r.Code.Trim().ToUpperInvariant();
        var unique = await ValuationGroupRules.EnsureUniqueAsync(
            db, r.TenantId, code, r.NameAr, null, cancellationToken);
        if (unique.IsFailure)
            return Result<InventoryValuationGroupDto>.Failure(unique.ErrorCode!, unique.ErrorMessage);

        if (r.CostCenterId.HasValue)
        {
            var exists = await db.CostCenters.AnyAsync(
                c => c.Id == r.CostCenterId.Value && c.TenantId == r.TenantId, cancellationToken);
            if (!exists)
                return Result<InventoryValuationGroupDto>.Failure("CostCenterNotFound", "Cost center not found.");
        }

        try
        {
            var entity = new InventoryValuationGroup(
                r.TenantId,
                code,
                r.NameAr,
                r.NameEn,
                r.Description,
                r.CostCenterId,
                r.SortOrder);

            db.InventoryValuationGroups.Add(entity);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Valuation group created: {Id} ({Code})", entity.Id, entity.Code);
            return Result<InventoryValuationGroupDto>.Success(ValuationGroupMapper.ToDto(entity));
        }
        catch (Exception ex) when (ex is ArgumentException or Domain.Common.Exceptions.BusinessException)
        {
            return Result<InventoryValuationGroupDto>.Failure("ValidationFailed", ex.Message);
        }
    }
}

public sealed record UpdateInventoryValuationGroupCommand(Guid Id, UpdateInventoryValuationGroupRequest Request)
    : IRequest<Result>;

public sealed class UpdateInventoryValuationGroupCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateInventoryValuationGroupCommand, Result>
{
    public async Task<Result> Handle(UpdateInventoryValuationGroupCommand command, CancellationToken cancellationToken)
    {
        var r = command.Request;
        var entity = await db.InventoryValuationGroups
            .FirstOrDefaultAsync(x => x.Id == command.Id && x.TenantId == r.TenantId, cancellationToken);
        if (entity is null)
            return Result.Failure("ValuationGroupNotFound", "Valuation group not found.");

        var unique = await ValuationGroupRules.EnsureUniqueAsync(
            db, r.TenantId, entity.Code, r.NameAr, command.Id, cancellationToken);
        if (unique.IsFailure) return unique;

        if (r.CostCenterId.HasValue)
        {
            var exists = await db.CostCenters.AnyAsync(
                c => c.Id == r.CostCenterId.Value && c.TenantId == r.TenantId, cancellationToken);
            if (!exists)
                return Result.Failure("CostCenterNotFound", "Cost center not found.");
        }

        try
        {
            entity.Update(r.NameAr, r.NameEn, r.Description, r.CostCenterId, r.SortOrder);
            if (r.IsActive) entity.Activate();
            else entity.Deactivate();
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Domain.Common.Exceptions.BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed record DeleteInventoryValuationGroupCommand(Guid Id, Guid TenantId) : IRequest<Result>;

public sealed class DeleteInventoryValuationGroupCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteInventoryValuationGroupCommand, Result>
{
    public async Task<Result> Handle(DeleteInventoryValuationGroupCommand command, CancellationToken cancellationToken)
    {
        var entity = await db.InventoryValuationGroups
            .FirstOrDefaultAsync(x => x.Id == command.Id && x.TenantId == command.TenantId, cancellationToken);
        if (entity is null)
            return Result.Failure("ValuationGroupNotFound", "Valuation group not found.");

        try
        {
            entity.SoftDeleteGroup("system");
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Domain.Common.Exceptions.BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed record ActivateInventoryValuationGroupCommand(Guid Id, Guid TenantId, bool IsActive) : IRequest<Result>;

public sealed class ActivateInventoryValuationGroupCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ActivateInventoryValuationGroupCommand, Result>
{
    public async Task<Result> Handle(ActivateInventoryValuationGroupCommand command, CancellationToken cancellationToken)
    {
        var entity = await db.InventoryValuationGroups
            .FirstOrDefaultAsync(x => x.Id == command.Id && x.TenantId == command.TenantId, cancellationToken);
        if (entity is null)
            return Result.Failure("ValuationGroupNotFound", "Valuation group not found.");

        if (command.IsActive) entity.Activate();
        else entity.Deactivate();
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
