using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Sales.ProductPricing.Dtos;
using GastroErp.Domain.Entities.Sales.Pricing;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Sales.ProductPricing.Commands;

internal static class ProductPricingMapper
{
    public static SalesPriceListDto ToDto(SalesPriceList e) =>
        new(
            e.Id,
            e.TenantId,
            e.Code,
            e.NameAr,
            e.NameEn,
            e.Description,
            e.DefaultSalesChannel,
            e.SortOrder,
            e.IsDefault,
            e.IsSystem,
            e.IsActive,
            e.CreatedAt.UtcDateTime);

    public static ProductPriceDto ToDto(
        ProductPrice e,
        string? productNameAr = null,
        string? productSku = null,
        string? branchNameAr = null,
        string? priceListNameAr = null,
        string? unitNameAr = null,
        decimal unitFactor = 1m) =>
        new(
            e.Id,
            e.TenantId,
            e.ProductId,
            productNameAr,
            productSku,
            e.BranchId,
            branchNameAr,
            e.PriceListId,
            priceListNameAr,
            e.SalesChannel,
            e.UnitId,
            unitNameAr,
            unitFactor,
            e.PricingMethod,
            e.CostType,
            e.Cost,
            e.ProfitMargin,
            e.ProfitAmount,
            e.SellingPrice,
            e.MinimumPrice,
            e.MaximumDiscount,
            e.StartDate,
            e.EndDate,
            e.Priority,
            e.CurrencyId,
            e.IsDefault,
            e.IsActive,
            e.Notes,
            e.CreatedAt.UtcDateTime,
            e.UpdatedAt?.UtcDateTime);
}

internal static class ProductPricingRules
{
    public static async Task<Result> EnsureNoOverlapAsync(
        IApplicationDbContext db,
        Guid tenantId,
        Guid productId,
        Guid unitId,
        Guid? branchId,
        Guid priceListId,
        SalesChannel salesChannel,
        DateTimeOffset startDate,
        DateTimeOffset? endDate,
        Guid? excludeId,
        CancellationToken ct)
    {
        var query = db.ProductPrices.Where(x =>
            x.TenantId == tenantId
            && x.ProductId == productId
            && x.UnitId == unitId
            && x.PriceListId == priceListId
            && x.SalesChannel == salesChannel
            && x.IsActive);

        query = branchId.HasValue
            ? query.Where(x => x.BranchId == branchId.Value)
            : query.Where(x => x.BranchId == null);

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        var existing = await query
            .Select(x => new { x.Id, x.StartDate, x.EndDate })
            .ToListAsync(ct);

        foreach (var row in existing)
        {
            var thisEnd = endDate ?? DateTimeOffset.MaxValue;
            var thatEnd = row.EndDate ?? DateTimeOffset.MaxValue;
            if (startDate <= thatEnd && row.StartDate <= thisEnd)
            {
                return Result.Failure(
                    "PricePeriodOverlap",
                    "A price already exists for this product, unit, branch, and price list within the same period.");
            }
        }

        return Result.Success();
    }

