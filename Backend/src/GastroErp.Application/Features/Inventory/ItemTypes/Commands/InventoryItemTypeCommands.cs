using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.ItemTypes.Dtos;
using GastroErp.Domain.Entities.Inventory.Catalog;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Inventory.ItemTypes.Commands;

public sealed record CreateInventoryItemTypeCommand(CreateInventoryItemTypeRequest Request)
    : IRequest<Result<InventoryItemTypeDto>>;

public sealed class CreateInventoryItemTypeCommandHandler(
    IApplicationDbContext db,
    ILogger<CreateInventoryItemTypeCommandHandler> logger)
    : IRequestHandler<CreateInventoryItemTypeCommand, Result<InventoryItemTypeDto>>
{
    public async Task<Result<InventoryItemTypeDto>> Handle(
        CreateInventoryItemTypeCommand command,
        CancellationToken cancellationToken)
    {
        var r = command.Request;
        var code = r.Code.Trim().ToUpperInvariant();

        var unique = await InventoryItemTypeRules.EnsureUniqueAsync(
            db, r.TenantId, code, r.NameAr, r.NameEn, excludeId: null, cancellationToken);
        if (unique.IsFailure) return Result<InventoryItemTypeDto>.Failure(unique.ErrorCode!, unique.ErrorMessage);

        var range = await InventoryItemTypeRules.EnsureCodeRangeNoOverlapAsync(
            db, r.TenantId, r.CodeStart, r.CodeEnd, excludeId: null, cancellationToken);
        if (range.IsFailure) return Result<InventoryItemTypeDto>.Failure(range.ErrorCode!, range.ErrorMessage);

        try
        {
            var entity = new InventoryItemType(
                r.TenantId,
                code,
                r.NameAr.Trim(),
                r.Category,
                string.IsNullOrWhiteSpace(r.NameEn) ? null : r.NameEn.Trim(),
                string.IsNullOrWhiteSpace(r.Description) ? null : r.Description.Trim(),
                r.CodeStart,
                r.CodeEnd,
                r.IsInventory,
                r.CanSell,
                r.CanPurchase,
                r.IsRecipe,
                r.IsProduction,
                r.AllowNegativeStock,
                r.Color,
                r.SortOrder,
                isSystem: false,
                r.CompanyId);

            db.InventoryItemTypes.Add(entity);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("InventoryItemType created: {Id} ({Code})", entity.Id, entity.Code);
            return Result<InventoryItemTypeDto>.Success(InventoryItemTypeMapper.ToDto(entity));
        }
        catch (Exception ex) when (ex is ArgumentException or Domain.Common.Exceptions.BusinessException)
        {
            return Result<InventoryItemTypeDto>.Failure("ValidationFailed", ex.Message);
        }
    }
}

public sealed record UpdateInventoryItemTypeCommand(Guid Id, UpdateInventoryItemTypeRequest Request)
    : IRequest<Result>;

public sealed class UpdateInventoryItemTypeCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateInventoryItemTypeCommand, Result>
{
    public async Task<Result> Handle(UpdateInventoryItemTypeCommand command, CancellationToken cancellationToken)
    {
        var r = command.Request;
        var entity = await db.InventoryItemTypes
            .FirstOrDefaultAsync(x => x.Id == command.Id && x.TenantId == r.TenantId, cancellationToken);
        if (entity is null)
            return Result.Failure("ItemTypeNotFound", "Item type not found.");

        var unique = await InventoryItemTypeRules.EnsureUniqueAsync(
            db, r.TenantId, entity.Code, r.NameAr, r.NameEn, command.Id, cancellationToken);
        if (unique.IsFailure) return unique;

        var range = await InventoryItemTypeRules.EnsureCodeRangeNoOverlapAsync(
            db, r.TenantId, r.CodeStart, r.CodeEnd, command.Id, cancellationToken);
        if (range.IsFailure) return range;

        try
        {
            entity.Update(
                entity.Code,
                r.NameAr.Trim(),
                string.IsNullOrWhiteSpace(r.NameEn) ? null : r.NameEn.Trim(),
                string.IsNullOrWhiteSpace(r.Description) ? null : r.Description.Trim(),
                r.Category,
                r.CodeStart,
                r.CodeEnd,
                r.IsInventory,
                r.CanSell,
                r.CanPurchase,
                r.IsRecipe,
                r.IsProduction,
                r.AllowNegativeStock,
                r.Color,
                r.SortOrder);

            if (r.IsActive) entity.Activate();
            else entity.Deactivate();

            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex) when (ex is ArgumentException or Domain.Common.Exceptions.BusinessException)
        {
            return Result.Failure("ValidationFailed", ex.Message);
        }
    }
}

public sealed record DeleteInventoryItemTypeCommand(Guid Id, Guid TenantId) : IRequest<Result>;

public sealed class DeleteInventoryItemTypeCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteInventoryItemTypeCommand, Result>
{
    public async Task<Result> Handle(DeleteInventoryItemTypeCommand command, CancellationToken cancellationToken)
    {
        var entity = await db.InventoryItemTypes
            .FirstOrDefaultAsync(x => x.Id == command.Id && x.TenantId == command.TenantId, cancellationToken);
        if (entity is null)
            return Result.Failure("ItemTypeNotFound", "Item type not found.");

        if (entity.IsSystem)
            return Result.Failure("CannotDeleteSystemItemType", "System item types cannot be deleted. Deactivate instead.");

        var inUse = await db.InventoryItems
            .AnyAsync(x => x.TenantId == command.TenantId && x.ItemTypeId == command.Id, cancellationToken);
        if (inUse)
            return Result.Failure("ItemTypeInUse", "Item type is used by inventory items and cannot be deleted. Deactivate instead.");

        try
        {
            entity.SoftDeleteType();
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Domain.Common.Exceptions.BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed record ActivateInventoryItemTypeCommand(Guid Id, Guid TenantId, bool IsActive) : IRequest<Result>;

public sealed class ActivateInventoryItemTypeCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ActivateInventoryItemTypeCommand, Result>
{
    public async Task<Result> Handle(ActivateInventoryItemTypeCommand command, CancellationToken cancellationToken)
    {
        var entity = await db.InventoryItemTypes
            .FirstOrDefaultAsync(x => x.Id == command.Id && x.TenantId == command.TenantId, cancellationToken);
        if (entity is null)
            return Result.Failure("ItemTypeNotFound", "Item type not found.");

        if (command.IsActive) entity.Activate();
        else entity.Deactivate();

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

internal static class InventoryItemTypeRules
{
    public static async Task<Result> EnsureUniqueAsync(
        IApplicationDbContext db,
        Guid tenantId,
        string code,
        string nameAr,
        string? nameEn,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        var q = db.InventoryItemTypes.Where(x => x.TenantId == tenantId);
        if (excludeId.HasValue)
            q = q.Where(x => x.Id != excludeId.Value);

        if (await q.AnyAsync(x => x.Code == code, cancellationToken))
            return Result.Failure("DuplicateItemTypeCode", "Item type code must be unique.");

        var nameArTrim = nameAr.Trim();
        if (await q.AnyAsync(x => x.NameAr == nameArTrim, cancellationToken))
            return Result.Failure("DuplicateItemTypeName", "Item type Arabic name must be unique.");

        if (!string.IsNullOrWhiteSpace(nameEn))
        {
            var nameEnTrim = nameEn.Trim();
            if (await q.AnyAsync(x => x.NameEn != null && x.NameEn == nameEnTrim, cancellationToken))
                return Result.Failure("DuplicateItemTypeName", "Item type English name must be unique.");
        }

        return Result.Success();
    }

    public static async Task<Result> EnsureCodeRangeNoOverlapAsync(
        IApplicationDbContext db,
        Guid tenantId,
        int? codeStart,
        int? codeEnd,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        if (!codeStart.HasValue || !codeEnd.HasValue)
            return Result.Success();

        if (codeStart > codeEnd)
            return Result.Failure("InvalidCodeRange", "Code range start cannot be greater than end.");

        var q = db.InventoryItemTypes.Where(x =>
            x.TenantId == tenantId
            && x.CodeStart != null
            && x.CodeEnd != null);
        if (excludeId.HasValue)
            q = q.Where(x => x.Id != excludeId.Value);

        var ranges = await q
            .Select(x => new { x.Code, x.CodeStart, x.CodeEnd })
            .ToListAsync(cancellationToken);

        foreach (var range in ranges)
        {
            if (codeStart <= range.CodeEnd && codeEnd >= range.CodeStart)
                return Result.Failure(
                    "CodeRangeOverlap",
                    $"Code range overlaps with item type '{range.Code}' ({range.CodeStart}-{range.CodeEnd}).");
        }

        return Result.Success();
    }
}

internal static class InventoryItemTypeMapper
{
    public static InventoryItemTypeDto ToDto(InventoryItemType e) => new()
    {
        Id = e.Id,
        Code = e.Code,
        NameAr = e.NameAr,
        NameEn = e.NameEn,
        Description = e.Description,
        Category = e.Category,
        CategoryName = e.Category.ToString(),
        CodeStart = e.CodeStart,
        CodeEnd = e.CodeEnd,
        IsInventory = e.IsInventory,
        CanSell = e.CanSell,
        CanPurchase = e.CanPurchase,
        IsRecipe = e.IsRecipe,
        IsProduction = e.IsProduction,
        AllowNegativeStock = e.AllowNegativeStock,
        Color = e.Color,
        SortOrder = e.SortOrder,
        IsSystem = e.IsSystem,
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt.UtcDateTime,
        UpdatedAt = e.UpdatedAt?.UtcDateTime
    };
}
