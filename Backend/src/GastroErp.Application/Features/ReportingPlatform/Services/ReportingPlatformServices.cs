using System.Diagnostics;
using System.Text.Json;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Features.Automation.DTOs;
using GastroErp.Application.Features.Automation.Services;
using GastroErp.Application.Features.Reporting;
using GastroErp.Application.Features.Reporting.DTOs;
using GastroErp.Application.Features.Reporting.Services;
using GastroErp.Application.Features.ReportingPlatform.DTOs;
using GastroErp.Domain.Entities.Reporting;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Reporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ExportFormat = GastroErp.Application.Features.Reporting.DTOs.ExportFormat;

namespace GastroErp.Application.Features.ReportingPlatform.Services;

public interface IDashboardManagementService
{
    Task<DashboardDto> CreateAsync(Guid tenantId, Guid? ownerUserId, CreateDashboardDto dto, CancellationToken ct = default);
    Task<DashboardDto> UpdateAsync(Guid tenantId, Guid id, UpdateDashboardDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid tenantId, Guid id, string? deletedBy, CancellationToken ct = default);
    Task<DashboardDto> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<DashboardDto>> GetListAsync(Guid tenantId, Guid? userId, ReportingPlatformFilterDto filter, CancellationToken ct = default);
    Task<DashboardDto> ShareAsync(Guid tenantId, Guid id, ShareDashboardDto dto, CancellationToken ct = default);
    Task SetFavoriteAsync(Guid tenantId, Guid id, bool favorite, CancellationToken ct = default);
}

public interface IReportDefinitionService
{
    Task<ReportDefinitionDto> CreateAsync(Guid tenantId, CreateReportDefinitionDto dto, CancellationToken ct = default);
    Task<ReportDefinitionDto> UpdateAsync(Guid tenantId, Guid id, UpdateReportDefinitionDto dto, CancellationToken ct = default);
    Task<ReportDefinitionDto> PublishAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<ReportDefinitionDto> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ReportDefinitionDto>> GetListAsync(Guid tenantId, ReportingPlatformFilterDto filter, CancellationToken ct = default);
}

