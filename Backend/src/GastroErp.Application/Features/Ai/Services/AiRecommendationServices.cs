using System.Text.Json;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Features.Ai.DTOs;
using GastroErp.Domain.Entities.Ai;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Ai.Services;

public interface IPurchaseRecommendationService
{
    Task<PurchaseRecommendationsResultDto> GetRecommendationsAsync(Guid tenantId, Guid? branchId = null, CancellationToken ct = default);
}

public interface IRecipeCostOptimizationService
{
    Task<RecipeCostRecommendationsResultDto> GetRecommendationsAsync(Guid tenantId, CancellationToken ct = default);
}

public interface IStaffSchedulingAdvisorService
{
    Task<StaffSchedulingRecommendationsResultDto> GetRecommendationsAsync(Guid tenantId, Guid? branchId = null, CancellationToken ct = default);
}

public interface IDynamicPricingService
{
    Task<DynamicPricingRecommendationsResultDto> GetRecommendationsAsync(Guid tenantId, Guid? branchId = null, CancellationToken ct = default);
}

public interface IRecommendationActionService
{
    Task<IReadOnlyList<RecommendationActionDto>> GetActionsAsync(Guid tenantId, RecommendationFilterDto filter, CancellationToken ct = default);
    Task ApplyAsync(Guid tenantId, Guid actionId, Guid userId, CancellationToken ct = default);
    Task DismissAsync(Guid tenantId, Guid actionId, Guid userId, string? reason = null, CancellationToken ct = default);
    Task<int> PersistRecommendationsAsync(Guid tenantId, RefreshRecommendationsDto options, CancellationToken ct = default);
}

public sealed class PurchaseRecommendationService : IPurchaseRecommendationService
{
    private readonly IApplicationDbContext _context;
    private readonly IInventoryForecastService _forecast;

    public PurchaseRecommendationService(IApplicationDbContext context, IInventoryForecastService forecast)
        => (_context, _forecast) = (context, forecast);

    public async Task<PurchaseRecommendationsResultDto> GetRecommendationsAsync(
        Guid tenantId, Guid? branchId = null, CancellationToken ct = default)
    {
        var forecast = await _forecast.ForecastAsync(tenantId, new ForecastFilterDto(BranchId: branchId), ct);
        var highRisk = forecast.Items
            .Where(i => i.RiskLevel is StockOutRiskLevel.High or StockOutRiskLevel.Critical)
            .ToList();

        if (highRisk.Count == 0)
            return new PurchaseRecommendationsResultDto([], 0, 0);

        var itemIds = highRisk.Select(i => i.InventoryItemId).ToList();
        var items = await _context.InventoryItems.AsNoTracking()
            .Where(i => itemIds.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id, ct);

        var orders = await _context.PurchaseOrders.AsNoTracking()
            .Where(p => p.TenantId == tenantId)
            .Include(p => p.Lines)
            .OrderByDescending(p => p.CreatedAt)
            .Take(100)
            .ToListAsync(ct);

        var priceLookup = orders
            .SelectMany(p => p.Lines.Select(l => new { l.InventoryItemId, l.UnitPrice, p.SupplierId, l.CreatedAt }))
            .GroupBy(x => x.InventoryItemId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.CreatedAt).First());
        var preferredSupplier = await _context.Suppliers.AsNoTracking()
            .Where(s => s.TenantId == tenantId && s.IsActive && s.IsPreferred)
            .Select(s => new { s.Id, s.NameAr })
            .FirstOrDefaultAsync(ct);

        var results = new List<PurchaseRecommendationDto>();
        decimal totalCost = 0;

        foreach (var f in highRisk)
        {
            if (!items.TryGetValue(f.InventoryItemId, out var item)) continue;
            var qty = item.ReorderQuantity > 0 ? item.ReorderQuantity : f.SuggestedSafetyStock;
            priceLookup.TryGetValue(f.InventoryItemId, out var priceInfo);
            var unitPrice = priceInfo?.UnitPrice ?? 1;
            var supplierId = priceInfo?.SupplierId ?? preferredSupplier?.Id;
            var supplierName = preferredSupplier?.NameAr;
            var cost = qty * unitPrice;
            totalCost += cost;

            results.Add(new PurchaseRecommendationDto(
                f.InventoryItemId, f.ItemName, f.CurrentStock, qty, cost,
                supplierId, supplierName,
                f.RiskLevel == StockOutRiskLevel.Critical ? 100 : 70,
                $"Stock-out in ~{f.DaysUntilStockout} days; reorder suggested"));
        }

