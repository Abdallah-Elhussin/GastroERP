using GastroErp.Application.Common.Interfaces;
using GastroErp.Domain.Entities.Catalog;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Catalog.Services;

public interface ICatalogCodeGenerator
{
    Task<string> GenerateNextCodeAsync(Guid tenantId, ProductCatalogType catalogType, CancellationToken cancellationToken = default);
}

public sealed class CatalogCodeGenerator(IApplicationDbContext context) : ICatalogCodeGenerator
{
    private static readonly IReadOnlyDictionary<ProductCatalogType, string> PrefixMap =
        new Dictionary<ProductCatalogType, string>
        {
            [ProductCatalogType.RawMaterial] = "RAW",
            [ProductCatalogType.SemiFinished] = "SEM",
            [ProductCatalogType.FinishedProduct] = "PRD",
            [ProductCatalogType.MenuItem] = "MEN",
            [ProductCatalogType.Combo] = "COM",
            [ProductCatalogType.Modifier] = "MOD",
            [ProductCatalogType.Bundle] = "BND",
            [ProductCatalogType.Service] = "SRV",
            [ProductCatalogType.Voucher] = "VCH",
            [ProductCatalogType.GiftCard] = "GFT",
            [ProductCatalogType.Packaging] = "PKG",
            [ProductCatalogType.Asset] = "AST",
            [ProductCatalogType.Expense] = "EXP"
        };

    public async Task<string> GenerateNextCodeAsync(Guid tenantId, ProductCatalogType catalogType, CancellationToken cancellationToken = default)
    {
        var prefix = PrefixMap[catalogType];
        var sequence = await context.CatalogCodeSequences
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Prefix == prefix, cancellationToken);

        if (sequence is null)
        {
            sequence = new CatalogCodeSequence(tenantId, prefix);
            context.CatalogCodeSequences.Add(sequence);
        }

        return sequence.NextCode();
    }
}