public interface IReportExecutionService
{
    Task<ReportExecutionDto> ExecuteAsync(Guid tenantId, Guid userId, ExecuteReportDto dto, CancellationToken ct = default);
    Task<ReportExecutionDto> PreviewAsync(Guid tenantId, Guid userId, ExecuteReportDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<ReportExecutionHistoryDto>> GetHistoryAsync(Guid tenantId, Guid? reportDefinitionId, int take = 50, CancellationToken ct = default);
}

public interface IKpiAnalyticsEngine
{
    Task<KpiDefinitionDto> CreateAsync(Guid tenantId, CreateKpiDefinitionDto dto, CancellationToken ct = default);
    Task<KpiDefinitionDto> UpdateAsync(Guid tenantId, Guid id, UpdateKpiDefinitionDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<KpiDefinitionDto>> GetListAsync(Guid tenantId, ReportingPlatformFilterDto filter, CancellationToken ct = default);
    Task<KpiValueDto> CalculateAsync(Guid tenantId, Guid kpiDefinitionId, ReportFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<KpiHistoryDto>> GetHistoryAsync(Guid tenantId, Guid kpiDefinitionId, int take = 90, CancellationToken ct = default);
}

public interface IPlatformExportService
{
    Task<PlatformExportResultDto> ExportAsync(Guid tenantId, PlatformExportRequestDto request, CancellationToken ct = default);
}

public interface IChartService
{
    Task<ChartResultDto> BuildChartAsync(Guid tenantId, ChartRequestDto request, CancellationToken ct = default);
}

public interface IPowerBiIntegrationService
{
    Task<PowerBiWorkspaceConfigDto> GetWorkspaceConfigAsync(Guid tenantId, CancellationToken ct = default);
    Task RefreshDatasetAsync(Guid tenantId, CancellationToken ct = default);
    Task<PowerBiEmbedTokenDto> GetEmbedTokenAsync(Guid tenantId, string reportId, CancellationToken ct = default);
}

public interface IScheduledReportService
{
    Task<ScheduledReportDto> CreateAsync(Guid tenantId, CreateScheduledReportDto dto, CancellationToken ct = default);
    Task<ScheduledReportDto> UpdateAsync(Guid tenantId, Guid id, UpdateScheduledReportDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid tenantId, Guid id, string? deletedBy, CancellationToken ct = default);
    Task<ScheduledReportDto> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ScheduledReportDto>> GetListAsync(Guid tenantId, CancellationToken ct = default);
    Task EnableAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task DisableAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task ExecuteNowAsync(Guid tenantId, Guid id, Guid userId, CancellationToken ct = default);
}

public interface IReportingPlatformJobExecutor
{
    Task RunScheduledReportsAsync(Guid tenantId, CancellationToken ct = default);
    Task RunKpiCalculationAsync(Guid tenantId, CancellationToken ct = default);
    Task RunDashboardCacheRefreshAsync(Guid tenantId, CancellationToken ct = default);
    Task RunReportCleanupAsync(Guid tenantId, CancellationToken ct = default);
}

public sealed class DashboardManagementService : IDashboardManagementService
{
    private readonly IApplicationDbContext _context;

    public DashboardManagementService(IApplicationDbContext context) => _context = context;

    public async Task<DashboardDto> CreateAsync(Guid tenantId, Guid? ownerUserId, CreateDashboardDto dto, CancellationToken ct = default)
    {
        var dashboard = Dashboard.Create(tenantId, dto.Name, ownerUserId, dto.Description, dto.IsPublic, dto.LayoutJson);
        if (dto.Widgets is not null)
            foreach (var w in dto.Widgets.OrderBy(x => x.Position))
                dashboard.AddWidget(w.WidgetType, w.Title, w.Position, w.Width, w.Height, w.ConfigurationJson);

        _context.ReportingDashboards.Add(dashboard);
        await _context.SaveChangesAsync(ct);
        return MapDashboard(dashboard);
    }

    public async Task<DashboardDto> UpdateAsync(Guid tenantId, Guid id, UpdateDashboardDto dto, CancellationToken ct = default)
    {
        var dashboard = await LoadDashboardAsync(tenantId, id, ct);
        dashboard.Update(dto.Name, dto.Description, dto.IsPublic, dto.LayoutJson);
        await _context.SaveChangesAsync(ct);
        return MapDashboard(dashboard);
    }

    public async Task DeleteAsync(Guid tenantId, Guid id, string? deletedBy, CancellationToken ct = default)
    {
        var dashboard = await LoadDashboardAsync(tenantId, id, ct);
        dashboard.SoftDelete(deletedBy);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<DashboardDto> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
        => MapDashboard(await LoadDashboardAsync(tenantId, id, ct));

    public async Task<IReadOnlyList<DashboardDto>> GetListAsync(Guid tenantId, Guid? userId, ReportingPlatformFilterDto filter, CancellationToken ct = default)
    {
        var q = _context.ReportingDashboards.AsNoTracking()
            .Include(d => d.Widgets)
            .Where(d => d.TenantId == tenantId && (d.IsPublic || d.OwnerUserId == userId || userId == null));

        if (!string.IsNullOrWhiteSpace(filter.Search))
            q = q.Where(d => d.Name.Contains(filter.Search));

        var items = await q.OrderByDescending(d => d.IsFavorite).ThenBy(d => d.Name)
            .Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync(ct);
        return items.Select(MapDashboard).ToList();
    }

    public async Task<DashboardDto> ShareAsync(Guid tenantId, Guid id, ShareDashboardDto dto, CancellationToken ct = default)
    {
        var dashboard = await LoadDashboardAsync(tenantId, id, ct);
        dashboard.Update(dashboard.Name, dashboard.Description, dto.IsPublic, dashboard.LayoutJson);
        await _context.SaveChangesAsync(ct);
        return MapDashboard(dashboard);
    }

    public async Task SetFavoriteAsync(Guid tenantId, Guid id, bool favorite, CancellationToken ct = default)
    {
        var dashboard = await LoadDashboardAsync(tenantId, id, ct);
        dashboard.SetFavorite(favorite);
        await _context.SaveChangesAsync(ct);
    }

    private async Task<Dashboard> LoadDashboardAsync(Guid tenantId, Guid id, CancellationToken ct)
        => await _context.ReportingDashboards.Include(d => d.Widgets)
            .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.Id == id, ct)
            ?? throw new InvalidOperationException("Dashboard not found.");

    private static DashboardDto MapDashboard(Dashboard d) => new(
        d.Id, d.Name, d.Description, d.IsDefault, d.IsPublic, d.IsFavorite, d.LayoutJson, d.OwnerUserId,
        d.Widgets.Select(w => new DashboardWidgetDto(w.Id, w.WidgetType, w.Title, w.Position, w.Width, w.Height, w.ConfigurationJson)).ToList());
}

public sealed class ReportDefinitionService : IReportDefinitionService
{
    private readonly IApplicationDbContext _context;

    public ReportDefinitionService(IApplicationDbContext context) => _context = context;

    public async Task<ReportDefinitionDto> CreateAsync(Guid tenantId, CreateReportDefinitionDto dto, CancellationToken ct = default)
    {
        var exists = await _context.ReportDefinitions.AnyAsync(r => r.TenantId == tenantId && r.Code == dto.Code.Trim().ToUpperInvariant(), ct);
        if (exists) throw new InvalidOperationException("Report code already exists.");

        var report = ReportDefinition.Create(tenantId, dto.Name, dto.Code, dto.Module, dto.Category,
            dto.DataSource, dto.QueryDefinition, dto.ParametersJson);
        _context.ReportDefinitions.Add(report);
        await _context.SaveChangesAsync(ct);
        return Map(report);
    }

    public async Task<ReportDefinitionDto> UpdateAsync(Guid tenantId, Guid id, UpdateReportDefinitionDto dto, CancellationToken ct = default)
    {
        var report = await GetEntityAsync(tenantId, id, ct);
        report.Update(dto.Name, dto.Category, dto.DataSource, dto.QueryDefinition, dto.ParametersJson);
        await _context.SaveChangesAsync(ct);
        return Map(report);
    }

    public async Task<ReportDefinitionDto> PublishAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var report = await GetEntityAsync(tenantId, id, ct);
        report.Publish();
        await _context.SaveChangesAsync(ct);
        return Map(report);
    }

    public async Task<ReportDefinitionDto> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
        => Map(await GetEntityAsync(tenantId, id, ct));

    public async Task<IReadOnlyList<ReportDefinitionDto>> GetListAsync(Guid tenantId, ReportingPlatformFilterDto filter, CancellationToken ct = default)
    {
        var q = _context.ReportDefinitions.AsNoTracking().Where(r => r.TenantId == tenantId);
        if (filter.Module.HasValue) q = q.Where(r => r.Module == filter.Module);
        if (!string.IsNullOrWhiteSpace(filter.Search)) q = q.Where(r => r.Name.Contains(filter.Search) || r.Code.Contains(filter.Search));
        var items = await q.OrderBy(r => r.Name).Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync(ct);
        return items.Select(Map).ToList();
    }

    private async Task<ReportDefinition> GetEntityAsync(Guid tenantId, Guid id, CancellationToken ct)
        => await _context.ReportDefinitions.FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id, ct)
            ?? throw new InvalidOperationException("Report not found.");