        return new PurchaseRecommendationsResultDto(
            results.OrderByDescending(r => r.PriorityScore).ToList(), totalCost, results.Count);
    }
}

public sealed class RecipeCostOptimizationService : IRecipeCostOptimizationService
{
    private readonly IApplicationDbContext _context;

    public RecipeCostOptimizationService(IApplicationDbContext context) => _context = context;

    public async Task<RecipeCostRecommendationsResultDto> GetRecommendationsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var recipes = await _context.Recipes.AsNoTracking()
            .Include(r => r.Items)
            .Where(r => r.TenantId == tenantId && r.Status == RecipeStatus.Active)
            .Take(50)
            .ToListAsync(ct);

        var productIds = recipes.Select(r => r.ProductId).ToList();
        var products = await _context.Products.AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        var itemIds = recipes.SelectMany(r => r.Items).Select(i => i.InventoryItemId).Distinct().ToList();
        var unitCosts = await _context.StockMovements.AsNoTracking()
            .Where(m => m.TenantId == tenantId && itemIds.Contains(m.InventoryItemId))
            .GroupBy(m => m.InventoryItemId)
            .Select(g => new { ItemId = g.Key, AvgCost = g.Average(x => x.UnitCost) })
            .ToDictionaryAsync(x => x.ItemId, x => x.AvgCost, ct);

        const decimal targetMargin = 35m;
        var results = new List<RecipeCostRecommendationDto>();

        foreach (var recipe in recipes)
        {
            if (!products.TryGetValue(recipe.ProductId, out var product)) continue;

            var cost = recipe.Items.Sum(i =>
            {
                unitCosts.TryGetValue(i.InventoryItemId, out var c);
                var wasteFactor = 1 + i.WastePercentage / 100m;
                return i.Quantity * c * wasteFactor;
            }) / (recipe.Yield > 0 ? recipe.Yield : 1);

            var margin = product.BasePrice > 0 ? (product.BasePrice - cost) / product.BasePrice * 100 : 0;
            if (margin >= targetMargin) continue;

            var savings = (targetMargin / 100 * product.BasePrice) - (product.BasePrice - cost);
            results.Add(new RecipeCostRecommendationDto(
                recipe.Id, recipe.ProductId, recipe.NameAr, cost, product.BasePrice,
                margin, targetMargin, Math.Max(0, savings),
                margin < 20 ? "Review ingredient quantities or negotiate supplier prices"
                    : "Consider minor portion adjustment or price increase"));
        }

        return new RecipeCostRecommendationsResultDto(results.OrderBy(r => r.CurrentMarginPercent).ToList());
    }
}

public sealed class StaffSchedulingAdvisorService : IStaffSchedulingAdvisorService
{
    private readonly IApplicationDbContext _context;

    public StaffSchedulingAdvisorService(IApplicationDbContext context) => _context = context;

    public async Task<StaffSchedulingRecommendationsResultDto> GetRecommendationsAsync(
        Guid tenantId, Guid? branchId = null, CancellationToken ct = default)
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-60));
        var salesQuery = _context.SalesDailySnapshots.AsNoTracking()
            .Where(s => s.TenantId == tenantId && s.BusinessDate >= from);
        if (branchId.HasValue) salesQuery = salesQuery.Where(s => s.BranchId == branchId);

        var sales = await salesQuery.ToListAsync(ct);
        var branches = await _context.Branches.AsNoTracking()
            .Where(b => b.TenantId == tenantId)
            .ToDictionaryAsync(b => b.Id, b => b.NameAr, ct);

        var shiftQuery = _context.CashierShifts.AsNoTracking()
            .Where(s => s.TenantId == tenantId && s.OpenedAt >= DateTimeOffset.UtcNow.AddDays(-60));
        if (branchId.HasValue) shiftQuery = shiftQuery.Where(s => s.BranchId == branchId);

        var shiftsByBranchDow = await shiftQuery
            .GroupBy(s => new { s.BranchId, Dow = s.OpenedAt.DayOfWeek })
            .Select(g => new { g.Key.BranchId, g.Key.Dow, Count = g.Count() })
            .ToListAsync(ct);

        var avgOrders = sales.Any() ? sales.Average(s => (double)s.OrderCount) : 0;
        var results = new List<StaffSchedulingRecommendationDto>();

        foreach (var group in sales.GroupBy(s => new { s.BranchId, s.BusinessDate.DayOfWeek }))
        {
            var peakOrders = (int)group.Average(s => s.OrderCount);
            var recommended = Math.Max(1, (int)Math.Ceiling(peakOrders / Math.Max(avgOrders, 1) * 2));
            var currentAvg = shiftsByBranchDow
                .FirstOrDefault(x => x.BranchId == group.Key.BranchId && x.Dow == group.Key.DayOfWeek)?.Count ?? 0;
            var weeks = 8;
            currentAvg = currentAvg / Math.Max(weeks, 1);

            var alert = recommended > currentAvg + 1 ? "UnderStaffed"
                : recommended < currentAvg - 1 ? "OverStaffed" : "Adequate";

            if (alert == "Adequate") continue;

            branches.TryGetValue(group.Key.BranchId, out var branchName);
            results.Add(new StaffSchedulingRecommendationDto(
                group.Key.BranchId, branchName ?? "Branch", group.Key.DayOfWeek,
                recommended, currentAvg, peakOrders, alert,
                alert == "UnderStaffed"
                    ? $"Add {recommended - currentAvg} staff on {group.Key.DayOfWeek}"
                    : $"Reduce staff by {currentAvg - recommended} on {group.Key.DayOfWeek}"));
        }

        return new StaffSchedulingRecommendationsResultDto(results.OrderByDescending(r => r.PeakHourOrders).Take(20).ToList());
    }
}

