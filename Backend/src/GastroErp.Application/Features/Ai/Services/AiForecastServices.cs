using System.Text.Json;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Features.Ai.DTOs;
using GastroErp.Domain.Entities.Ai;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Ai.Services;

internal static class ForecastHeuristics
{
    public static (double Value, double Lower, double Upper, double Confidence) Project(
        double baseline, double trendPercent, int daysAhead, int dataPoints)
    {
        var trendFactor = 1 + trendPercent / 100.0;
        var predicted = baseline * Math.Pow(trendFactor, daysAhead / 7.0);
        var confidence = Math.Clamp(dataPoints * 5.0, 40, 92);
        var margin = predicted * (1 - confidence / 100) * 0.5;
        return (predicted, Math.Max(0, predicted - margin), predicted + margin, confidence);
    }

    public static double ComputeTrend(IReadOnlyList<double> values)
    {
        if (values.Count < 2) return 0;
        var half = values.Count / 2;
        var recent = values.TakeLast(half).DefaultIfEmpty(0).Average();
        var prior = values.Take(half).DefaultIfEmpty(0).Average();
        return prior > 0 ? (recent - prior) / prior * 100 : 0;
    }
}

public sealed class PredictionRunService : IPredictionRunService
{
    private readonly IApplicationDbContext _context;
    public PredictionRunService(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<PredictionRunDto>> GetRecentAsync(
        Guid tenantId, ForecastType? type = null, int take = 50, CancellationToken ct = default)
    {
        var query = _context.PredictionRuns.AsNoTracking().Where(p => p.TenantId == tenantId);
        if (type.HasValue) query = query.Where(p => p.ForecastType == type);
        return await query.OrderByDescending(p => p.CreatedAt).Take(take)
            .Select(p => new PredictionRunDto(
                p.Id, p.ForecastType, p.Provider, p.Status, p.ForecastDate,
                p.Confidence, p.BranchId, p.EntityId, p.CreatedAt))
            .ToListAsync(ct);
    }

    internal static async Task SaveRunAsync(
        IApplicationDbContext context, Guid tenantId, ForecastType type,
        DateOnly date, double confidence, object output, object explainability,
        Guid? branchId = null, Guid? entityId = null, CancellationToken ct = default)
    {
        var run = PredictionRun.Create(tenantId, type, AiModelProvider.Heuristic, date, branchId, entityId);
        run.Complete(confidence, JsonSerializer.Serialize(output), JsonSerializer.Serialize(explainability));
        context.PredictionRuns.Add(run);
        await context.SaveChangesAsync(ct);
    }
}

public sealed class DemandForecastService : IDemandForecastService
{
    private readonly IApplicationDbContext _context;

    public DemandForecastService(IApplicationDbContext context) => _context = context;

    public async Task<DemandForecastResultDto> ForecastAsync(
        Guid tenantId, ForecastFilterDto filter, CancellationToken ct = default)
    {
        var from = DateTimeOffset.UtcNow.AddDays(-30);
        var ordersQuery = _context.SalesOrders.AsNoTracking()
            .Where(o => o.TenantId == tenantId && o.Status == OrderStatus.Completed && o.CompletedAt >= from);
        if (filter.BranchId.HasValue) ordersQuery = ordersQuery.Where(o => o.BranchId == filter.BranchId);

        var orderIds = await ordersQuery.Select(o => o.Id).ToListAsync(ct);
        if (orderIds.Count == 0)
            return new DemandForecastResultDto(filter.BranchId, DateTimeOffset.UtcNow, AiModelProvider.Heuristic, []);

        var itemsQuery = _context.OrderItems.AsNoTracking()
            .Where(i => orderIds.Contains(i.SalesOrderId) && !i.IsVoided);
        if (filter.ProductId.HasValue) itemsQuery = itemsQuery.Where(i => i.ProductId == filter.ProductId);

        var itemAgg = await itemsQuery
            .GroupBy(i => new { i.ProductId, i.ProductNameAr })
            .Select(g => new { g.Key.ProductId, g.Key.ProductNameAr, TotalQty = g.Sum(x => x.Quantity), Days = 30 })
            .OrderByDescending(x => x.TotalQty)
            .Take(50)
            .ToListAsync(ct);

        var dowMultiplier = await GetDayOfWeekMultiplierAsync(tenantId, filter.BranchId, ct);
        var results = new List<DemandForecastItemDto>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        foreach (var item in itemAgg)
        {
            var avgDaily = (double)item.TotalQty / item.Days;
            var periods = new List<ForecastPeriodDto>();

            for (var d = 1; d <= filter.DaysAhead; d++)
            {
                var date = today.AddDays(d);
                var mult = dowMultiplier.GetValueOrDefault(date.DayOfWeek.ToString(), 1.0);
                var baseline = avgDaily * mult;
                var (val, lower, upper, conf) = ForecastHeuristics.Project(baseline, 0, d, itemAgg.Count);
                periods.Add(new ForecastPeriodDto(date, val, lower, upper, conf));
            }

            var explain = new { method = "moving_average_dow", avgDaily, dataPoints = item.TotalQty, dowMultiplier };
            results.Add(new DemandForecastItemDto(
                item.ProductId, item.ProductNameAr, avgDaily, periods,
                JsonSerializer.Serialize(explain)));

            await PredictionRunService.SaveRunAsync(_context, tenantId, ForecastType.Demand, today.AddDays(1),
                periods[0].Confidence, new { item.ProductId, periods[0].PredictedValue }, explain,
                filter.BranchId, item.ProductId, ct);
        }

        return new DemandForecastResultDto(filter.BranchId, DateTimeOffset.UtcNow, AiModelProvider.Heuristic, results);
    }

