using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Catalog.DTOs;
using GastroErp.Application.Features.Catalog.Services;
using GastroErp.Domain.Entities.Catalog;
using GastroErp.Domain.Entities.Inventory.Recipe;
using GastroErp.Domain.Entities.Menu;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Catalog.Commands;

public class SaveCatalogRecipeCommandHandler(
    IApplicationDbContext context,
    ILogger<SaveCatalogRecipeCommandHandler> logger) : IRequestHandler<SaveCatalogRecipeCommand, Result<ProductCatalogDefinitionDto>>
{
    public async Task<Result<ProductCatalogDefinitionDto>> Handle(SaveCatalogRecipeCommand request, CancellationToken cancellationToken)
    {
        var catalog = await context.ProductCatalogDefinitions
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (catalog is null)
            return Result<ProductCatalogDefinitionDto>.Failure("CatalogNotFound", "Product catalog definition not found.");

        var typeMeta = ProductCatalogTypeRegistry.Describe(catalog.CatalogType);
        if (!typeMeta.RequiresRecipe)
            return Result<ProductCatalogDefinitionDto>.Failure("RecipeNotRequired", "This catalog type does not require a recipe.");

        var productId = await CatalogOrchestration.EnsureProductAsync(context, catalog, cancellationToken);

        catalog.UpdateRecipeInfo(
            request.Dto.Yield,
            request.Dto.WastePercentage,
            request.Dto.PreparationTime,
            request.Dto.Instructions);

        Recipe recipe;
        if (catalog.RecipeId.HasValue)
        {
            var existing = await context.Recipes.Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == catalog.RecipeId.Value, cancellationToken);
            if (existing is null)
            {
                recipe = new Recipe(catalog.TenantId, productId, catalog.NameAr, catalog.NameEn, request.Dto.Yield,
                    request.Dto.WastePercentage, request.Dto.PreparationTime);
                recipe.UpdateInstructions(request.Dto.Instructions);
                context.Recipes.Add(recipe);
                catalog.LinkRecipe(recipe.Id);
            }
            else
            {
                recipe = existing;
                recipe.CreateNewVersion(request.Dto.Yield, request.Dto.WastePercentage, request.Dto.PreparationTime);
                recipe.UpdateInstructions(request.Dto.Instructions);
            }
        }
        else
        {
            recipe = new Recipe(catalog.TenantId, productId, catalog.NameAr, catalog.NameEn, request.Dto.Yield,
                request.Dto.WastePercentage, request.Dto.PreparationTime);
            recipe.UpdateInstructions(request.Dto.Instructions);
            context.Recipes.Add(recipe);
            catalog.LinkRecipe(recipe.Id);
        }

        foreach (var existing in recipe.Items.ToList())
            recipe.RemoveItem(existing.InventoryItemId);

        foreach (var ingredient in request.Dto.Ingredients)
        {
            var itemExists = await context.InventoryItems.AnyAsync(i => i.Id == ingredient.InventoryItemId, cancellationToken);
            var unitExists = await context.InventoryUnits.AnyAsync(u => u.Id == ingredient.UnitId, cancellationToken);
            if (!itemExists || !unitExists) continue;
            recipe.AddItem(ingredient.InventoryItemId, ingredient.UnitId, ingredient.Quantity, ingredient.WastePercentage);
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Catalog recipe saved: {CatalogId} → Recipe {RecipeId}", catalog.Id, catalog.RecipeId);
        return Result<ProductCatalogDefinitionDto>.Success(await CatalogDefinitionMapper.MapAsync(context, catalog, cancellationToken));
    }
}