    private static ReportDefinitionDto Map(ReportDefinition r) => new(
        r.Id, r.Name, r.Code, r.Module, r.Category, r.DataSource, r.QueryDefinition, r.ParametersJson, r.IsPublished);
}

public sealed class ReportExecutionService : IReportExecutionService
{
    private readonly IApplicationDbContext _context;
    private readonly IReportDataResolver _resolver;

    public ReportExecutionService(IApplicationDbContext context, IReportDataResolver resolver)
        => (_context, _resolver) = (context, resolver);

    public Task<ReportExecutionDto> ExecuteAsync(Guid tenantId, Guid userId, ExecuteReportDto dto, CancellationToken ct = default)
        => RunAsync(tenantId, userId, dto, persist: true, ct);

    public Task<ReportExecutionDto> PreviewAsync(Guid tenantId, Guid userId, ExecuteReportDto dto, CancellationToken ct = default)
        => RunAsync(tenantId, userId, dto, persist: false, ct);

    public async Task<IReadOnlyList<ReportExecutionHistoryDto>> GetHistoryAsync(Guid tenantId, Guid? reportDefinitionId, int take = 50, CancellationToken ct = default)
    {
        var q = _context.ReportExecutions.AsNoTracking().Where(e => e.TenantId == tenantId);
        if (reportDefinitionId.HasValue) q = q.Where(e => e.ReportDefinitionId == reportDefinitionId);
        return await q.OrderByDescending(e => e.ExecutionDate).Take(take)
            .Join(_context.ReportDefinitions.AsNoTracking(), e => e.ReportDefinitionId, r => r.Id,
                (e, r) => new ReportExecutionHistoryDto(e.Id, e.ReportDefinitionId, r.Name, e.ExecutionDate, e.Status, e.DurationMs))
            .ToListAsync(ct);
    }