    private async Task<Dictionary<string, double>> GetDayOfWeekMultiplierAsync(
        Guid tenantId, Guid? branchId, CancellationToken ct)
    {
        var query = _context.SalesDailySnapshots.AsNoTracking().Where(s => s.TenantId == tenantId);
        if (branchId.HasValue) query = query.Where(s => s.BranchId == branchId);

        var snapshots = await query.ToListAsync(ct);
        if (snapshots.Count == 0) return new Dictionary<string, double>();

        var avg = snapshots.Average(s => (double)s.NetRevenue);
        if (avg <= 0) return new Dictionary<string, double>();

        return snapshots.GroupBy(s => s.BusinessDate.DayOfWeek.ToString())
            .ToDictionary(g => g.Key, g => g.Sum(x => (double)x.NetRevenue) / g.Count() / avg);
    }
}

public sealed class SalesForecastService : ISalesForecastService
{
    private readonly IApplicationDbContext _context;

    public SalesForecastService(IApplicationDbContext context) => _context = context;

    public async Task<SalesForecastResultDto> ForecastAsync(
        Guid tenantId, ForecastFilterDto filter, CancellationToken ct = default)
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-60));
        var query = _context.SalesDailySnapshots.AsNoTracking()
            .Where(s => s.TenantId == tenantId && s.BusinessDate >= from);
        if (filter.BranchId.HasValue) query = query.Where(s => s.BranchId == filter.BranchId);

        var snapshots = await query.OrderBy(s => s.BusinessDate).ToListAsync(ct);
        var branches = snapshots.GroupBy(s => s.BranchId).ToList();
        var results = new List<SalesForecastBranchDto>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        double totalPredicted = 0;

        foreach (var branch in branches)
        {
            var revenues = branch.Select(s => (double)s.NetRevenue).ToList();
            var avgDaily = revenues.DefaultIfEmpty(0).Average();
            var trend = ForecastHeuristics.ComputeTrend(revenues);
            var periods = new List<ForecastPeriodDto>();

            for (var d = 1; d <= filter.DaysAhead; d++)
            {
                var date = today.AddDays(d);
                var (val, lower, upper, conf) = ForecastHeuristics.Project(avgDaily, trend, d, revenues.Count);
                periods.Add(new ForecastPeriodDto(date, val, lower, upper, conf));
                if (d == filter.DaysAhead) totalPredicted += val;
            }

            var explain = new { method = "moving_average_trend", avgDaily, trend, dataPoints = revenues.Count };
            results.Add(new SalesForecastBranchDto(branch.Key, avgDaily, trend, periods, JsonSerializer.Serialize(explain)));

            await PredictionRunService.SaveRunAsync(_context, tenantId, ForecastType.Sales, today.AddDays(1),
                periods[0].Confidence, new { branch.Key, total = periods.Sum(p => p.PredictedValue) }, explain,
                branch.Key, null, ct);
        }

        return new SalesForecastResultDto(DateTimeOffset.UtcNow, AiModelProvider.Heuristic, results, totalPredicted);
    }
}

public sealed class InventoryForecastService : IInventoryForecastService
{
    private readonly IApplicationDbContext _context;

    public InventoryForecastService(IApplicationDbContext context) => _context = context;

    public async Task<InventoryForecastResultDto> ForecastAsync(
        Guid tenantId, ForecastFilterDto filter, CancellationToken ct = default)
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var invQuery = _context.InventoryDailySnapshots.AsNoTracking()
            .Where(s => s.TenantId == tenantId && s.BusinessDate >= from);
        if (filter.InventoryItemId.HasValue) invQuery = invQuery.Where(s => s.InventoryItemId == filter.InventoryItemId);

        var snapshots = await invQuery.ToListAsync(ct);
        var itemIds = snapshots.Select(s => s.InventoryItemId).Distinct().ToList();

        var items = await _context.InventoryItems.AsNoTracking()
            .Where(i => itemIds.Contains(i.Id))
            .Select(i => new { i.Id, i.NameAr, i.ReorderLevel })
            .ToDictionaryAsync(i => i.Id, ct);

