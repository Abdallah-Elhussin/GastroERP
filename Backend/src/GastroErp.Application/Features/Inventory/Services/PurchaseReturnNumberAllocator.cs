using GastroErp.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Inventory.Services;

/// <summary>
/// Purchase return numbers: <c>PR{yyyyMM}{####}</c> (no separators).
/// </summary>
public static class PurchaseReturnNumberAllocator
{
    public const string TransactionPrefix = "PR";

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
        var existing = await context.PurchaseReturns.IgnoreQueryFilters()
            .AsNoTracking()
            .Where(r => r.TenantId == tenantId && r.ReturnNumber.StartsWith(prefix))
            .Select(r => r.ReturnNumber)
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
