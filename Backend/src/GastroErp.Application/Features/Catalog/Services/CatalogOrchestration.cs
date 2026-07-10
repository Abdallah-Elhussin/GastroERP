using GastroErp.Application.Common.Interfaces;
using GastroErp.Domain.Entities.Catalog;
using GastroErp.Domain.Entities.Menu;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Catalog.Services;

internal static class CatalogOrchestration
{
    public static async Task<Guid> EnsureProductAsync(
        IApplicationDbContext context,
        ProductCatalogDefinition catalog,
        CancellationToken cancellationToken)
    {
        if (catalog.ProductId.HasValue)
        {
            var exists = await context.Products.AnyAsync(p => p.Id == catalog.ProductId.Value, cancellationToken);
            if (exists) return catalog.ProductId.Value;
        }

        var categoryId = catalog.MenuCategoryId
            ?? await ResolveDefaultMenuCategoryIdAsync(context, catalog.TenantId, cancellationToken);

        var product = new Product(
            catalog.TenantId,
            categoryId,
            catalog.NameAr,
            catalog.BasePrice,
            catalog.Currency,
            catalog.NameEn,
            catalog.Sku);

        product.UpdateInfo(
            catalog.NameAr,
            catalog.NameEn,
            catalog.ShortDescriptionAr ?? catalog.LongDescriptionAr,
            catalog.ShortDescriptionEn ?? catalog.LongDescriptionEn,
            catalog.Sku,
            catalog.Barcode,
            null,
            null,
            catalog.PrepTimeMinutes > 0 ? catalog.PrepTimeMinutes : catalog.RecipePreparationTime);

        if (!string.IsNullOrWhiteSpace(catalog.PrimaryImageUrl))
            product.AddImage(catalog.PrimaryImageUrl, isPrimary: true);

        context.Products.Add(product);
        catalog.LinkProduct(product.Id);
        return product.Id;
    }

    public static async Task SyncProductFromCatalogAsync(
        IApplicationDbContext context,
        ProductCatalogDefinition catalog,
        Product product,
        CancellationToken cancellationToken)
    {
        product.UpdateInfo(
            catalog.NameAr,
            catalog.NameEn,
            catalog.ShortDescriptionAr ?? catalog.LongDescriptionAr,
            catalog.ShortDescriptionEn ?? catalog.LongDescriptionEn,
            catalog.Sku,
            catalog.Barcode,
            null,
            null,
            catalog.PrepTimeMinutes);

        if (catalog.IsAvailableOnPos && !product.IsAvailable)
            product.MarkAvailable();
        else if (!catalog.IsAvailableOnPos && product.IsAvailable)
            product.MarkUnavailable("Catalog POS settings");

        if (catalog.IsFeaturedOnPos) product.SetFeatured(true);
        else product.SetFeatured(false);

        if (catalog.MenuCategoryId.HasValue && catalog.MenuCategoryId.Value != product.CategoryId)
        {
            var categoryExists = await context.Categories.AnyAsync(c => c.Id == catalog.MenuCategoryId.Value, cancellationToken);
            if (categoryExists) product.SetCategory(catalog.MenuCategoryId.Value);
        }

        _ = context;
    }

    private static async Task<Guid> ResolveDefaultMenuCategoryIdAsync(
        IApplicationDbContext context,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var categoryId = await context.Categories
            .AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.SortOrder)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (categoryId == Guid.Empty)
            throw new InvalidOperationException("No menu category found. Create a menu category before saving POS/recipe steps.");

        return categoryId;
    }
}