public class SaveCatalogPosCommandHandler(
    IApplicationDbContext context,
    ILogger<SaveCatalogPosCommandHandler> logger) : IRequestHandler<SaveCatalogPosCommand, Result<ProductCatalogDefinitionDto>>
{
    public async Task<Result<ProductCatalogDefinitionDto>> Handle(SaveCatalogPosCommand request, CancellationToken cancellationToken)
    {
        var catalog = await context.ProductCatalogDefinitions
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (catalog is null)
            return Result<ProductCatalogDefinitionDto>.Failure("CatalogNotFound", "Product catalog definition not found.");

        var typeMeta = ProductCatalogTypeRegistry.Describe(catalog.CatalogType);
        if (!typeMeta.RequiresProduct)
            return Result<ProductCatalogDefinitionDto>.Failure("ProductNotRequired", "This catalog type does not require POS product.");

        var categoryExists = await context.Categories.AnyAsync(c => c.Id == request.Dto.MenuCategoryId, cancellationToken);
        if (!categoryExists)
            return Result<ProductCatalogDefinitionDto>.Failure("CategoryNotFound", "Menu category not found.");

        if (request.Dto.KitchenStationId.HasValue)
        {
            var stationExists = await context.KitchenStations.AnyAsync(k => k.Id == request.Dto.KitchenStationId.Value, cancellationToken);
            if (!stationExists)
                return Result<ProductCatalogDefinitionDto>.Failure("KitchenStationNotFound", "Kitchen station not found.");
        }

        catalog.UpdatePosInfo(
            request.Dto.PrepTimeMinutes,
            request.Dto.IsAvailableOnPos,
            request.Dto.IsFeaturedOnPos,
            request.Dto.KitchenStationId,
            request.Dto.MenuCategoryId);

        var productId = await CatalogOrchestration.EnsureProductAsync(context, catalog, cancellationToken);
        var product = await context.Products.FirstAsync(p => p.Id == productId, cancellationToken);
        await CatalogOrchestration.SyncProductFromCatalogAsync(context, catalog, product, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Catalog POS saved: {CatalogId} → Product {ProductId}", catalog.Id, catalog.ProductId);
        return Result<ProductCatalogDefinitionDto>.Success(await CatalogDefinitionMapper.MapAsync(context, catalog, cancellationToken));
    }
}

public class SaveCatalogPricingCommandHandler(
    IApplicationDbContext context,
    ILogger<SaveCatalogPricingCommandHandler> logger) : IRequestHandler<SaveCatalogPricingCommand, Result<ProductCatalogDefinitionDto>>
{
    public async Task<Result<ProductCatalogDefinitionDto>> Handle(SaveCatalogPricingCommand request, CancellationToken cancellationToken)
    {
        var catalog = await context.ProductCatalogDefinitions
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (catalog is null)
            return Result<ProductCatalogDefinitionDto>.Failure("CatalogNotFound", "Product catalog definition not found.");

        var typeMeta = ProductCatalogTypeRegistry.Describe(catalog.CatalogType);
        if (!typeMeta.RequiresPricing)
            return Result<ProductCatalogDefinitionDto>.Failure("PricingNotRequired", "This catalog type does not require pricing.");

        var oldBasePrice = catalog.BasePrice;
        catalog.UpdatePricingInfo(request.Dto.BasePrice, request.Dto.Currency,
            CatalogDefinitionMapper.SerializePriceLevels(request.Dto.PriceLevels));

        if (typeMeta.RequiresProduct)
        {
            var productId = await CatalogOrchestration.EnsureProductAsync(context, catalog, cancellationToken);
            var product = await context.Products.Include(p => p.PriceLevels)
                .FirstAsync(p => p.Id == productId, cancellationToken);

            if (product.BasePrice != request.Dto.BasePrice)
            {
                context.ProductPriceHistories.Add(new ProductPriceHistory(
                    catalog.TenantId, catalog.Id, product.Id, oldBasePrice, request.Dto.BasePrice, request.Dto.Currency));
                product.UpdatePrice(request.Dto.BasePrice);
            }

            foreach (var level in request.Dto.PriceLevels)
            {
                var priceLevel = await context.PriceLevels.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == level.PriceLevelId, cancellationToken);
                if (priceLevel is null) continue;

                var existing = product.PriceLevels.FirstOrDefault(p => p.PriceLevelId == level.PriceLevelId);
                var oldPrice = existing?.Price ?? product.BasePrice;
                if (oldPrice != level.Price)
                {
                    context.ProductPriceHistories.Add(new ProductPriceHistory(
                        catalog.TenantId, catalog.Id, product.Id, oldPrice, level.Price, request.Dto.Currency,
                        level.PriceLevelId, level.PriceLevelName ?? priceLevel.NameAr));
                }
                product.SetPriceLevel(level.PriceLevelId, level.Price);
            }
        }
        else if (oldBasePrice != request.Dto.BasePrice)
        {
            context.ProductPriceHistories.Add(new ProductPriceHistory(
                catalog.TenantId, catalog.Id, null, oldBasePrice, request.Dto.BasePrice, request.Dto.Currency));
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Catalog pricing saved: {CatalogId}", catalog.Id);
        return Result<ProductCatalogDefinitionDto>.Success(await CatalogDefinitionMapper.MapAsync(context, catalog, cancellationToken));
    }
}

public class SaveCatalogExtensionsCommandHandler(
    IApplicationDbContext context,
    ILogger<SaveCatalogExtensionsCommandHandler> logger) : IRequestHandler<SaveCatalogExtensionsCommand, Result<ProductCatalogDefinitionDto>>
{
    public async Task<Result<ProductCatalogDefinitionDto>> Handle(SaveCatalogExtensionsCommand request, CancellationToken cancellationToken)
    {
        var catalog = await context.ProductCatalogDefinitions
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (catalog is null)
            return Result<ProductCatalogDefinitionDto>.Failure("CatalogNotFound", "Product catalog definition not found.");

        catalog.UpdateExtensions(
            CatalogDefinitionMapper.SerializeGuids(request.Dto.SupplierIds),
            CatalogDefinitionMapper.SerializeStrings(request.Dto.MediaUrls),
            request.Dto.VariantAttributesJson);

        if (catalog.ProductId.HasValue && request.Dto.MediaUrls.Count > 0)
        {
            var product = await context.Products.Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == catalog.ProductId.Value, cancellationToken);
            if (product is not null)
            {
                foreach (var url in request.Dto.MediaUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
                {
                    if (product.Images.All(i => i.ImageUrl != url))
                        product.AddImage(url!, isPrimary: product.Images.Count == 0);
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Catalog extensions saved: {CatalogId}", catalog.Id);
        return Result<ProductCatalogDefinitionDto>.Success(await CatalogDefinitionMapper.MapAsync(context, catalog, cancellationToken));
    }
}

public class SaveCatalogRelationshipsCommandHandler(
    IApplicationDbContext context,
    ILogger<SaveCatalogRelationshipsCommandHandler> logger) : IRequestHandler<SaveCatalogRelationshipsCommand, Result<ProductCatalogDefinitionDto>>
{
    public async Task<Result<ProductCatalogDefinitionDto>> Handle(SaveCatalogRelationshipsCommand request, CancellationToken cancellationToken)
    {
        var catalog = await context.ProductCatalogDefinitions
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (catalog is null)
            return Result<ProductCatalogDefinitionDto>.Failure("CatalogNotFound", "Product catalog definition not found.");

        catalog.UpdateRelationships(CatalogDefinitionMapper.SerializeRelationships(request.Dto.RelatedProducts));
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Catalog relationships saved: {CatalogId}", catalog.Id);
        return Result<ProductCatalogDefinitionDto>.Success(await CatalogDefinitionMapper.MapAsync(context, catalog, cancellationToken));
    }
}

public class ActivateCatalogDefinitionCommandHandler(
    IApplicationDbContext context,
    ILogger<ActivateCatalogDefinitionCommandHandler> logger) : IRequestHandler<ActivateCatalogDefinitionCommand, Result<ProductCatalogDefinitionDto>>
{
    public async Task<Result<ProductCatalogDefinitionDto>> Handle(ActivateCatalogDefinitionCommand request, CancellationToken cancellationToken)
    {
        var catalog = await context.ProductCatalogDefinitions
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (catalog is null)
            return Result<ProductCatalogDefinitionDto>.Failure("CatalogNotFound", "Product catalog definition not found.");

        catalog.Activate();
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Catalog activated: {CatalogId}", catalog.Id);
        return Result<ProductCatalogDefinitionDto>.Success(await CatalogDefinitionMapper.MapAsync(context, catalog, cancellationToken));
    }
}

public class ImportCatalogDefinitionsCommandHandler(
    IApplicationDbContext context,
    ICatalogCodeGenerator codeGenerator,
    ILogger<ImportCatalogDefinitionsCommandHandler> logger) : IRequestHandler<ImportCatalogDefinitionsCommand, Result<int>>
{
    public async Task<Result<int>> Handle(ImportCatalogDefinitionsCommand request, CancellationToken cancellationToken)
    {
        var imported = 0;
        foreach (var row in request.Rows)
        {
            if (string.IsNullOrWhiteSpace(row.NameAr)) continue;
            var code = await codeGenerator.GenerateNextCodeAsync(request.TenantId, row.CatalogType, cancellationToken);
            var entity = new ProductCatalogDefinition(request.TenantId, row.CatalogType, code, row.NameAr);
            entity.UpdateGeneralInfo(row.NameAr, row.NameEn, null, null, null, null, null, null, null, row.Sku, row.Barcode, null, null, null);
            if (row.BasePrice > 0)
                entity.UpdatePricingInfo(row.BasePrice, "SAR", null);
            context.ProductCatalogDefinitions.Add(entity);
            imported++;
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Catalog import completed: {Count} rows", imported);
        return Result<int>.Success(imported);
    }
}