public sealed class DynamicPricingService : IDynamicPricingService
{
    private readonly IApplicationDbContext _context;
    private readonly IDemandForecastService _demand;

    public DynamicPricingService(IApplicationDbContext context, IDemandForecastService demand)
        => (_context, _demand) = (context, demand);

    public async Task<DynamicPricingRecommendationsResultDto> GetRecommendationsAsync(
        Guid tenantId, Guid? branchId = null, CancellationToken ct = default)
    {
        var demand = await _demand.ForecastAsync(tenantId, new ForecastFilterDto(BranchId: branchId, DaysAhead: 7), ct);
        if (demand.Items.Count == 0)
            return new DynamicPricingRecommendationsResultDto([]);

        var productIds = demand.Items.Select(i => i.ProductId).ToList();
        var products = await _context.Products.AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        var results = new List<DynamicPricingRecommendationDto>();

        foreach (var item in demand.Items)
        {
            if (!products.TryGetValue(item.ProductId, out var product)) continue;

            var forecastTotal = item.Forecast.Sum(f => f.PredictedValue);
            var trend = item.AvgDailyQuantity > 0
                ? (forecastTotal / 7 - item.AvgDailyQuantity) / item.AvgDailyQuantity * 100
                : 0;

            decimal suggested = product.BasePrice;
            string reason;

            if (trend > 15)
            {
                suggested = Math.Round(product.BasePrice * 1.05m, 2);
                reason = "High demand trend — consider 5% price increase";
            }
            else if (trend < -15)
            {
                suggested = Math.Round(product.BasePrice * 0.95m, 2);
                reason = "Low demand trend — consider 5% promotional discount";
            }
            else continue;

            var impact = (double)(suggested - product.BasePrice) * forecastTotal;
            results.Add(new DynamicPricingRecommendationDto(
                item.ProductId, item.ProductName, product.BasePrice, suggested,
                trend, (decimal)impact, reason));
        }

        return new DynamicPricingRecommendationsResultDto(results.OrderByDescending(r => Math.Abs(r.EstimatedRevenueImpact)).Take(20).ToList());
    }
}

public sealed class RecommendationActionService : IRecommendationActionService
{
    private readonly IApplicationDbContext _context;
    private readonly IPurchaseRecommendationService _purchase;
    private readonly IRecipeCostOptimizationService _recipe;
    private readonly IStaffSchedulingAdvisorService _staff;
    private readonly IDynamicPricingService _pricing;
    private readonly ILogger<RecommendationActionService> _logger;

    public RecommendationActionService(
        IApplicationDbContext context, IPurchaseRecommendationService purchase,
        IRecipeCostOptimizationService recipe, IStaffSchedulingAdvisorService staff,
        IDynamicPricingService pricing, ILogger<RecommendationActionService> logger)
        => (_context, _purchase, _recipe, _staff, _pricing, _logger) = (context, purchase, recipe, staff, pricing, logger);