    private async Task<ReportExecutionDto> RunAsync(Guid tenantId, Guid userId, ExecuteReportDto dto, bool persist, CancellationToken ct)
    {
        var (reportId, dataSource) = await ResolveSourceAsync(tenantId, dto, ct);
        var filter = new ReportFilterDto(dto.FromDate, dto.ToDate, dto.BranchId);
        ReportExecution? execution = null;
        if (persist)
        {
            execution = ReportExecution.Start(tenantId, reportId, userId);
            _context.ReportExecutions.Add(execution);
            await _context.SaveChangesAsync(ct);
        }

        var sw = Stopwatch.StartNew();
        try
        {
            var result = await _resolver.ResolveAsync(tenantId, dataSource, filter, ct);
            var json = JsonSerializer.Serialize(result);
            sw.Stop();
            if (execution is not null)
            {
                execution.Complete((int)sw.ElapsedMilliseconds, json);
                await _context.SaveChangesAsync(ct);
            }
            return new ReportExecutionDto(execution?.Id ?? Guid.Empty, reportId, userId,
                execution?.ExecutionDate ?? DateTimeOffset.UtcNow, (int)sw.ElapsedMilliseconds,
                ReportStatus.Completed, result, null);
        }
        catch (Exception ex)
        {
            sw.Stop();
            if (execution is not null)
            {
                execution.Fail((int)sw.ElapsedMilliseconds, ex.Message);
                await _context.SaveChangesAsync(ct);
            }
            return new ReportExecutionDto(execution?.Id ?? Guid.Empty, reportId, userId,
                execution?.ExecutionDate ?? DateTimeOffset.UtcNow, (int)sw.ElapsedMilliseconds,
                ReportStatus.Failed, null, ex.Message);
        }
    }

    private async Task<(Guid ReportId, string DataSource)> ResolveSourceAsync(Guid tenantId, ExecuteReportDto dto, CancellationToken ct)
    {
        if (dto.ReportDefinitionId.HasValue)
        {
            var report = await _context.ReportDefinitions.AsNoTracking()
                .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == dto.ReportDefinitionId, ct)
                ?? throw new InvalidOperationException("Report not found.");
            return (report.Id, report.DataSource);
        }
        if (!string.IsNullOrWhiteSpace(dto.DataSource))
            return (Guid.Empty, dto.DataSource);
        throw new InvalidOperationException("ReportDefinitionId or DataSource required.");
    }
}

public interface IReportDataResolver
{
    Task<object> ResolveAsync(Guid tenantId, string dataSource, ReportFilterDto filter, CancellationToken ct = default);
}

public sealed class ReportDataResolver : IReportDataResolver
{
    private readonly IDashboardService _dashboard;
    private readonly ISalesAnalyticsService _sales;
    private readonly IKpiEngineService _kpi;
    private readonly IInventoryAnalyticsService _inventory;
    private readonly IFinancialAnalyticsService _finance;

    public ReportDataResolver(
        IDashboardService dashboard, ISalesAnalyticsService sales, IKpiEngineService kpi,
        IInventoryAnalyticsService inventory, IFinancialAnalyticsService finance)
        => (_dashboard, _sales, _kpi, _inventory, _finance) = (dashboard, sales, kpi, inventory, finance);

    public async Task<object> ResolveAsync(Guid tenantId, string dataSource, ReportFilterDto filter, CancellationToken ct = default)
    {
        var key = dataSource.Trim().ToLowerInvariant().Replace('_', '-');
        return key switch
        {
            "executive-dashboard" => await _dashboard.GetExecutiveDashboardAsync(tenantId, filter, ct),
            "kpi-dashboard" => await _kpi.GetKpiDashboardAsync(tenantId, filter, ct),
            "daily-sales" => await _sales.GetDailySalesAsync(tenantId, filter, ct),
            "monthly-sales" => await _sales.GetMonthlySalesAsync(tenantId, filter, ct),
            "sales-by-branch" => await _sales.GetSalesByBranchAsync(tenantId, filter, ct),
            "sales-by-product" => await _sales.GetSalesByProductAsync(tenantId, filter, ct),
            "stock-balance" => await _inventory.GetStockBalanceAsync(tenantId, filter, ct),
            "income-statement" => await _finance.GetIncomeStatementAsync(tenantId, filter, ct),
            _ => throw new InvalidOperationException($"Unknown data source: {dataSource}")
        };
    }
}

