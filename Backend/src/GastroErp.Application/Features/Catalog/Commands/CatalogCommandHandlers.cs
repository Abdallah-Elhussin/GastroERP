using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Catalog;
using GastroErp.Application.Features.Catalog.DTOs;
using GastroErp.Application.Features.Catalog.Services;
using GastroErp.Domain.Entities.Catalog;
using GastroErp.Domain.Entities.Inventory.Catalog;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Catalog.Commands;

public class CreateCatalogDraftCommandHandler(
    IApplicationDbContext context,
    ICatalogCodeGenerator codeGenerator,
    ILogger<CreateCatalogDraftCommandHandler> logger) : IRequestHandler<CreateCatalogDraftCommand, Result<ProductCatalogDefinitionDto>>
{
    public async Task<Result<ProductCatalogDefinitionDto>> Handle(CreateCatalogDraftCommand request, CancellationToken cancellationToken)
    {
        var code = await codeGenerator.GenerateNextCodeAsync(request.TenantId, request.Dto.CatalogType, cancellationToken);
        var entity = new ProductCatalogDefinition(request.TenantId, request.Dto.CatalogType, code, request.Dto.NameAr);
        if (!string.IsNullOrWhiteSpace(request.Dto.NameEn))
            entity.UpdateGeneralInfo(request.Dto.NameAr, request.Dto.NameEn, null, null, null, null, null, null, null, null, null, null, null, null);

        context.ProductCatalogDefinitions.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Catalog draft created: {Id} {Code}", entity.Id, entity.Code);
        return Result<ProductCatalogDefinitionDto>.Success(CatalogDefinitionMapper.MapSync(entity));
    }

    internal static ProductCatalogDefinitionDto Map(ProductCatalogDefinition e) => CatalogDefinitionMapper.MapSync(e);
}

public class UpdateCatalogGeneralInfoCommandHandler(
    IApplicationDbContext context,
    ILogger<UpdateCatalogGeneralInfoCommandHandler> logger) : IRequestHandler<UpdateCatalogGeneralInfoCommand, Result<ProductCatalogDefinitionDto>>
{
    public async Task<Result<ProductCatalogDefinitionDto>> Handle(UpdateCatalogGeneralInfoCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.ProductCatalogDefinitions.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (entity is null) return Result<ProductCatalogDefinitionDto>.Failure("CatalogNotFound", "Product catalog definition not found.");

        entity.UpdateGeneralInfo(
            request.Dto.NameAr, request.Dto.NameEn,
            request.Dto.ShortDescriptionAr, request.Dto.ShortDescriptionEn,
            request.Dto.LongDescriptionAr, request.Dto.LongDescriptionEn,
            request.Dto.Keywords, request.Dto.Brand, request.Dto.TagsJson,
            request.Dto.Sku, request.Dto.Barcode, request.Dto.PrimaryImageUrl,
            request.Dto.MenuCategoryId, request.Dto.InventoryCategoryId);

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Catalog general info updated: {Id}", entity.Id);
        return Result<ProductCatalogDefinitionDto>.Success(CreateCatalogDraftCommandHandler.Map(entity));
    }
}