    public static async Task<Result> EnsureProductActiveAsync(
        IApplicationDbContext db,
        Guid tenantId,
        Guid productId,
        CancellationToken ct)
    {
        var item = await db.InventoryItems.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == productId && x.TenantId == tenantId, ct);
        if (item is null)
            return Result.Failure("ProductNotFound", "Product (inventory item) not found.");
        if (!item.IsActive)
            return Result.Failure("ProductInactive", "Product must be active before pricing.");
        return Result.Success();
    }

    public static async Task<decimal> ResolveReferenceCostAsync(
        IApplicationDbContext db,
        Guid tenantId,
        Guid productId,
        ProductCostType costType,
        CancellationToken ct)
    {
        if (costType == ProductCostType.Average)
        {
            var avg = await db.InventoryBalances.AsNoTracking()
                .Where(b => b.TenantId == tenantId && b.InventoryItemId == productId && b.QtyOnHand > 0)
                .Select(b => (decimal?)b.AvgCost)
                .AverageAsync(ct);
            if (avg.HasValue && avg.Value > 0)
                return decimal.Round(avg.Value, 4, MidpointRounding.AwayFromZero);
        }

        if (costType == ProductCostType.LastPurchase)
        {
            var last = await db.StockMovements.AsNoTracking()
                .Where(m => m.TenantId == tenantId
                            && m.InventoryItemId == productId
                            && m.UnitCost > 0)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => (decimal?)m.UnitCost)
                .FirstOrDefaultAsync(ct);
            if (last.HasValue)
                return decimal.Round(last.Value, 4, MidpointRounding.AwayFromZero);
        }

        // Standard / fallback: latest average or zero
        var fallback = await db.InventoryBalances.AsNoTracking()
            .Where(b => b.TenantId == tenantId && b.InventoryItemId == productId)
            .OrderByDescending(b => b.UpdatedAt)
            .Select(b => (decimal?)b.AvgCost)
            .FirstOrDefaultAsync(ct);

        return decimal.Round(fallback ?? 0m, 4, MidpointRounding.AwayFromZero);
    }
}

public sealed record CreateSalesPriceListCommand(CreateSalesPriceListRequest Request)
    : IRequest<Result<SalesPriceListDto>>;

public sealed class CreateSalesPriceListCommandHandler(
    IApplicationDbContext db,
    ILogger<CreateSalesPriceListCommandHandler> logger)
    : IRequestHandler<CreateSalesPriceListCommand, Result<SalesPriceListDto>>
{
    public async Task<Result<SalesPriceListDto>> Handle(
        CreateSalesPriceListCommand command,
        CancellationToken cancellationToken)
    {
        var r = command.Request;
        var code = r.Code.Trim().ToUpperInvariant();

        if (await db.SalesPriceLists.AnyAsync(x => x.TenantId == r.TenantId && x.Code == code, cancellationToken))
            return Result<SalesPriceListDto>.Failure("DuplicatePriceListCode", "Price list code must be unique.");

        try
        {
            if (r.IsDefault)
            {
                var defaults = await db.SalesPriceLists
                    .Where(x => x.TenantId == r.TenantId && x.IsDefault)
                    .ToListAsync(cancellationToken);
                foreach (var d in defaults)
                    d.Update(d.NameAr, d.NameEn, d.Description, d.DefaultSalesChannel, d.SortOrder, false);
            }

            var entity = new SalesPriceList(
                r.TenantId, code, r.NameAr, r.NameEn, r.Description,
                r.DefaultSalesChannel, r.SortOrder, r.IsDefault);

            db.SalesPriceLists.Add(entity);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Sales price list created: {Id} ({Code})", entity.Id, entity.Code);
            return Result<SalesPriceListDto>.Success(ProductPricingMapper.ToDto(entity));
        }
        catch (Exception ex) when (ex is ArgumentException or Domain.Common.Exceptions.BusinessException)
        {
            return Result<SalesPriceListDto>.Failure("ValidationFailed", ex.Message);
        }
    }
}

public sealed record UpdateSalesPriceListCommand(Guid Id, UpdateSalesPriceListRequest Request) : IRequest<Result>;

