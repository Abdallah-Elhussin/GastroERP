using System.Text;
using System.Text.Json;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Features.Ai.DTOs;
using GastroErp.Domain.Entities.Ai;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Ai.Services;

public sealed class DataWarehouseSyncService : IDataWarehouseSyncService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DataWarehouseSyncService> _logger;

    public DataWarehouseSyncService(IApplicationDbContext context, ILogger<DataWarehouseSyncService> logger)
        => (_context, _logger) = (context, logger);

    public async Task<WarehouseSyncRunDto> SyncAsync(Guid tenantId, int lookbackDays = 90, CancellationToken ct = default)
    {
        var run = WarehouseSyncRun.Create(tenantId);
        _context.WarehouseSyncRuns.Add(run);
        await _context.SaveChangesAsync(ct);

        try
        {
            run.Start();
            _context.WarehouseSyncRuns.Update(run);
            await _context.SaveChangesAsync(ct);

            var fromDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-lookbackDays));
            var salesCount = await SyncSalesFactsAsync(tenantId, fromDate, ct);
            var inventoryCount = await SyncInventoryFactsAsync(tenantId, fromDate, ct);

            run.Complete(salesCount, inventoryCount);
        }
        catch (Exception ex)
        {
            run.Fail(ex.Message);
            _logger.LogError(ex, "Warehouse sync failed for tenant {TenantId}", tenantId);
        }

        _context.WarehouseSyncRuns.Update(run);
        await _context.SaveChangesAsync(ct);
        return MapRun(run);
    }

    public async Task<WarehouseStatusDto> GetStatusAsync(Guid tenantId, CancellationToken ct = default)
    {
        var last = await _context.WarehouseSyncRuns.AsNoTracking()
            .Where(r => r.TenantId == tenantId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(ct);

        var salesFacts = await _context.SalesDailySnapshots.AsNoTracking()
            .CountAsync(s => s.TenantId == tenantId, ct);
        var inventoryFacts = await _context.InventoryDailySnapshots.AsNoTracking()
            .CountAsync(s => s.TenantId == tenantId, ct);

        var isStale = last?.FinishedAt is null || last.FinishedAt < DateTimeOffset.UtcNow.AddDays(-1);

        return new WarehouseStatusDto(
            last?.FinishedAt ?? last?.StartedAt, last?.Status,
            salesFacts, inventoryFacts, isStale);
    }

    public async Task<IReadOnlyList<WarehouseSyncRunDto>> GetHistoryAsync(Guid tenantId, int take = 20, CancellationToken ct = default)
        => await _context.WarehouseSyncRuns.AsNoTracking()
            .Where(r => r.TenantId == tenantId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(take)
            .Select(r => new WarehouseSyncRunDto(
                r.Id, r.Status, r.StartedAt, r.FinishedAt,
                r.SalesFactsWritten, r.InventoryFactsWritten, r.ErrorMessage))
            .ToListAsync(ct);

    private async Task<int> SyncSalesFactsAsync(Guid tenantId, DateOnly fromDate, CancellationToken ct)
    {
        var from = new DateTimeOffset(fromDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        var aggregates = await _context.SalesOrders.AsNoTracking()
            .Where(o => o.TenantId == tenantId && o.Status == OrderStatus.Completed
                && o.CompletedAt >= from)
            .GroupBy(o => new { o.BranchId, Date = DateOnly.FromDateTime(o.CompletedAt!.Value.UtcDateTime) })
            .Select(g => new
            {
                g.Key.BranchId,
                g.Key.Date,
                OrderCount = g.Count(),
                GrossRevenue = g.Sum(x => x.GrandTotal),
                TaxTotal = g.Sum(x => x.TaxTotal),
                DiscountTotal = g.Sum(x => x.DiscountTotal)
            })
            .ToListAsync(ct);

        var existing = await _context.SalesDailySnapshots
            .Where(s => s.TenantId == tenantId && s.BusinessDate >= fromDate)
            .ToListAsync(ct);

        _context.SalesDailySnapshots.RemoveRange(existing);

        foreach (var a in aggregates)
        {
            _context.SalesDailySnapshots.Add(SalesDailySnapshot.Create(
                tenantId, a.BranchId, a.Date, a.OrderCount, a.GrossRevenue, a.TaxTotal, a.DiscountTotal));
        }

        await _context.SaveChangesAsync(ct);
        return aggregates.Count;
    }

    private async Task<int> SyncInventoryFactsAsync(Guid tenantId, DateOnly fromDate, CancellationToken ct)
    {
        var from = new DateTimeOffset(fromDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        var movements = await _context.StockMovements.AsNoTracking()
            .Where(m => m.TenantId == tenantId && m.CreatedAt >= from)
            .GroupBy(m => new { m.InventoryItemId, Date = DateOnly.FromDateTime(m.CreatedAt.UtcDateTime) })
            .Select(g => new
            {
                g.Key.InventoryItemId,
                g.Key.Date,
                NetChange = g.Sum(x => x.QuantityChange),
                Consumption = g.Where(x => x.QuantityChange < 0).Sum(x => -x.QuantityChange)
            })
            .ToListAsync(ct);

        var wasteByItemDate = await _context.WasteItems.AsNoTracking()
            .Where(w => w.TenantId == tenantId && w.CreatedAt >= from)
            .GroupBy(w => new { w.InventoryItemId, Date = DateOnly.FromDateTime(w.CreatedAt.UtcDateTime) })
            .Select(g => new { g.Key.InventoryItemId, g.Key.Date, Waste = g.Sum(x => x.Quantity) })
            .ToListAsync(ct);

        var wasteLookup = wasteByItemDate.ToDictionary(w => (w.InventoryItemId, w.Date), w => w.Waste);

        var balances = await _context.StockMovements.AsNoTracking()
            .Where(m => m.TenantId == tenantId)
            .GroupBy(m => m.InventoryItemId)
            .Select(g => new { ItemId = g.Key, Balance = g.Sum(x => x.QuantityChange) })
            .ToDictionaryAsync(x => x.ItemId, x => x.Balance, ct);

        var existing = await _context.InventoryDailySnapshots
            .Where(s => s.TenantId == tenantId && s.BusinessDate >= fromDate)
            .ToListAsync(ct);

        _context.InventoryDailySnapshots.RemoveRange(existing);

        foreach (var m in movements)
        {
            var waste = wasteLookup.GetValueOrDefault((m.InventoryItemId, m.Date), 0);
            balances.TryGetValue(m.InventoryItemId, out var closing);
            _context.InventoryDailySnapshots.Add(InventoryDailySnapshot.Create(
                tenantId, m.InventoryItemId, m.Date, m.NetChange, m.Consumption, waste, closing));
        }

        await _context.SaveChangesAsync(ct);
        return movements.Count;
    }

    private static WarehouseSyncRunDto MapRun(WarehouseSyncRun r) =>
        new(r.Id, r.Status, r.StartedAt, r.FinishedAt, r.SalesFactsWritten, r.InventoryFactsWritten, r.ErrorMessage);
}

public sealed class DataQualityService : IDataQualityService
{
    private readonly IApplicationDbContext _context;

    public DataQualityService(IApplicationDbContext context) => _context = context;

    public async Task<DataQualityDashboardDto> EvaluateAsync(Guid tenantId, CancellationToken ct = default)
    {
        var metrics = new List<DataQualityMetricDto>();

        var lastSync = await _context.WarehouseSyncRuns.AsNoTracking()
            .Where(r => r.TenantId == tenantId && r.Status == WarehouseSyncStatus.Succeeded)
            .OrderByDescending(r => r.FinishedAt)
            .FirstOrDefaultAsync(ct);

        var freshnessHours = lastSync?.FinishedAt is null
            ? 999.0
            : (DateTimeOffset.UtcNow - lastSync.FinishedAt.Value).TotalHours;

        var freshnessScore = freshnessHours <= 24 ? 100 : freshnessHours <= 72 ? 70 : 30;
        var freshnessLevel = freshnessScore >= 80 ? DataQualityLevel.Good
            : freshnessScore >= 50 ? DataQualityLevel.Warning : DataQualityLevel.Critical;

        metrics.Add(new DataQualityMetricDto(
            "warehouse_freshness", freshnessLevel, freshnessScore, DateTimeOffset.UtcNow,
            JsonSerializer.Serialize(new { freshnessHours, lastSyncAt = lastSync?.FinishedAt })));

        var salesFacts = await _context.SalesDailySnapshots.AsNoTracking()
            .CountAsync(s => s.TenantId == tenantId, ct);
        var completenessScore = salesFacts > 0 ? Math.Min(100, salesFacts) : 0;
        var completenessLevel = completenessScore >= 30 ? DataQualityLevel.Good
            : completenessScore >= 7 ? DataQualityLevel.Warning : DataQualityLevel.Critical;

        metrics.Add(new DataQualityMetricDto(
            "sales_fact_completeness", completenessLevel, completenessScore, DateTimeOffset.UtcNow,
            JsonSerializer.Serialize(new { salesFacts })));

        var featureCount = await _context.FeatureStoreSnapshots.AsNoTracking()
            .CountAsync(f => f.TenantId == tenantId, ct);
        var featureScore = featureCount >= 5 ? 100 : featureCount * 20;
        metrics.Add(new DataQualityMetricDto(
            "feature_store_coverage", featureScore >= 80 ? DataQualityLevel.Good : DataQualityLevel.Warning,
            featureScore, DateTimeOffset.UtcNow, JsonSerializer.Serialize(new { featureCount })));

        foreach (var m in metrics)
        {
            _context.DataQualityMetrics.Add(DataQualityMetric.Create(
                tenantId, m.MetricName, m.Level, m.Score, m.DetailsJson));
        }

        await _context.SaveChangesAsync(ct);

        var overall = metrics.Average(m => m.Score);
        var overallLevel = overall >= 80 ? DataQualityLevel.Good
            : overall >= 50 ? DataQualityLevel.Warning : DataQualityLevel.Critical;

        return new DataQualityDashboardDto(overall, overallLevel, metrics);
    }
}

public sealed class FeatureStoreService : IFeatureStoreService
{
    private readonly IApplicationDbContext _context;

    public FeatureStoreService(IApplicationDbContext context) => _context = context;

    public async Task EnsureDefaultDefinitionsAsync(Guid tenantId, CancellationToken ct = default)
    {
        if (await _context.FeatureDefinitions.AnyAsync(f => f.TenantId == tenantId, ct)) return;

        var defs = new[]
        {
            FeatureDefinition.Create(tenantId, AiFeatureGroup.SalesVelocity, FeatureEntityType.Branch, "Sales Velocity", "Average daily orders and revenue"),
            FeatureDefinition.Create(tenantId, AiFeatureGroup.Seasonality, FeatureEntityType.Branch, "Seasonality", "Day-of-week sales patterns"),
            FeatureDefinition.Create(tenantId, AiFeatureGroup.StockTurnover, FeatureEntityType.InventoryItem, "Stock Turnover", "Consumption vs closing balance"),
            FeatureDefinition.Create(tenantId, AiFeatureGroup.CustomerRfm, FeatureEntityType.Customer, "Customer RFM", "Recency, frequency, monetary value"),
            FeatureDefinition.Create(tenantId, AiFeatureGroup.KitchenLoad, FeatureEntityType.Branch, "Kitchen Load", "Ticket volume and prep times")
        };

        _context.FeatureDefinitions.AddRange(defs);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<FeatureDefinitionDto>> GetDefinitionsAsync(Guid tenantId, CancellationToken ct = default)
        => await _context.FeatureDefinitions.AsNoTracking()
            .Where(f => f.TenantId == tenantId && f.IsActive)
            .Select(f => new FeatureDefinitionDto(
                f.Id, f.FeatureGroup, f.EntityType, f.Name, f.Description, f.Version, f.IsActive))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<FeatureSnapshotDto>> GetSnapshotsAsync(
        Guid tenantId, AiDataFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.FeatureStoreSnapshots.AsNoTracking().Where(f => f.TenantId == tenantId);
        if (filter.FeatureGroup.HasValue) query = query.Where(f => f.FeatureGroup == filter.FeatureGroup);
        if (filter.FromDate.HasValue) query = query.Where(f => f.AsOfDate >= filter.FromDate);
        if (filter.ToDate.HasValue) query = query.Where(f => f.AsOfDate <= filter.ToDate);

        return await query.OrderByDescending(f => f.AsOfDate).Take(500)
            .Select(f => new FeatureSnapshotDto(
                f.Id, f.FeatureGroup, f.EntityType, f.EntityId, f.AsOfDate, f.FeaturesJson))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<FeatureLineageDto>> GetLineageAsync(Guid tenantId, CancellationToken ct = default)
        => await _context.FeatureLineages.AsNoTracking()
            .Where(l => l.TenantId == tenantId)
            .Select(l => new FeatureLineageDto(
                l.FeatureGroup, l.SourceTables, l.LastRefreshedAt, l.QualityScore, l.RecordCount))
            .ToListAsync(ct);
}

public sealed class FeatureComputationService : IFeatureComputationService
{
    private readonly IApplicationDbContext _context;
    private readonly IFeatureStoreService _featureStore;

    public FeatureComputationService(IApplicationDbContext context, IFeatureStoreService featureStore)
        => (_context, _featureStore) = (context, featureStore);

    public async Task ComputeAllAsync(Guid tenantId, CancellationToken ct = default)
    {
        await _featureStore.EnsureDefaultDefinitionsAsync(tenantId, ct);
        foreach (AiFeatureGroup group in Enum.GetValues<AiFeatureGroup>())
            await ComputeGroupAsync(tenantId, group, ct);
    }

    public async Task ComputeGroupAsync(Guid tenantId, AiFeatureGroup group, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var from = today.AddDays(-30);
        var count = 0;

        switch (group)
        {
            case AiFeatureGroup.SalesVelocity:
                count = await ComputeSalesVelocityAsync(tenantId, today, from, ct);
                break;
            case AiFeatureGroup.Seasonality:
                count = await ComputeSeasonalityAsync(tenantId, today, ct);
                break;
            case AiFeatureGroup.StockTurnover:
                count = await ComputeStockTurnoverAsync(tenantId, today, from, ct);
                break;
            case AiFeatureGroup.CustomerRfm:
                count = await ComputeCustomerRfmAsync(tenantId, today, ct);
                break;
            case AiFeatureGroup.KitchenLoad:
                count = await ComputeKitchenLoadAsync(tenantId, today, from, ct);
                break;
        }

        await UpsertLineageAsync(tenantId, group, count, ct);
    }

    private async Task<int> ComputeSalesVelocityAsync(Guid tenantId, DateOnly today, DateOnly from, CancellationToken ct)
    {
        var data = await _context.SalesDailySnapshots.AsNoTracking()
            .Where(s => s.TenantId == tenantId && s.BusinessDate >= from)
            .GroupBy(s => s.BranchId)
            .Select(g => new
            {
                BranchId = g.Key,
                AvgOrders = g.Average(x => (double)x.OrderCount),
                AvgRevenue = g.Average(x => (double)x.NetRevenue)
            })
            .ToListAsync(ct);

        await RemoveSnapshotsAsync(tenantId, AiFeatureGroup.SalesVelocity, today, ct);

        foreach (var d in data)
        {
            var json = JsonSerializer.Serialize(new { d.AvgOrders, d.AvgRevenue, windowDays = 30 });
            _context.FeatureStoreSnapshots.Add(FeatureStoreSnapshot.Create(
                tenantId, AiFeatureGroup.SalesVelocity, FeatureEntityType.Branch, d.BranchId, today, json));
        }

        await _context.SaveChangesAsync(ct);
        return data.Count;
    }

    private async Task<int> ComputeSeasonalityAsync(Guid tenantId, DateOnly today, CancellationToken ct)
    {
        var snapshots = await _context.SalesDailySnapshots.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .ToListAsync(ct);

        var byBranch = snapshots.GroupBy(s => s.BranchId);
        var count = 0;

        await RemoveSnapshotsAsync(tenantId, AiFeatureGroup.Seasonality, today, ct);

        foreach (var branch in byBranch)
        {
            var dowPattern = branch.GroupBy(s => s.BusinessDate.DayOfWeek)
                .ToDictionary(g => g.Key.ToString(), g => g.Sum(x => (double)x.NetRevenue));
            var json = JsonSerializer.Serialize(dowPattern);
            _context.FeatureStoreSnapshots.Add(FeatureStoreSnapshot.Create(
                tenantId, AiFeatureGroup.Seasonality, FeatureEntityType.Branch, branch.Key, today, json));
            count++;
        }

        await _context.SaveChangesAsync(ct);
        return count;
    }

    private async Task<int> ComputeStockTurnoverAsync(Guid tenantId, DateOnly today, DateOnly from, CancellationToken ct)
    {
        var data = await _context.InventoryDailySnapshots.AsNoTracking()
            .Where(s => s.TenantId == tenantId && s.BusinessDate >= from)
            .GroupBy(s => s.InventoryItemId)
            .Select(g => new
            {
                ItemId = g.Key,
                TotalConsumption = g.Sum(x => x.ConsumptionQty),
                AvgClosing = g.Average(x => (double)x.ClosingBalance)
            })
            .ToListAsync(ct);

        await RemoveSnapshotsAsync(tenantId, AiFeatureGroup.StockTurnover, today, ct);

        foreach (var d in data)
        {
            var turnover = d.AvgClosing > 0 ? d.TotalConsumption / (decimal)d.AvgClosing : 0;
            var json = JsonSerializer.Serialize(new { d.TotalConsumption, d.AvgClosing, turnover });
            _context.FeatureStoreSnapshots.Add(FeatureStoreSnapshot.Create(
                tenantId, AiFeatureGroup.StockTurnover, FeatureEntityType.InventoryItem, d.ItemId, today, json));
        }

        await _context.SaveChangesAsync(ct);
        return data.Count;
    }

    private async Task<int> ComputeCustomerRfmAsync(Guid tenantId, DateOnly today, CancellationToken ct)
    {
        var raw = await _context.SalesOrders.AsNoTracking()
            .Where(o => o.TenantId == tenantId && o.Status == OrderStatus.Completed && o.CustomerId != null)
            .Select(o => new { CustomerId = o.CustomerId!.Value, o.CompletedAt, o.GrandTotal })
            .ToListAsync(ct);

        var orders = raw.GroupBy(o => o.CustomerId)
            .Select(g => new
            {
                CustomerId = g.Key,
                RecencyDays = g.Max(x => x.CompletedAt) is { } last
                    ? (DateTimeOffset.UtcNow - last).TotalDays : 999,
                Frequency = g.Count(),
                Monetary = g.Sum(x => x.GrandTotal)
            })
            .Take(1000)
            .ToList();

        await RemoveSnapshotsAsync(tenantId, AiFeatureGroup.CustomerRfm, today, ct);

        foreach (var c in orders)
        {
            var json = JsonSerializer.Serialize(new { c.RecencyDays, c.Frequency, c.Monetary });
            _context.FeatureStoreSnapshots.Add(FeatureStoreSnapshot.Create(
                tenantId, AiFeatureGroup.CustomerRfm, FeatureEntityType.Customer, c.CustomerId, today, json));
        }

        await _context.SaveChangesAsync(ct);
        return orders.Count;
    }

    private async Task<int> ComputeKitchenLoadAsync(Guid tenantId, DateOnly today, DateOnly from, CancellationToken ct)
    {
        var fromDt = new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        var tickets = await _context.KitchenTickets.AsNoTracking()
            .Where(t => t.TenantId == tenantId && t.CreatedAt >= fromDt
                && t.CompletedAt != null && t.StartedAt != null)
            .Select(t => new { t.BranchId, t.StartedAt, t.CompletedAt })
            .ToListAsync(ct);

        var data = tickets.GroupBy(t => t.BranchId)
            .Select(g => new
            {
                BranchId = g.Key,
                TicketCount = g.Count(),
                AvgPrepMinutes = g.Average(t => (t.CompletedAt!.Value - t.StartedAt!.Value).TotalMinutes)
            })
            .ToList();

        await RemoveSnapshotsAsync(tenantId, AiFeatureGroup.KitchenLoad, today, ct);

        foreach (var d in data)
        {
            var json = JsonSerializer.Serialize(new { d.TicketCount, d.AvgPrepMinutes, windowDays = 30 });
            _context.FeatureStoreSnapshots.Add(FeatureStoreSnapshot.Create(
                tenantId, AiFeatureGroup.KitchenLoad, FeatureEntityType.Branch, d.BranchId, today, json));
        }

        await _context.SaveChangesAsync(ct);
        return data.Count;
    }

    private async Task RemoveSnapshotsAsync(Guid tenantId, AiFeatureGroup group, DateOnly asOfDate, CancellationToken ct)
    {
        var existing = await _context.FeatureStoreSnapshots
            .Where(f => f.TenantId == tenantId && f.FeatureGroup == group && f.AsOfDate == asOfDate)
            .ToListAsync(ct);
        _context.FeatureStoreSnapshots.RemoveRange(existing);
    }

    private async Task UpsertLineageAsync(Guid tenantId, AiFeatureGroup group, int recordCount, CancellationToken ct)
    {
        var source = group switch
        {
            AiFeatureGroup.SalesVelocity or AiFeatureGroup.Seasonality => "SalesDailySnapshots",
            AiFeatureGroup.StockTurnover => "InventoryDailySnapshots",
            AiFeatureGroup.CustomerRfm => "SalesOrders,Customers",
            AiFeatureGroup.KitchenLoad => "KitchenTickets",
            _ => "Unknown"
        };

        var lineage = await _context.FeatureLineages
            .FirstOrDefaultAsync(l => l.TenantId == tenantId && l.FeatureGroup == group, ct);

        var score = recordCount > 0 ? Math.Min(100, recordCount * 10) : 0;

        if (lineage is null)
        {
            _context.FeatureLineages.Add(FeatureLineage.Create(tenantId, group, source, score, recordCount));
        }
        else
        {
            lineage.Refresh(score, recordCount);
            _context.FeatureLineages.Update(lineage);
        }

        await _context.SaveChangesAsync(ct);
    }
}

public sealed class MlDatasetBuilderService : IMlDatasetBuilderService
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorage _fileStorage;

    public MlDatasetBuilderService(IApplicationDbContext context, IFileStorage fileStorage)
        => (_context, _fileStorage) = (context, fileStorage);

    public async Task<MlDatasetDefinitionDto> CreateDefinitionAsync(
        Guid tenantId, CreateDatasetDefinitionDto dto, CancellationToken ct = default)
    {
        var def = MlDatasetDefinition.Create(tenantId, dto.Name, dto.Description, dto.PrimaryFeatureGroup, dto.SpecJson);
        _context.MlDatasetDefinitions.Add(def);
        await _context.SaveChangesAsync(ct);
        return MapDef(def);
    }

    public async Task<IReadOnlyList<MlDatasetDefinitionDto>> GetDefinitionsAsync(Guid tenantId, CancellationToken ct = default)
        => await _context.MlDatasetDefinitions.AsNoTracking()
            .Where(d => d.TenantId == tenantId && d.IsActive)
            .Select(d => new MlDatasetDefinitionDto(
                d.Id, d.Name, d.Description, d.PrimaryFeatureGroup, d.IsActive, d.CreatedAt))
            .ToListAsync(ct);

    public async Task<MlDatasetExportDto> BuildAsync(Guid tenantId, BuildDatasetDto dto, CancellationToken ct = default)
    {
        var def = await _context.MlDatasetDefinitions.AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == dto.DefinitionId && d.TenantId == tenantId, ct)
            ?? throw new InvalidOperationException("Dataset definition not found.");

        var export = MlDatasetExport.Create(tenantId, def.Id, dto.Format, dto.Split);
        export.Start();
        _context.MlDatasetExports.Add(export);
        await _context.SaveChangesAsync(ct);

        try
        {
            var snapshots = await _context.FeatureStoreSnapshots.AsNoTracking()
                .Where(f => f.TenantId == tenantId && f.FeatureGroup == def.PrimaryFeatureGroup)
                .ToListAsync(ct);

            var rows = ApplySplit(snapshots, dto.Split, dto.TrainRatio, dto.ValidationRatio);
            var content = dto.Format == MlDatasetFormat.Json
                ? JsonSerializer.Serialize(rows.Select(r => new { r.EntityId, r.FeatureGroup, r.FeaturesJson, r.AsOfDate }))
                : BuildCsv(rows);

            var fileName = $"ai-datasets/{tenantId}/{def.Name}_{dto.Split}_{DateTime.UtcNow:yyyyMMddHHmmss}.{(dto.Format == MlDatasetFormat.Json ? "json" : "csv")}";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var path = await _fileStorage.UploadAsync(fileName, stream, dto.Format == MlDatasetFormat.Json ? "application/json" : "text/csv", ct);

            export.Complete(rows.Count, path);
        }
        catch (Exception ex)
        {
            export.Fail(ex.Message);
        }

        _context.MlDatasetExports.Update(export);
        await _context.SaveChangesAsync(ct);
        return MapExport(export);
    }

    public async Task<IReadOnlyList<MlDatasetExportDto>> GetExportsAsync(
        Guid tenantId, Guid definitionId, CancellationToken ct = default)
        => await _context.MlDatasetExports.AsNoTracking()
            .Where(e => e.TenantId == tenantId && e.DatasetDefinitionId == definitionId)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new MlDatasetExportDto(
                e.Id, e.DatasetDefinitionId, e.Format, e.Split, e.Status,
                e.RowCount, e.ContentPath, e.CreatedAt, e.CompletedAt, e.ErrorMessage))
            .ToListAsync(ct);

    private static string BuildCsv(List<FeatureStoreSnapshot> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("entity_id,feature_group,as_of_date,features_json");
        foreach (var r in rows)
            sb.AppendLine($"{r.EntityId},{r.FeatureGroup},{r.AsOfDate:yyyy-MM-dd},\"{r.FeaturesJson.Replace("\"", "\"\"")}\"");
        return sb.ToString();
    }

    private static List<FeatureStoreSnapshot> ApplySplit(
        List<FeatureStoreSnapshot> all, MlDatasetSplit split, double trainRatio, double validationRatio)
    {
        if (split == MlDatasetSplit.Full) return all;
        var ordered = all.OrderBy(x => x.EntityId).ToList();
        var trainEnd = (int)(ordered.Count * trainRatio);
        var valEnd = trainEnd + (int)(ordered.Count * validationRatio);
        return split switch
        {
            MlDatasetSplit.Train => ordered.Take(trainEnd).ToList(),
            MlDatasetSplit.Validation => ordered.Skip(trainEnd).Take(valEnd - trainEnd).ToList(),
            MlDatasetSplit.Test => ordered.Skip(valEnd).ToList(),
            _ => ordered
        };
    }

    private static MlDatasetDefinitionDto MapDef(MlDatasetDefinition d) =>
        new(d.Id, d.Name, d.Description, d.PrimaryFeatureGroup, d.IsActive, d.CreatedAt);

    private static MlDatasetExportDto MapExport(MlDatasetExport e) =>
        new(e.Id, e.DatasetDefinitionId, e.Format, e.Split, e.Status,
            e.RowCount, e.ContentPath, e.CreatedAt, e.CompletedAt, e.ErrorMessage);
}