public class SaveCatalogInventoryCommandHandler(
    IApplicationDbContext context,
    ILogger<SaveCatalogInventoryCommandHandler> logger) : IRequestHandler<SaveCatalogInventoryCommand, Result<ProductCatalogDefinitionDto>>
{
    public async Task<Result<ProductCatalogDefinitionDto>> Handle(SaveCatalogInventoryCommand request, CancellationToken cancellationToken)
    {
        var catalog = await context.ProductCatalogDefinitions
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (catalog is null)
            return Result<ProductCatalogDefinitionDto>.Failure("CatalogNotFound", "Product catalog definition not found.");

        var typeMeta = ProductCatalogTypeRegistry.Describe(catalog.CatalogType);
        if (!typeMeta.RequiresInventory)
            return Result<ProductCatalogDefinitionDto>.Failure("InventoryNotRequired", "This catalog type does not require inventory.");

        if (!catalog.InventoryCategoryId.HasValue)
            return Result<ProductCatalogDefinitionDto>.Failure("CategoryRequired", "Inventory category is required before saving inventory.");

        var categoryExists = await context.InventoryCategories
            .AnyAsync(c => c.Id == catalog.InventoryCategoryId.Value, cancellationToken);
        if (!categoryExists)
            return Result<ProductCatalogDefinitionDto>.Failure("CategoryNotFound", "Inventory category not found.");

        var unitExists = await context.InventoryUnits
            .AnyAsync(u => u.Id == request.Dto.BaseUnitId, cancellationToken);
        if (!unitExists)
            return Result<ProductCatalogDefinitionDto>.Failure("UnitNotFound", "Base unit not found.");

        catalog.UpdateInventoryInfo(
            request.Dto.BaseUnitId,
            request.Dto.DefaultPurchaseUnitId,
            request.Dto.DefaultRecipeUnitId,
            request.Dto.MinStock,
            request.Dto.MaxStock,
            request.Dto.SafetyStock,
            request.Dto.ReorderLevel,
            request.Dto.ReorderQuantity,
            request.Dto.CostingMethod,
            request.Dto.TrackBatch,
            request.Dto.TrackSerial,
            request.Dto.TrackExpiry,
            request.Dto.AllowNegativeStock);

        if (catalog.InventoryItemId.HasValue)
        {
            var existing = await context.InventoryItems
                .FirstOrDefaultAsync(i => i.Id == catalog.InventoryItemId.Value, cancellationToken);
            if (existing is null)
                return Result<ProductCatalogDefinitionDto>.Failure("ItemNotFound", "Linked inventory item not found.");

            existing.UpdateInfo(catalog.NameAr, catalog.NameEn, catalog.LongDescriptionAr, catalog.LongDescriptionEn,
                catalog.Sku, catalog.Barcode, catalog.ResolveInventoryItemKind(), catalog.PrimaryImageUrl);
            existing.SetCategory(catalog.InventoryCategoryId.Value);
            existing.SetBaseUnit(request.Dto.BaseUnitId);
            existing.SetUnits(request.Dto.DefaultPurchaseUnitId, request.Dto.DefaultRecipeUnitId);
            existing.SetReorderInfo(request.Dto.ReorderLevel, request.Dto.ReorderQuantity);
        }
        else
        {
            var item = new InventoryItem(
                catalog.TenantId,
                catalog.InventoryCategoryId.Value,
                catalog.NameAr,
                request.Dto.BaseUnitId,
                catalog.NameEn,
                catalog.Sku,
                catalog.Barcode,
                catalog.ResolveInventoryItemKind(),
                catalog.PrimaryImageUrl);

            if (!string.IsNullOrWhiteSpace(catalog.LongDescriptionAr) || !string.IsNullOrWhiteSpace(catalog.LongDescriptionEn))
                item.UpdateInfo(catalog.NameAr, catalog.NameEn, catalog.LongDescriptionAr, catalog.LongDescriptionEn,
                    catalog.Sku, catalog.Barcode, catalog.ResolveInventoryItemKind(), catalog.PrimaryImageUrl);

            item.SetUnits(request.Dto.DefaultPurchaseUnitId, request.Dto.DefaultRecipeUnitId);
            item.SetReorderInfo(request.Dto.ReorderLevel, request.Dto.ReorderQuantity);
            context.InventoryItems.Add(item);
            catalog.LinkInventoryItem(item.Id);
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Catalog inventory saved: {CatalogId} → Item {ItemId}", catalog.Id, catalog.InventoryItemId);
        return Result<ProductCatalogDefinitionDto>.Success(CreateCatalogDraftCommandHandler.Map(catalog));
    }
}