public sealed class UpdateSalesPriceListCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateSalesPriceListCommand, Result>
{
    public async Task<Result> Handle(UpdateSalesPriceListCommand command, CancellationToken cancellationToken)
    {
        var r = command.Request;
        var entity = await db.SalesPriceLists
            .FirstOrDefaultAsync(x => x.Id == command.Id && x.TenantId == r.TenantId, cancellationToken);
        if (entity is null)
            return Result.Failure("PriceListNotFound", "Price list not found.");

        try
        {
            if (r.IsDefault && !entity.IsDefault)
            {
                var defaults = await db.SalesPriceLists
                    .Where(x => x.TenantId == r.TenantId && x.IsDefault && x.Id != entity.Id)
                    .ToListAsync(cancellationToken);
                foreach (var d in defaults)
                    d.Update(d.NameAr, d.NameEn, d.Description, d.DefaultSalesChannel, d.SortOrder, false);
            }

            entity.Update(r.NameAr, r.NameEn, r.Description, r.DefaultSalesChannel, r.SortOrder, r.IsDefault);
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

public sealed record DeleteSalesPriceListCommand(Guid Id, Guid TenantId) : IRequest<Result>;

public sealed class DeleteSalesPriceListCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteSalesPriceListCommand, Result>
{
    public async Task<Result> Handle(DeleteSalesPriceListCommand command, CancellationToken cancellationToken)
    {
        var entity = await db.SalesPriceLists
            .FirstOrDefaultAsync(x => x.Id == command.Id && x.TenantId == command.TenantId, cancellationToken);
        if (entity is null)
            return Result.Failure("PriceListNotFound", "Price list not found.");

        var hasPrices = await db.ProductPrices.AnyAsync(
            x => x.TenantId == command.TenantId && x.PriceListId == command.Id, cancellationToken);
        if (hasPrices)
            return Result.Failure("PriceListInUse", "Cannot delete a price list that has product prices.");

        try
        {
            entity.SoftDeleteList("system");
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Domain.Common.Exceptions.BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}

public sealed record CreateProductPriceCommand(CreateProductPriceRequest Request)
    : IRequest<Result<ProductPriceDto>>;

public sealed class CreateProductPriceCommandHandler(
    IApplicationDbContext db,
    ILogger<CreateProductPriceCommandHandler> logger)
    : IRequestHandler<CreateProductPriceCommand, Result<ProductPriceDto>>
{
    public async Task<Result<ProductPriceDto>> Handle(
        CreateProductPriceCommand command,
        CancellationToken cancellationToken)
    {
        var r = command.Request;
        var productOk = await ProductPricingRules.EnsureProductActiveAsync(db, r.TenantId, r.ProductId, cancellationToken);
        if (productOk.IsFailure)
            return Result<ProductPriceDto>.Failure(productOk.ErrorCode!, productOk.ErrorMessage);

        var priceListExists = await db.SalesPriceLists.AnyAsync(
            x => x.Id == r.PriceListId && x.TenantId == r.TenantId && x.IsActive, cancellationToken);
        if (!priceListExists)
            return Result<ProductPriceDto>.Failure("PriceListNotFound", "Price list not found or inactive.");

        var unitExists = await db.InventoryUnits.AnyAsync(
            x => x.Id == r.UnitId && x.TenantId == r.TenantId, cancellationToken);
        if (!unitExists)
            return Result<ProductPriceDto>.Failure("UnitNotFound", "Unit not found.");

        if (r.BranchId.HasValue)
        {
            var branchOk = await db.Branches.AnyAsync(
                b => b.Id == r.BranchId.Value && b.TenantId == r.TenantId, cancellationToken);
            if (!branchOk)
                return Result<ProductPriceDto>.Failure("BranchNotFound", "Branch not found.");
        }

        var overlap = await ProductPricingRules.EnsureNoOverlapAsync(
            db, r.TenantId, r.ProductId, r.UnitId, r.BranchId, r.PriceListId, r.SalesChannel,
            r.StartDate, r.EndDate, null, cancellationToken);
        if (overlap.IsFailure)
            return Result<ProductPriceDto>.Failure(overlap.ErrorCode!, overlap.ErrorMessage);

        try
        {
            var entity = new ProductPrice(
                r.TenantId, r.ProductId, r.PriceListId, r.UnitId,
                r.PricingMethod, r.CostType, r.Cost, r.ProfitMargin, r.ProfitAmount, r.SellingPrice,
                r.StartDate, r.BranchId, r.SalesChannel, r.MinimumPrice, r.MaximumDiscount,
                r.EndDate, r.Priority, r.CurrencyId, r.IsDefault, r.Notes);

            db.ProductPrices.Add(entity);
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Product price created: {Id} for product {ProductId}", entity.Id, entity.ProductId);
            return Result<ProductPriceDto>.Success(ProductPricingMapper.ToDto(entity));
        }
        catch (Exception ex) when (ex is ArgumentException or Domain.Common.Exceptions.BusinessException)
        {
            return Result<ProductPriceDto>.Failure("ValidationFailed", ex.Message);
        }
    }
}

public sealed record CreateProductPricesBatchCommand(CreateProductPricesBatchRequest Request)
    : IRequest<Result<IReadOnlyList<ProductPriceDto>>>;

public sealed class CreateProductPricesBatchCommandHandler(
    IApplicationDbContext db,
    ILogger<CreateProductPricesBatchCommandHandler> logger)
    : IRequestHandler<CreateProductPricesBatchCommand, Result<IReadOnlyList<ProductPriceDto>>>
{
    public async Task<Result<IReadOnlyList<ProductPriceDto>>> Handle(
        CreateProductPricesBatchCommand command,
        CancellationToken cancellationToken)
    {
        var r = command.Request;
        var lines = r.Lines.Where(l => l.Save).ToList();
        if (lines.Count == 0)
            return Result<IReadOnlyList<ProductPriceDto>>.Failure("NoPriceLines", "At least one unit price line is required.");

        var productOk = await ProductPricingRules.EnsureProductActiveAsync(db, r.TenantId, r.ProductId, cancellationToken);
        if (productOk.IsFailure)
            return Result<IReadOnlyList<ProductPriceDto>>.Failure(productOk.ErrorCode!, productOk.ErrorMessage);

        var priceListExists = await db.SalesPriceLists.AnyAsync(
            x => x.Id == r.PriceListId && x.TenantId == r.TenantId && x.IsActive, cancellationToken);
        if (!priceListExists)
            return Result<IReadOnlyList<ProductPriceDto>>.Failure("PriceListNotFound", "Price list not found or inactive.");

        var created = new List<ProductPrice>();
        foreach (var line in lines)
        {
            var overlap = await ProductPricingRules.EnsureNoOverlapAsync(
                db, r.TenantId, r.ProductId, line.UnitId, r.BranchId, r.PriceListId, r.SalesChannel,
                r.StartDate, r.EndDate, null, cancellationToken);
            if (overlap.IsFailure)
                return Result<IReadOnlyList<ProductPriceDto>>.Failure(overlap.ErrorCode!, overlap.ErrorMessage);

            try
            {
                var entity = new ProductPrice(
                    r.TenantId, r.ProductId, r.PriceListId, line.UnitId,
                    r.PricingMethod, r.CostType, line.Cost, line.ProfitMargin, line.ProfitAmount, line.SellingPrice,
                    r.StartDate, r.BranchId, r.SalesChannel, line.MinimumPrice, r.MaximumDiscount,
                    r.EndDate, r.Priority, r.CurrencyId, r.IsDefault, r.Notes);
                db.ProductPrices.Add(entity);
                created.Add(entity);
            }
            catch (Exception ex) when (ex is ArgumentException or Domain.Common.Exceptions.BusinessException)
            {
                return Result<IReadOnlyList<ProductPriceDto>>.Failure("ValidationFailed", ex.Message);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Batch product prices created: {Count} for product {ProductId}",
            created.Count, r.ProductId);

        return Result<IReadOnlyList<ProductPriceDto>>.Success(
            created.Select(e => ProductPricingMapper.ToDto(e)).ToList());
    }
}

public sealed record UpdateProductPriceCommand(Guid Id, UpdateProductPriceRequest Request) : IRequest<Result>;

public sealed class UpdateProductPriceCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateProductPriceCommand, Result>
{
    public async Task<Result> Handle(UpdateProductPriceCommand command, CancellationToken cancellationToken)
    {
        var r = command.Request;
        var entity = await db.ProductPrices
            .FirstOrDefaultAsync(x => x.Id == command.Id && x.TenantId == r.TenantId, cancellationToken);
        if (entity is null)
            return Result.Failure("ProductPriceNotFound", "Product price not found.");

        var overlap = await ProductPricingRules.EnsureNoOverlapAsync(
            db, r.TenantId, entity.ProductId, r.UnitId, r.BranchId, r.PriceListId, r.SalesChannel,
            r.StartDate, r.EndDate, command.Id, cancellationToken);
        if (overlap.IsFailure) return overlap;

        try
        {
            entity.Update(
                r.BranchId, r.PriceListId, r.SalesChannel, r.UnitId,
                r.PricingMethod, r.CostType, r.Cost, r.ProfitMargin, r.ProfitAmount, r.SellingPrice,
                r.MinimumPrice, r.MaximumDiscount, r.StartDate, r.EndDate, r.Priority,
                r.CurrencyId, r.IsDefault, r.Notes);

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

public sealed record DeleteProductPriceCommand(Guid Id, Guid TenantId) : IRequest<Result>;

public sealed class DeleteProductPriceCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteProductPriceCommand, Result>
{
    public async Task<Result> Handle(DeleteProductPriceCommand command, CancellationToken cancellationToken)
    {
        var entity = await db.ProductPrices
            .FirstOrDefaultAsync(x => x.Id == command.Id && x.TenantId == command.TenantId, cancellationToken);
        if (entity is null)
            return Result.Failure("ProductPriceNotFound", "Product price not found.");

        entity.SoftDeletePrice("system");
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed record ActivateProductPriceCommand(Guid Id, Guid TenantId, bool IsActive) : IRequest<Result>;

public sealed class ActivateProductPriceCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ActivateProductPriceCommand, Result>
{
    public async Task<Result> Handle(ActivateProductPriceCommand command, CancellationToken cancellationToken)
    {
        var entity = await db.ProductPrices
            .FirstOrDefaultAsync(x => x.Id == command.Id && x.TenantId == command.TenantId, cancellationToken);
        if (entity is null)
            return Result.Failure("ProductPriceNotFound", "Product price not found.");

        if (command.IsActive) entity.Activate();
        else entity.Deactivate();
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed record CopyPriceListCommand(CopyPriceListRequest Request)
    : IRequest<Result<int>>;

public sealed class CopyPriceListCommandHandler(
    IApplicationDbContext db,
    ILogger<CopyPriceListCommandHandler> logger)
    : IRequestHandler<CopyPriceListCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CopyPriceListCommand command, CancellationToken cancellationToken)
    {
        var r = command.Request;
        if (r.SourcePriceListId == r.TargetPriceListId)
            return Result<int>.Failure("SamePriceList", "Source and target price lists must differ.");

        var listsOk = await db.SalesPriceLists.CountAsync(
            x => x.TenantId == r.TenantId
                 && (x.Id == r.SourcePriceListId || x.Id == r.TargetPriceListId),
            cancellationToken);
        if (listsOk < 2)
            return Result<int>.Failure("PriceListNotFound", "Source or target price list not found.");

        var sourcePrices = await db.ProductPrices
            .Where(x => x.TenantId == r.TenantId && x.PriceListId == r.SourcePriceListId && x.IsActive)
            .ToListAsync(cancellationToken);

        var copied = 0;
        var start = r.NewStartDate ?? DateTimeOffset.UtcNow.Date;

        foreach (var src in sourcePrices)
        {
            var branchId = r.BranchId ?? src.BranchId;
            var overlap = await ProductPricingRules.EnsureNoOverlapAsync(
                db, r.TenantId, src.ProductId, src.UnitId, branchId, r.TargetPriceListId, src.SalesChannel,
                start, src.EndDate, null, cancellationToken);
            if (overlap.IsFailure)
                continue;

            var clone = new ProductPrice(
                r.TenantId, src.ProductId, r.TargetPriceListId, src.UnitId,
                src.PricingMethod, src.CostType, src.Cost, src.ProfitMargin, src.ProfitAmount, src.SellingPrice,
                start, branchId, src.SalesChannel, src.MinimumPrice, src.MaximumDiscount,
                src.EndDate, src.Priority, src.CurrencyId, false, src.Notes);
            db.ProductPrices.Add(clone);
            copied++;
        }

        if (r.DeactivateSource)
        {
            foreach (var src in sourcePrices)
                src.Deactivate();
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Copied {Count} prices from {Source} to {Target}",
            copied, r.SourcePriceListId, r.TargetPriceListId);
        return Result<int>.Success(copied);
    }
}