public sealed class AiDataJobExecutor : IAiDataJobExecutor
{
    private readonly IDataWarehouseSyncService _warehouse;
    private readonly IFeatureComputationService _features;
    private readonly IDataQualityService _quality;
    private readonly IAiForecastOrchestrator _forecasts;
    private readonly IRecommendationActionService _recommendations;

    public AiDataJobExecutor(
        IDataWarehouseSyncService warehouse, IFeatureComputationService features,
        IDataQualityService quality, IAiForecastOrchestrator forecasts,
        IRecommendationActionService recommendations)
        => (_warehouse, _features, _quality, _forecasts, _recommendations) = (warehouse, features, quality, forecasts, recommendations);

    public async Task SyncWarehouseAsync(Guid tenantId, CancellationToken ct = default)
        => await _warehouse.SyncAsync(tenantId, ct: ct);

    public async Task ComputeFeaturesAsync(Guid tenantId, CancellationToken ct = default)
        => await _features.ComputeAllAsync(tenantId, ct);

    public async Task EvaluateDataQualityAsync(Guid tenantId, CancellationToken ct = default)
        => await _quality.EvaluateAsync(tenantId, ct);

    public async Task RefreshForecastsAsync(Guid tenantId, CancellationToken ct = default)
        => await _forecasts.RefreshAllAsync(tenantId, new DTOs.RefreshForecastsDto(), ct);

    public async Task RefreshRecommendationsAsync(Guid tenantId, CancellationToken ct = default)
        => await _recommendations.PersistRecommendationsAsync(tenantId, new DTOs.RefreshRecommendationsDto(), ct);
}