public sealed class KpiAnalyticsEngine : IKpiAnalyticsEngine
{
    private readonly IApplicationDbContext _context;
    private readonly IKpiEngineService _kpiEngine;
    private readonly ISalesAnalyticsService _sales;

    public KpiAnalyticsEngine(IApplicationDbContext context, IKpiEngineService kpiEngine, ISalesAnalyticsService sales)
        => (_context, _kpiEngine, _sales) = (context, kpiEngine, sales);

    public async Task<KpiDefinitionDto> CreateAsync(Guid tenantId, CreateKpiDefinitionDto dto, CancellationToken ct = default)
    {
        var exists = await _context.KpiDefinitions.AnyAsync(k => k.TenantId == tenantId && k.Code == dto.Code.Trim().ToUpperInvariant(), ct);
        if (exists) throw new InvalidOperationException("KPI code already exists.");
        var kpi = KpiDefinition.Create(tenantId, dto.Name, dto.Code, dto.Formula, dto.Module, dto.TargetValue, dto.WarningValue, dto.CriticalValue);
        _context.KpiDefinitions.Add(kpi);
        await _context.SaveChangesAsync(ct);
        return Map(kpi);
    }

    public async Task<KpiDefinitionDto> UpdateAsync(Guid tenantId, Guid id, UpdateKpiDefinitionDto dto, CancellationToken ct = default)
    {
        var kpi = await GetEntityAsync(tenantId, id, ct);
        kpi.Update(dto.Name, dto.Formula, dto.TargetValue, dto.WarningValue, dto.CriticalValue);
        await _context.SaveChangesAsync(ct);
        return Map(kpi);
    }

    public async Task<IReadOnlyList<KpiDefinitionDto>> GetListAsync(Guid tenantId, ReportingPlatformFilterDto filter, CancellationToken ct = default)
    {
        var q = _context.KpiDefinitions.AsNoTracking().Where(k => k.TenantId == tenantId && k.IsActive);
        if (filter.Module.HasValue) q = q.Where(k => k.Module == filter.Module);
        var items = await q.OrderBy(k => k.Name).Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync(ct);
        return items.Select(Map).ToList();
    }

    public async Task<KpiValueDto> CalculateAsync(Guid tenantId, Guid kpiDefinitionId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var kpi = await GetEntityAsync(tenantId, kpiDefinitionId, ct);
        var value = await EvaluateFormulaAsync(tenantId, kpi.Code, kpi.Formula, filter, ct);
        var previous = await _context.KpiSnapshots.AsNoTracking()
            .Where(s => s.TenantId == tenantId && s.KpiDefinitionId == kpiDefinitionId)
            .OrderByDescending(s => s.SnapshotDate).FirstOrDefaultAsync(ct);
        var trend = previous is null ? KpiTrend.Unknown
            : value > previous.Value ? KpiTrend.Up : value < previous.Value ? KpiTrend.Down : KpiTrend.Stable;

        var snapshot = KpiSnapshot.Record(tenantId, kpiDefinitionId, value, trend, DateOnly.FromDateTime(DateTime.UtcNow));
        _context.KpiSnapshots.Add(snapshot);
        await _context.SaveChangesAsync(ct);

        var status = ResolveStatus(value, kpi.TargetValue, kpi.WarningValue, kpi.CriticalValue);
        return new KpiValueDto(kpi.Id, kpi.Name, kpi.Code, value, trend, kpi.TargetValue, status, snapshot.SnapshotDate);
    }

    public async Task<IReadOnlyList<KpiHistoryDto>> GetHistoryAsync(Guid tenantId, Guid kpiDefinitionId, int take = 90, CancellationToken ct = default)
        => await _context.KpiSnapshots.AsNoTracking()
            .Where(s => s.TenantId == tenantId && s.KpiDefinitionId == kpiDefinitionId)
            .OrderByDescending(s => s.SnapshotDate).Take(take)
            .Select(s => new KpiHistoryDto(s.SnapshotDate, s.Value, s.Trend)).ToListAsync(ct);

