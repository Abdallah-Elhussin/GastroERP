using GastroErp.Application.Common.Interfaces;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Inventory.Services;

/// <summary>
/// Purchase invoice numbers by kind:
/// FromReceipt → <c>PI{yyyyMM}{####}</c>
/// Direct → <c>DPI{yyyyMM}{####}</c>
/// </summary>
public static class PurchaseInvoiceNumberAllocator
{
    public static string PrefixFor(PurchaseInvoiceKind kind)
        => kind == PurchaseInvoiceKind.Direct ? "DPI" : "PI";

    public static string PeriodPrefix(PurchaseInvoiceKind kind, DateTimeOffset? at = null)
    {
        var stamp = (at ?? DateTimeOffset.UtcNow).ToString("yyyyMM");
        return $"{PrefixFor(kind)}{stamp}";
    }

    public static async Task<string> PeekNextAsync(
        IApplicationDbContext context,
        Guid tenantId,
        PurchaseInvoiceKind kind = PurchaseInvoiceKind.FromReceipt,
        CancellationToken cancellationToken = default)
    {
        var prefix = PeriodPrefix(kind);
        var existing = await context.PurchaseInvoices.IgnoreQueryFilters()
            .AsNoTracking()
            .Where(i => i.TenantId == tenantId
                        && i.Kind == kind
                        && i.InvoiceNumber.StartsWith(prefix))
            .Select(i => i.InvoiceNumber)
            .ToListAsync(cancellationToken);

        var maxSeq = 0;
        foreach (var number in existing)
        {
            if (number.Length <= prefix.Length) continue;
            var suffix = number[prefix.Length..];
            if (int.TryParse(suffix, out var seq) && seq > maxSeq)
                maxSeq = seq;
        }

        // Skip any number already taken (e.g. legacy Direct invoices that used PI…).
        var next = maxSeq + 1;
        while (true)
        {
            var candidate = $"{prefix}{next:D4}";
            var taken = existing.Contains(candidate)
                || await context.PurchaseInvoices.IgnoreQueryFilters()
                    .AsNoTracking()
                    .AnyAsync(i => i.TenantId == tenantId && i.InvoiceNumber == candidate, cancellationToken);
            if (!taken)
                return candidate;
            next++;
        }
    }
}
