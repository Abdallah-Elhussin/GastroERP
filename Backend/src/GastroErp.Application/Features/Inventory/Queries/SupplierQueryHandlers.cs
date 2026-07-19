using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Application.Features.Inventory.Services;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Inventory.Queries;

public class GetSuppliersQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSuppliersQuery, PagedResult<SupplierListItemDto>>
{
    public async Task<PagedResult<SupplierListItemDto>> Handle(
        GetSuppliersQuery request, CancellationToken cancellationToken)
    {
        var page = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;

        var query = context.Suppliers.AsNoTracking()
            .Where(s => s.TenantId == request.TenantId);

        if (request.IsActive.HasValue)
            query = query.Where(s => s.IsActive == request.IsActive.Value);
        if (request.IsPreferred.HasValue)
            query = query.Where(s => s.IsPreferred == request.IsPreferred.Value);
        if (request.IsBlacklisted.HasValue)
            query = query.Where(s => s.IsBlacklisted == request.IsBlacklisted.Value);
        if (request.Category.HasValue)
            query = query.Where(s => s.Category == request.Category.Value);
        if (!string.IsNullOrWhiteSpace(request.City))
            query = query.Where(s => s.City != null && s.City.Contains(request.City.Trim()));
        if (!string.IsNullOrWhiteSpace(request.Country))
            query = query.Where(s => s.Country != null && s.Country.Contains(request.Country.Trim()));
        if (!string.IsNullOrWhiteSpace(request.Code))
            query = query.Where(s => s.Code.Contains(request.Code.Trim()));
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(s =>
                s.NameAr.Contains(term)
                || (s.NameEn != null && s.NameEn.Contains(term))
                || s.Code.Contains(term)
                || (s.TaxNumber != null && s.TaxNumber.Contains(term)));
        }

        var suppliers = await query
            .OrderByDescending(s => s.IsPreferred)
            .ThenBy(s => s.NameAr)
            .ToListAsync(cancellationToken);

        var ids = suppliers.Select(s => s.Id).ToList();
        var accountIds = suppliers.Where(s => s.ApAccountId.HasValue).Select(s => s.ApAccountId!.Value).Distinct().ToList();

        var accounts = await context.ChartOfAccounts.AsNoTracking()
            .Where(a => accountIds.Contains(a.Id))
            .Select(a => new { a.Id, a.AccountNumber })
            .ToDictionaryAsync(a => a.Id, a => a.AccountNumber, cancellationToken);

        var invoiceAgg = await context.PurchaseInvoices.AsNoTracking()
            .Where(i => ids.Contains(i.SupplierId) && i.Status == PurchasingDocumentStatus.Posted)
            .GroupBy(i => i.SupplierId)
            .Select(g => new
            {
                SupplierId = g.Key,
                Outstanding = g.Sum(i => i.TotalAmount - i.PaidAmount),
                LastPurchase = g.Max(i => (DateOnly?)i.InvoiceDate)
            })
            .ToListAsync(cancellationToken);
        var invMap = invoiceAgg.ToDictionary(x => x.SupplierId);

        var items = new List<SupplierListItemDto>();
        foreach (var s in suppliers)
        {
            invMap.TryGetValue(s.Id, out var inv);
            var balance = s.OpeningBalance + (inv?.Outstanding ?? 0);
            if (request.HasBalance == true && balance <= 0) continue;
            if (request.HasBalance == false && balance > 0) continue;
            if (request.OverCreditLimit == true && !s.IsOverCreditLimit(balance)) continue;
            if (request.OverCreditLimit == false && s.IsOverCreditLimit(balance)) continue;

            string? acct = null;
            if (s.ApAccountId.HasValue) accounts.TryGetValue(s.ApAccountId.Value, out acct);

            items.Add(SupplierMapper.ToListItem(s, acct, balance, inv?.LastPurchase, null));
        }

        var total = items.Count;
        var pageItems = items.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return PagedResult<SupplierListItemDto>.Success(pageItems, page, pageSize, total);
    }
}