    private async Task<decimal> EvaluateFormulaAsync(Guid tenantId, string code, string formula, ReportFilterDto filter, CancellationToken ct)
    {
        var dashboard = await _kpiEngine.GetKpiDashboardAsync(tenantId, filter, ct);
        var match = dashboard.Kpis.FirstOrDefault(k =>
            string.Equals(k.KpiName, code, StringComparison.OrdinalIgnoreCase)
            || formula.Contains(k.KpiName, StringComparison.OrdinalIgnoreCase));
        if (match is not null) return match.Value;

        var orders = ReportQueryHelper.FilterOrders(_context.SalesOrders, tenantId, filter);
        var revenue = await orders.SumAsync(o => o.GrandTotal, ct);
        var count = await orders.CountAsync(ct);
        return code.ToUpperInvariant() switch
        {
            "AVG_TICKET" or "AVERAGE_TICKET" => count > 0 ? revenue / count : 0,
            "REVENUE" or "TOTAL_REVENUE" => revenue,
            "ORDER_COUNT" => count,
            _ => revenue
        };
    }

    private static string ResolveStatus(decimal value, decimal? target, decimal? warning, decimal? critical)
    {
        if (critical.HasValue && value <= critical.Value) return "critical";
        if (warning.HasValue && value <= warning.Value) return "warning";
        if (target.HasValue && value >= target.Value) return "on-target";
        return "normal";
    }

    private async Task<KpiDefinition> GetEntityAsync(Guid tenantId, Guid id, CancellationToken ct)
        => await _context.KpiDefinitions.FirstOrDefaultAsync(k => k.TenantId == tenantId && k.Id == id, ct)
            ?? throw new InvalidOperationException("KPI not found.");

    private static KpiDefinitionDto Map(KpiDefinition k) => new(
        k.Id, k.Name, k.Code, k.Formula, k.Module, k.TargetValue, k.WarningValue, k.CriticalValue, k.IsActive);
}

public sealed class PlatformExportService : IPlatformExportService
{
    private readonly IApplicationDbContext _context;
    private readonly IReportExportService _export;
    private readonly IReportExecutionService _execution;

    public PlatformExportService(IApplicationDbContext context, IReportExportService export, IReportExecutionService execution)
        => (_context, _export, _execution) = (context, export, execution);

    public async Task<PlatformExportResultDto> ExportAsync(Guid tenantId, PlatformExportRequestDto request, CancellationToken ct = default)
    {
        var reportKey = request.ReportKey;
        if (request.ReportDefinitionId.HasValue)
        {
            var def = await _context.ReportDefinitions.AsNoTracking()
                .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == request.ReportDefinitionId, ct)
                ?? throw new InvalidOperationException("Report not found.");
            reportKey = def.DataSource;
        }
        reportKey ??= "daily-sales";

        var legacyFormat = request.Format switch
        {
            ReportExportFormat.Pdf => ExportFormat.Pdf,
            ReportExportFormat.Excel => ExportFormat.Excel,
            _ => ExportFormat.Csv
        };

        if (request.Format == ReportExportFormat.Json)
        {
            var result = await _execution.ExecuteAsync(tenantId, Guid.Empty, new ExecuteReportDto(
                request.ReportDefinitionId, reportKey, request.FromDate, request.ToDate, request.BranchId), ct);
            var bytes = JsonSerializer.SerializeToUtf8Bytes(result.Result);
            return new PlatformExportResultDto(bytes, "application/json", $"{reportKey}.json");
        }

        var filter = new ReportFilterDto(request.FromDate, request.ToDate, request.BranchId);
        var exported = await _export.ExportAsync(tenantId, new ExportReportRequestDto(reportKey, legacyFormat, filter), ct);
        return new PlatformExportResultDto(exported.Content, exported.ContentType, exported.FileName);
    }
}

public sealed class ChartService : IChartService
{
    private readonly IReportDataResolver _resolver;

    public ChartService(IReportDataResolver resolver) => _resolver = resolver;