        var balances = await _context.StockMovements.AsNoTracking()
            .Where(m => m.TenantId == tenantId && itemIds.Contains(m.InventoryItemId))
            .GroupBy(m => m.InventoryItemId)
            .Select(g => new { ItemId = g.Key, Balance = g.Sum(x => x.QuantityChange) })
            .ToDictionaryAsync(x => x.ItemId, x => x.Balance, ct);

        var results = new List<InventoryForecastItemDto>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var highRisk = 0;

        foreach (var group in snapshots.GroupBy(s => s.InventoryItemId))
        {
            if (!items.TryGetValue(group.Key, out var item)) continue;

            var days = group.Select(g => g.BusinessDate).Distinct().Count();
            if (days == 0) days = 1;
            var avgConsumption = (double)group.Sum(g => g.ConsumptionQty) / days;
            balances.TryGetValue(group.Key, out var stock);
            var daysUntil = avgConsumption > 0 ? (int)(stock / (decimal)avgConsumption) : 999;

            var risk = daysUntil switch
            {
                <= 3 => StockOutRiskLevel.Critical,
                <= 7 => StockOutRiskLevel.High,
                <= 14 => StockOutRiskLevel.Medium,
                _ => StockOutRiskLevel.Low
            };
            if (risk is StockOutRiskLevel.High or StockOutRiskLevel.Critical) highRisk++;

            var safetyStock = (decimal)(avgConsumption * 7);
            var explain = new
            {
                method = "consumption_rate",
                avgConsumption,
                currentStock = stock,
                reorderLevel = item.ReorderLevel,
                dataPoints = group.Count()
            };

            results.Add(new InventoryForecastItemDto(
                group.Key, item.NameAr, stock, avgConsumption, daysUntil, risk, safetyStock,
                JsonSerializer.Serialize(explain)));

            await PredictionRunService.SaveRunAsync(_context, tenantId, ForecastType.Inventory, today,
                avgConsumption > 0 ? 75 : 40, new { group.Key, daysUntil, risk }, explain,
                null, group.Key, ct);
        }

        return new InventoryForecastResultDto(
            DateTimeOffset.UtcNow, AiModelProvider.Heuristic,
            results.OrderBy(r => r.DaysUntilStockout).ToList(), highRisk);
    }
}

public sealed class AiForecastOrchestrator : IAiForecastOrchestrator
{
    private readonly IApplicationDbContext _context;
    private readonly IDemandForecastService _demand;
    private readonly ISalesForecastService _sales;
    private readonly IInventoryForecastService _inventory;
    private readonly ILogger<AiForecastOrchestrator> _logger;

    public AiForecastOrchestrator(
        IApplicationDbContext context, IDemandForecastService demand,
        ISalesForecastService sales, IInventoryForecastService inventory,
        ILogger<AiForecastOrchestrator> logger)
        => (_context, _demand, _sales, _inventory, _logger) = (context, demand, sales, inventory, logger);

    public async Task<RefreshForecastsResultDto> RefreshAllAsync(
        Guid tenantId, RefreshForecastsDto options, CancellationToken ct = default)
    {
        await EnsureModelsAsync(tenantId, ct);
        var filter = new ForecastFilterDto(DaysAhead: 7);
        var demandCount = 0;
        var salesCount = 0;
        var inventoryCount = 0;

        if (options.Demand)
        {
            var d = await _demand.ForecastAsync(tenantId, filter, ct);
            demandCount = d.Items.Count;
        }

        if (options.Sales)
        {
            var s = await _sales.ForecastAsync(tenantId, filter, ct);
            salesCount = s.Branches.Count;
        }

        if (options.Inventory)
        {
            var i = await _inventory.ForecastAsync(tenantId, filter, ct);
            inventoryCount = i.Items.Count;
        }

        _logger.LogInformation(
            "Forecasts refreshed for tenant {TenantId}: demand={Demand}, sales={Sales}, inventory={Inventory}",
            tenantId, demandCount, salesCount, inventoryCount);

        return new RefreshForecastsResultDto(demandCount, salesCount, inventoryCount);
    }

    private async Task EnsureModelsAsync(Guid tenantId, CancellationToken ct)
    {
        if (await _context.AiModelRegistries.AnyAsync(m => m.TenantId == tenantId, ct)) return;

        _context.AiModelRegistries.AddRange(
            AiModelRegistry.Create(tenantId, ForecastType.Demand, AiModelProvider.Heuristic, "DemandMovingAverage"),
            AiModelRegistry.Create(tenantId, ForecastType.Sales, AiModelProvider.Heuristic, "SalesTrendAverage"),
            AiModelRegistry.Create(tenantId, ForecastType.Inventory, AiModelProvider.Heuristic, "InventoryConsumptionRate"));

        await _context.SaveChangesAsync(ct);
    }
}