    public async Task<IReadOnlyList<RecommendationActionDto>> GetActionsAsync(
        Guid tenantId, RecommendationFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.RecommendationActions.AsNoTracking().Where(r => r.TenantId == tenantId);
        if (filter.Type.HasValue) query = query.Where(r => r.Type == filter.Type);
        if (filter.Status.HasValue) query = query.Where(r => r.Status == filter.Status);
        if (filter.BranchId.HasValue) query = query.Where(r => r.BranchId == filter.BranchId);

        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        return await query.OrderByDescending(r => r.CreatedAt)
            .Skip((Math.Max(filter.Page, 1) - 1) * pageSize).Take(pageSize)
            .Select(r => new RecommendationActionDto(
                r.Id, r.Type, r.Status, r.Priority, r.Title, r.Description,
                r.PayloadJson, r.ExplainabilityJson, r.BranchId, r.ReferenceId,
                r.CreatedAt, r.AppliedAt, r.DismissedAt))
            .ToListAsync(ct);
    }

    public async Task ApplyAsync(Guid tenantId, Guid actionId, Guid userId, CancellationToken ct = default)
    {
        var action = await _context.RecommendationActions
            .FirstOrDefaultAsync(r => r.Id == actionId && r.TenantId == tenantId, ct)
            ?? throw new InvalidOperationException("Recommendation not found.");

        action.Apply(userId);
        _context.RecommendationActions.Update(action);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Recommendation {Id} applied by {UserId}", actionId, userId);
    }

    public async Task DismissAsync(Guid tenantId, Guid actionId, Guid userId, string? reason = null, CancellationToken ct = default)
    {
        var action = await _context.RecommendationActions
            .FirstOrDefaultAsync(r => r.Id == actionId && r.TenantId == tenantId, ct)
            ?? throw new InvalidOperationException("Recommendation not found.");

        action.Dismiss(userId, reason);
        _context.RecommendationActions.Update(action);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<int> PersistRecommendationsAsync(Guid tenantId, RefreshRecommendationsDto options, CancellationToken ct = default)
    {
        var pending = await _context.RecommendationActions
            .Where(r => r.TenantId == tenantId && r.Status == RecommendationStatus.Pending)
            .ToListAsync(ct);
        _context.RecommendationActions.RemoveRange(pending);

        var count = 0;

        if (options.Purchase)
        {
            var p = await _purchase.GetRecommendationsAsync(tenantId, ct: ct);
            foreach (var item in p.Items)
            {
                _context.RecommendationActions.Add(RecommendationAction.Create(
                    tenantId, RecommendationType.Purchase,
                    item.PriorityScore >= 90 ? RecommendationPriority.Critical : RecommendationPriority.High,
                    $"Purchase: {item.ItemName}", item.Reason,
                    JsonSerializer.Serialize(item), JsonSerializer.Serialize(new { item.PriorityScore }),
                    "InventoryItem", item.InventoryItemId));
                count++;
            }
        }

        if (options.RecipeCost)
        {
            var r = await _recipe.GetRecommendationsAsync(tenantId, ct);
            foreach (var item in r.Items)
            {
                _context.RecommendationActions.Add(RecommendationAction.Create(
                    tenantId, RecommendationType.RecipeCost, RecommendationPriority.Medium,
                    $"Recipe margin: {item.RecipeName}", item.Suggestion,
                    JsonSerializer.Serialize(item), JsonSerializer.Serialize(new { item.CurrentMarginPercent }),
                    "Recipe", item.RecipeId));
                count++;
            }
        }

        if (options.StaffScheduling)
        {
            var s = await _staff.GetRecommendationsAsync(tenantId, ct: ct);
            foreach (var item in s.Items)
            {
                _context.RecommendationActions.Add(RecommendationAction.Create(
                    tenantId, RecommendationType.StaffScheduling, RecommendationPriority.Medium,
                    $"Staffing: {item.BranchName} — {item.DayOfWeek}", item.Suggestion,
                    JsonSerializer.Serialize(item), JsonSerializer.Serialize(new { item.AlertLevel }),
                    "Branch", item.BranchId, branchId: item.BranchId));
                count++;
            }
        }

        if (options.DynamicPricing)
        {
            var d = await _pricing.GetRecommendationsAsync(tenantId, ct: ct);
            foreach (var item in d.Items)
            {
                _context.RecommendationActions.Add(RecommendationAction.Create(
                    tenantId, RecommendationType.DynamicPricing, RecommendationPriority.Medium,
                    $"Pricing: {item.ProductName}", item.Reason,
                    JsonSerializer.Serialize(item), JsonSerializer.Serialize(new { item.DemandTrendPercent }),
                    "Product", item.ProductId));
                count++;
            }
        }

        await _context.SaveChangesAsync(ct);
        return count;
    }
}