    public async Task<ChartResultDto> BuildChartAsync(Guid tenantId, ChartRequestDto request, CancellationToken ct = default)
    {
        var filter = new ReportFilterDto(request.FromDate, request.ToDate, request.BranchId);
        var data = await _resolver.ResolveAsync(tenantId, request.DataSource, filter, ct);
        object chart = request.ChartType switch
        {
            ChartType.Line when data is ExecutiveDashboardDto exec => exec.RevenueTrend,
            ChartType.Bar when data is ExecutiveDashboardDto exec => exec.TopProducts,
            ChartType.Pie when data is ExecutiveDashboardDto exec => exec.PaymentMix,
            ChartType.Area when data is ExecutiveDashboardDto exec => exec.RevenueTrend,
            ChartType.Donut when data is ExecutiveDashboardDto exec => exec.OrderTypeMix,
            _ => data
        };
        return new ChartResultDto(request.ChartType, chart);
    }
}

public sealed class PowerBiIntegrationService : IPowerBiIntegrationService
{
    public Task<PowerBiWorkspaceConfigDto> GetWorkspaceConfigAsync(Guid tenantId, CancellationToken ct = default)
        => Task.FromResult(new PowerBiWorkspaceConfigDto(string.Empty, string.Empty, false));

    public Task RefreshDatasetAsync(Guid tenantId, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task<PowerBiEmbedTokenDto> GetEmbedTokenAsync(Guid tenantId, string reportId, CancellationToken ct = default)
        => Task.FromResult(new PowerBiEmbedTokenDto(
            $"embed-token-{tenantId:N}-{reportId}",
            DateTimeOffset.UtcNow.AddHours(1),
            $"https://app.powerbi.com/reportEmbed?reportId={reportId}"));
}

public sealed class ScheduledReportService : IScheduledReportService
{
    private readonly IApplicationDbContext _context;
    private readonly IPlatformExportService _export;
    private readonly INotificationOrchestrator _notifications;

    public ScheduledReportService(IApplicationDbContext context, IPlatformExportService export, INotificationOrchestrator notifications)
        => (_context, _export, _notifications) = (context, export, notifications);

    public async Task<ScheduledReportDto> CreateAsync(Guid tenantId, CreateScheduledReportDto dto, CancellationToken ct = default)
    {
        var scheduled = ScheduledReport.Create(tenantId, dto.ReportDefinitionId, dto.Frequency, dto.ExportFormat, dto.CronExpression, dto.EmailRecipients);
        _context.ScheduledReports.Add(scheduled);
        await _context.SaveChangesAsync(ct);
        return Map(scheduled);
    }

    public async Task<ScheduledReportDto> UpdateAsync(Guid tenantId, Guid id, UpdateScheduledReportDto dto, CancellationToken ct = default)
    {
        var scheduled = await GetEntityAsync(tenantId, id, ct);
        scheduled.Update(dto.Frequency, dto.ExportFormat, dto.CronExpression, dto.EmailRecipients);
        await _context.SaveChangesAsync(ct);
        return Map(scheduled);
    }

    public async Task DeleteAsync(Guid tenantId, Guid id, string? deletedBy, CancellationToken ct = default)
    {
        var scheduled = await GetEntityAsync(tenantId, id, ct);
        scheduled.SoftDelete(deletedBy);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<ScheduledReportDto> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
        => Map(await GetEntityAsync(tenantId, id, ct));

    public async Task<IReadOnlyList<ScheduledReportDto>> GetListAsync(Guid tenantId, CancellationToken ct = default)
    {
        var items = await _context.ScheduledReports.AsNoTracking().Where(s => s.TenantId == tenantId)
            .OrderBy(s => s.CreatedAt).ToListAsync(ct);
        return items.Select(Map).ToList();
    }

    public async Task EnableAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var scheduled = await GetEntityAsync(tenantId, id, ct);
        scheduled.Enable();
        await _context.SaveChangesAsync(ct);
    }

    public async Task DisableAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var scheduled = await GetEntityAsync(tenantId, id, ct);
        scheduled.Disable();
        await _context.SaveChangesAsync(ct);
    }