public class GetSupplierByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSupplierByIdQuery, Result<SupplierDto>>
{
    public async Task<Result<SupplierDto>> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
    {
        var supplier = await context.Suppliers.AsNoTracking()
            .Include(s => s.Contacts)
            .Include(s => s.PaymentMethods)
            .Include(s => s.Attachments)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (supplier is null)
            return Result<SupplierDto>.Failure("SupplierNotFound", "Supplier not found.");

        string? accountNumber = null;
        if (supplier.ApAccountId.HasValue)
        {
            accountNumber = await context.ChartOfAccounts.AsNoTracking()
                .Where(a => a.Id == supplier.ApAccountId.Value)
                .Select(a => a.AccountNumber)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var balance = await SupplierMapper.ComputeCurrentBalanceAsync(
            context.PurchaseInvoices.AsNoTracking(), supplier.Id, supplier.OpeningBalance, cancellationToken);

        SupplierDashboardDto? dashboard = null;
        SupplierStatsDto? stats = null;
        if (request.IncludeStats)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var startMonth = new DateOnly(today.Year, today.Month, 1);
            var startYear = new DateOnly(today.Year, 1, 1);
            var startLastYear = new DateOnly(today.Year - 1, 1, 1);
            var endLastYear = new DateOnly(today.Year - 1, 12, 31);

            var poCount = await context.PurchaseOrders.AsNoTracking().CountAsync(p => p.SupplierId == supplier.Id, cancellationToken);
            var grnCount = await context.GoodsReceipts.AsNoTracking().CountAsync(g => g.SupplierId == supplier.Id, cancellationToken);
            var invoices = await context.PurchaseInvoices.AsNoTracking()
                .Where(i => i.SupplierId == supplier.Id)
                .ToListAsync(cancellationToken);
            var posted = invoices.Where(i => i.Status == PurchasingDocumentStatus.Posted).ToList();
            var returnCount = await context.PurchaseReturns.AsNoTracking().CountAsync(r => r.SupplierId == supplier.Id, cancellationToken);

            var unpaidCount = posted.Count(i => i.RemainingAmount > 0);
            var totalPurchases = posted.Sum(i => i.TotalAmount);
            var thisMonth = posted.Where(i => i.InvoiceDate >= startMonth).Sum(i => i.TotalAmount);
            var thisYear = posted.Where(i => i.InvoiceDate >= startYear).Sum(i => i.TotalAmount);
            var lastYear = posted.Where(i => i.InvoiceDate >= startLastYear && i.InvoiceDate <= endLastYear).Sum(i => i.TotalAmount);
            var totalTax = posted.Sum(i => i.TaxAmount);
            var lastPurchase = posted.Count > 0 ? posted.Max(i => i.InvoiceDate) : (DateOnly?)null;

            var warnings = SupplierMapper.BuildWarnings(supplier, balance, today);
            dashboard = new SupplierDashboardDto(
                balance, unpaidCount, poCount, posted.Count, returnCount, 0,
                lastPurchase, null, supplier.IsOverCreditLimit(balance),
                supplier.IsTaxCertificateExpired(today),
                supplier.IsCommercialRegisterExpired(today),
                warnings);

            stats = new SupplierStatsDto(
                poCount, grnCount, posted.Count, returnCount, 0,
                totalPurchases, thisMonth, thisYear, lastYear,
                supplier.PaymentDueDays, 0, totalTax, balance,
                posted.Sum(i => i.RemainingAmount), lastPurchase, null,
                supplier.UpdatedAt ?? supplier.CreatedAt);
        }

        return Result<SupplierDto>.Success(SupplierMapper.ToDto(supplier, accountNumber, balance, dashboard, stats));
    }
}

public class GetSupplierPurchasingDefaultsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSupplierPurchasingDefaultsQuery, Result<SupplierPurchasingDefaultsDto>>
{
    public async Task<Result<SupplierPurchasingDefaultsDto>> Handle(
        GetSupplierPurchasingDefaultsQuery request, CancellationToken cancellationToken)
    {
        var supplier = await context.Suppliers.AsNoTracking()
            .Include(s => s.PaymentMethods)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (supplier is null)
            return Result<SupplierPurchasingDefaultsDto>.Failure("SupplierNotFound", "Supplier not found.");

        var balance = await SupplierMapper.ComputeCurrentBalanceAsync(
            context.PurchaseInvoices.AsNoTracking(), supplier.Id, supplier.OpeningBalance, cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var warnings = SupplierMapper.BuildWarnings(supplier, balance, today);
        var defaultBank = supplier.PaymentMethods.FirstOrDefault(p => p.IsDefault)
                          ?? supplier.PaymentMethods.FirstOrDefault();

        return Result<SupplierPurchasingDefaultsDto>.Success(new SupplierPurchasingDefaultsDto(
            supplier.Id,
            supplier.Code,
            supplier.NameAr,
            supplier.ApAccountId,
            supplier.Currency,
            supplier.DefaultTaxPercent,
            supplier.PaymentDueDays,
            supplier.PaymentTerms,
            supplier.DefaultPaymentMethod,
            supplier.CreditLimit,
            balance,
            supplier.IsOverCreditLimit(balance),
            supplier.IsBlacklisted,
            supplier.IsActive,
            defaultBank is null ? null : new SupplierPaymentMethodDto(
                defaultBank.Id, defaultBank.Kind, defaultBank.BankName, defaultBank.Iban, defaultBank.Swift,
                defaultBank.AccountNumber, defaultBank.BeneficiaryName, defaultBank.Currency,
                defaultBank.IsDefault, defaultBank.Notes),
            warnings));
    }
}
