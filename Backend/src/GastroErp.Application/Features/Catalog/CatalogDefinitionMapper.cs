using System.Text.Json;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Features.Catalog.DTOs;
using GastroErp.Domain.Entities.Catalog;
using GastroErp.Domain.Entities.Inventory.Recipe;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Catalog;

internal static class CatalogDefinitionMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static async Task<ProductCatalogDefinitionDto> MapAsync(
        IApplicationDbContext context,
        ProductCatalogDefinition entity,
        CancellationToken cancellationToken)
    {
        var ingredients = new List<CatalogRecipeIngredientDto>();
        if (entity.RecipeId.HasValue)
        {
            var recipe = await context.Recipes.AsNoTracking()
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == entity.RecipeId.Value, cancellationToken);

            if (recipe is not null)
            {
                var itemIds = recipe.Items.Select(i => i.InventoryItemId).Distinct().ToList();
                var unitIds = recipe.Items.Select(i => i.UnitId).Distinct().ToList();
                var itemNames = await context.InventoryItems.AsNoTracking()
                    .Where(i => itemIds.Contains(i.Id))
                    .ToDictionaryAsync(i => i.Id, i => i.NameAr, cancellationToken);
                var unitNames = await context.InventoryUnits.AsNoTracking()
                    .Where(u => unitIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => u.NameAr, cancellationToken);

                ingredients = recipe.Items.Select(i => new CatalogRecipeIngredientDto(
                    i.InventoryItemId,
                    itemNames.GetValueOrDefault(i.InventoryItemId),
                    i.UnitId,
                    unitNames.GetValueOrDefault(i.UnitId),
                    i.Quantity,
                    i.WastePercentage)).ToList();
            }
        }

        var priceLevels = ParsePriceLevels(entity.PriceLevelsJson);
        var supplierIds = ParseGuids(entity.SupplierIdsJson);
        var mediaUrls = ParseStrings(entity.MediaUrlsJson);
        var related = ParseRelationships(entity.RelatedProductsJson);

        return new ProductCatalogDefinitionDto(
            entity.Id, entity.TenantId, entity.CatalogType, entity.Code, entity.Sku, entity.Barcode,
            entity.NameAr, entity.NameEn, entity.ShortDescriptionAr, entity.ShortDescriptionEn,
            entity.LongDescriptionAr, entity.LongDescriptionEn, entity.Keywords, entity.Brand, entity.TagsJson,
            entity.PrimaryImageUrl, entity.Status, entity.WizardStepCompleted,
            entity.MenuCategoryId, entity.InventoryCategoryId, entity.InventoryItemId, entity.ProductId, entity.RecipeId,
            entity.BaseUnitId, entity.DefaultPurchaseUnitId, entity.DefaultRecipeUnitId,
            entity.MinStock, entity.MaxStock, entity.SafetyStock, entity.ReorderLevel, entity.ReorderQuantity,
            entity.CostingMethod, entity.TrackBatch, entity.TrackSerial, entity.TrackExpiry, entity.AllowNegativeStock,
            entity.RecipeYield, entity.RecipeWastePercentage, entity.RecipePreparationTime, entity.RecipeInstructions,
            ingredients, entity.PrepTimeMinutes, entity.IsAvailableOnPos, entity.IsFeaturedOnPos, entity.KitchenStationId,
            entity.BasePrice, entity.Currency, priceLevels, supplierIds, mediaUrls, entity.VariantAttributesJson,
            related, entity.CreatedAt.UtcDateTime, entity.UpdatedAt?.UtcDateTime);
    }

    public static ProductCatalogDefinitionDto MapSync(ProductCatalogDefinition entity) =>
        new(
            entity.Id, entity.TenantId, entity.CatalogType, entity.Code, entity.Sku, entity.Barcode,
            entity.NameAr, entity.NameEn, entity.ShortDescriptionAr, entity.ShortDescriptionEn,
            entity.LongDescriptionAr, entity.LongDescriptionEn, entity.Keywords, entity.Brand, entity.TagsJson,
            entity.PrimaryImageUrl, entity.Status, entity.WizardStepCompleted,
            entity.MenuCategoryId, entity.InventoryCategoryId, entity.InventoryItemId, entity.ProductId, entity.RecipeId,
            entity.BaseUnitId, entity.DefaultPurchaseUnitId, entity.DefaultRecipeUnitId,
            entity.MinStock, entity.MaxStock, entity.SafetyStock, entity.ReorderLevel, entity.ReorderQuantity,
            entity.CostingMethod, entity.TrackBatch, entity.TrackSerial, entity.TrackExpiry, entity.AllowNegativeStock,
            entity.RecipeYield, entity.RecipeWastePercentage, entity.RecipePreparationTime, entity.RecipeInstructions,
            [], entity.PrepTimeMinutes, entity.IsAvailableOnPos, entity.IsFeaturedOnPos, entity.KitchenStationId,
            entity.BasePrice, entity.Currency, ParsePriceLevels(entity.PriceLevelsJson), ParseGuids(entity.SupplierIdsJson),
            ParseStrings(entity.MediaUrlsJson), entity.VariantAttributesJson, ParseRelationships(entity.RelatedProductsJson),
            entity.CreatedAt.UtcDateTime, entity.UpdatedAt?.UtcDateTime);

    public static string SerializePriceLevels(IReadOnlyList<CatalogPriceLevelLineDto> levels) =>
        JsonSerializer.Serialize(levels, JsonOptions);

    public static string SerializeGuids(IReadOnlyList<Guid> ids) =>
        JsonSerializer.Serialize(ids, JsonOptions);

    public static string SerializeStrings(IReadOnlyList<string> values) =>
        JsonSerializer.Serialize(values, JsonOptions);

    public static string SerializeRelationships(IReadOnlyList<CatalogRelationshipDto> relationships) =>
        JsonSerializer.Serialize(relationships, JsonOptions);

    private static IReadOnlyList<CatalogPriceLevelLineDto> ParsePriceLevels(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<CatalogPriceLevelLineDto>>(json, JsonOptions) ?? []; }
        catch { return []; }
    }

    private static IReadOnlyList<Guid> ParseGuids(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<Guid>>(json, JsonOptions) ?? []; }
        catch { return []; }
    }

    private static IReadOnlyList<string> ParseStrings(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? []; }
        catch { return []; }
    }

    private static IReadOnlyList<CatalogRelationshipDto> ParseRelationships(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<CatalogRelationshipDto>>(json, JsonOptions) ?? []; }
        catch { return []; }
    }
}