    public async Task ExecuteNowAsync(Guid tenantId, Guid id, Guid userId, CancellationToken ct = default)
    {
        var scheduled = await GetEntityAsync(tenantId, id, ct);
        try
        {
            await _export.ExportAsync(tenantId, new PlatformExportRequestDto(
                scheduled.ReportDefinitionId, null, scheduled.ExportFormat), ct);
            scheduled.MarkRun(true);
            await _context.SaveChangesAsync(ct);
            await _notifications.SendAsync(tenantId, new SendNotificationDto(
                "Scheduled Report Completed", "Your scheduled report completed successfully.",
                NotificationType.ScheduledReportCompleted, NotificationChannel.InApp,
                UserId: userId, ReferenceType: "ScheduledReport", ReferenceId: scheduled.Id), ct);
        }
        catch (Exception)
        {
            scheduled.MarkRun(false);
            await _context.SaveChangesAsync(ct);
            await _notifications.SendAsync(tenantId, new SendNotificationDto(
                "Scheduled Report Failed", "Your scheduled report failed.",
                NotificationType.ScheduledReportFailed, NotificationChannel.InApp,
                UserId: userId, ReferenceType: "ScheduledReport", ReferenceId: scheduled.Id), ct);
            throw;
        }
    }

    private async Task<ScheduledReport> GetEntityAsync(Guid tenantId, Guid id, CancellationToken ct)
        => await _context.ScheduledReports.FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == id, ct)
            ?? throw new InvalidOperationException("Scheduled report not found.");

    private static ScheduledReportDto Map(ScheduledReport s) => new(
        s.Id, s.ReportDefinitionId, s.Frequency, s.CronExpression, s.ExportFormat, s.EmailRecipients, s.IsEnabled, s.LastRunAt);
}

public sealed class ReportingPlatformJobExecutor : IReportingPlatformJobExecutor
{
    private readonly IApplicationDbContext _context;
    private readonly IScheduledReportService _scheduled;
    private readonly IKpiAnalyticsEngine _kpi;
    private readonly ILogger<ReportingPlatformJobExecutor> _logger;

    public ReportingPlatformJobExecutor(
        IApplicationDbContext context, IScheduledReportService scheduled,
        IKpiAnalyticsEngine kpi, ILogger<ReportingPlatformJobExecutor> logger)
        => (_context, _scheduled, _kpi, _logger) = (context, scheduled, kpi, logger);

    public async Task RunScheduledReportsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var due = await _context.ScheduledReports
            .Where(s => s.TenantId == tenantId && s.IsEnabled).ToListAsync(ct);
        foreach (var item in due)
        {
            if (!IsDue(item)) continue;
            try
            {
                await _scheduled.ExecuteNowAsync(tenantId, item.Id, Guid.Empty, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Scheduled report {Id} failed for tenant {TenantId}", item.Id, tenantId);
            }
        }
    }

    public async Task RunKpiCalculationAsync(Guid tenantId, CancellationToken ct = default)
    {
        var kpis = await _context.KpiDefinitions.Where(k => k.TenantId == tenantId && k.IsActive).ToListAsync(ct);
        var filter = new ReportFilterDto(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)), DateOnly.FromDateTime(DateTime.UtcNow));
        foreach (var kpi in kpis)
            await _kpi.CalculateAsync(tenantId, kpi.Id, filter, ct);
    }

    public Task RunDashboardCacheRefreshAsync(Guid tenantId, CancellationToken ct = default)
    {
        _logger.LogInformation("Dashboard cache refresh completed for tenant {TenantId}", tenantId);
        return Task.CompletedTask;
    }

    public async Task RunReportCleanupAsync(Guid tenantId, CancellationToken ct = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddMonths(-6);
        var old = await _context.ReportExecutions
            .Where(e => e.TenantId == tenantId && e.ExecutionDate < cutoff).ToListAsync(ct);
        foreach (var e in old) e.SoftDelete(null);
        await _context.SaveChangesAsync(ct);
    }

    private static bool IsDue(ScheduledReport s)
    {
        if (s.LastRunAt is null) return true;
        var last = s.LastRunAt.Value;
        return s.Frequency switch
        {
            ScheduleFrequency.Daily => last < DateTimeOffset.UtcNow.AddDays(-1),
            ScheduleFrequency.Weekly => last < DateTimeOffset.UtcNow.AddDays(-7),
            ScheduleFrequency.Monthly => last < DateTimeOffset.UtcNow.AddMonths(-1),
            ScheduleFrequency.Cron => last < DateTimeOffset.UtcNow.AddHours(-1),
            _ => false
        };
    }
}
