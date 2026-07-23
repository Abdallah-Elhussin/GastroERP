using GastroErp.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Inventory.Services;

/// <summary>
/// Goods receipt numbers: <c>GRN{yyyyMM}{####}</c> (no separators).
/// </summary>
public static class GoodsReceiptNumberAllocator
{
    public const string TransactionPrefix = "GRN";

    public static string PeriodPrefix(DateTimeOffset? at = null)
    {
        var stamp = (at ?? DateTimeOffset.UtcNow).ToString("yyyyMM");
        return $"{TransactionPrefix}{stamp}";
    }

    public static async Task<string> PeekNextAsync(
        IApplicationDbContext context,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var prefix = PeriodPrefix();
        var existing = await context.GoodsReceipts.IgnoreQueryFilters()
            .AsNoTracking()
            .Where(g => g.TenantId == tenantId && g.ReceiptNumber.StartsWith(prefix))
            .Select(g => g.ReceiptNumber)
            .ToListAsync(cancellationToken);

        var maxSeq = 0;
        foreach (var number in existing)
        {
            if (number.Length <= prefix.Length) continue;
            var suffix = number[prefix.Length..];
            if (int.TryParse(suffix, out var seq) && seq > maxSeq)
                maxSeq = seq;
        }

        return $"{prefix}{(maxSeq + 1):D4}";
    }
}
