using GastroErp.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Inventory.Services;

/// <summary>
/// Supplier codes: <c>SUP{yyyyMM}{####}</c> (transaction prefix + year/month + sequence, no separators).
/// </summary>
public static class SupplierCodeAllocator
{
    public const string TransactionPrefix = "SUP";

    public static string PeriodPrefix(DateTimeOffset? at = null)
    {
        var stamp = (at ?? DateTimeOffset.UtcNow).ToString("yyyyMM");
        return $"{TransactionPrefix}{stamp}";
    }

    /// <summary>Peeks the next code for the current year/month without persisting.</summary>
    public static async Task<string> PeekNextAsync(
        IApplicationDbContext context,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var prefix = PeriodPrefix();
        var count = await context.Suppliers.IgnoreQueryFilters()
            .CountAsync(s => s.TenantId == tenantId && s.Code.StartsWith(prefix), cancellationToken);
        return $"{prefix}{(count + 1):D4}";
    }
}
