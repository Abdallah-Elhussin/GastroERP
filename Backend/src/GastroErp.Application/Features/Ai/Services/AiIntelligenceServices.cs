using System.Text.Json;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Interfaces.Ai;
using GastroErp.Application.Features.Ai.DTOs;
using GastroErp.Application.Features.Automation.DTOs;
using GastroErp.Application.Features.Automation.Services;
using GastroErp.Domain.Entities.Ai;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Ai.Services;

public interface IFraudDetectionService
{
    Task<FraudAnalysisResultDto> AnalyzeAsync(Guid tenantId, RefreshIntelligenceDto options, CancellationToken ct = default);
    Task<IReadOnlyList<FraudAlertDto>> GetAlertsAsync(Guid tenantId, IntelligenceFilterDto filter, CancellationToken ct = default);
}

public interface ICustomerSegmentationService
{
    Task<SegmentationResultDto> RefreshAsync(Guid tenantId, RefreshIntelligenceDto options, CancellationToken ct = default);
    Task<IReadOnlyList<CustomerSegmentDto>> GetSegmentsAsync(Guid tenantId, IntelligenceFilterDto filter, CancellationToken ct = default);
}

public interface IChurnPredictionService
{
    Task<ChurnAnalysisResultDto> RefreshAsync(Guid tenantId, RefreshIntelligenceDto options, CancellationToken ct = default);
    Task<IReadOnlyList<ChurnPredictionDto>> GetPredictionsAsync(Guid tenantId, IntelligenceFilterDto filter, CancellationToken ct = default);
}

public interface IRecommendationEngineService
{
    Task<ProductRecommendationResultDto> RefreshAsync(Guid tenantId, RefreshIntelligenceDto options, CancellationToken ct = default);
    Task<IReadOnlyList<ProductRecommendationDto>> GetRecommendationsAsync(Guid tenantId, IntelligenceFilterDto filter, CancellationToken ct = default);
}

public interface IIntelligenceDashboardService
{
    Task<IntelligenceDashboardDto> GetDashboardAsync(Guid tenantId, Guid? branchId = null, CancellationToken ct = default);
    Task<IntelligenceMonitoringDto> GetMonitoringAsync(CancellationToken ct = default);
}

public interface IAiIntelligenceJobExecutor
{
    Task RunFraudAnalysisAsync(Guid tenantId, CancellationToken ct = default);
    Task RunSegmentationAsync(Guid tenantId, CancellationToken ct = default);
    Task RunChurnPredictionAsync(Guid tenantId, CancellationToken ct = default);
    Task RunProductRecommendationsAsync(Guid tenantId, CancellationToken ct = default);
}

public sealed class FraudDetectionService : IFraudDetectionService
{
    private readonly IApplicationDbContext _context;
    private readonly IFraudAnalysisEngine _engine;
    private readonly INotificationOrchestrator _notifications;
    private readonly ILogger<FraudDetectionService> _logger;

    public FraudDetectionService(
        IApplicationDbContext context, IFraudAnalysisEngine engine,
        INotificationOrchestrator notifications, ILogger<FraudDetectionService> logger)
        => (_context, _engine, _notifications, _logger) = (context, engine, notifications, logger);

    public async Task<FraudAnalysisResultDto> AnalyzeAsync(
        Guid tenantId, RefreshIntelligenceDto options, CancellationToken ct = default)
    {
        var signals = await _engine.AnalyzeAsync(tenantId, options.LookbackDays, options.BranchId, ct);
        var alerts = new List<FraudAlert>();

        foreach (var signal in signals)
        {
            var alert = FraudAlert.Create(
                tenantId, signal.AlertType, signal.RiskScore, signal.Severity,
                signal.Source, signal.DetailsJson, signal.ReferenceType, signal.ReferenceId, signal.BranchId);
            alerts.Add(alert);
            _context.FraudAlerts.Add(alert);
        }

        await _context.SaveChangesAsync(ct);

        if (options.NotifyOnCritical)
        {
            foreach (var critical in alerts.Where(a => a.Severity == FraudSeverity.Critical))
            {
                await _notifications.SendAsync(tenantId, new SendNotificationDto(
                    "Critical Fraud Alert",
                    $"Fraud alert {critical.AlertType} detected with risk score {critical.RiskScore:F0}",
                    NotificationType.System, NotificationChannel.InApp), ct);
            }
        }

        var dtos = alerts.Select(MapFraud).ToList();
        return new FraudAnalysisResultDto(
            dtos, dtos.Count, dtos.Count(a => a.Severity == FraudSeverity.Critical), DateTimeOffset.UtcNow);
    }

    public async Task<IReadOnlyList<FraudAlertDto>> GetAlertsAsync(
        Guid tenantId, IntelligenceFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.FraudAlerts.AsNoTracking().Where(a => a.TenantId == tenantId);
        if (filter.BranchId.HasValue)
            query = query.Where(a => a.BranchId == filter.BranchId);
        if (filter.MinSeverity.HasValue)
            query = query.Where(a => a.Severity >= filter.MinSeverity);

        var items = await query
            .OrderByDescending(a => a.RiskScore)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return items.Select(MapFraud).ToList();
    }

    private static FraudAlertDto MapFraud(FraudAlert a) => new(
        a.Id, a.AlertType, a.RiskScore, a.Severity, a.Source, a.Status,
        a.ReferenceType, a.ReferenceId, a.BranchId, a.DetailsJson, a.CreatedAt);
}

public sealed class CustomerSegmentationService : ICustomerSegmentationService
{
    private readonly IApplicationDbContext _context;
    private readonly ICustomerSegmentationEngine _engine;
    private readonly INotificationOrchestrator _notifications;

    public CustomerSegmentationService(
        IApplicationDbContext context, ICustomerSegmentationEngine engine,
        INotificationOrchestrator notifications)
        => (_context, _engine, _notifications) = (context, engine, notifications);

    public async Task<SegmentationResultDto> RefreshAsync(
        Guid tenantId, RefreshIntelligenceDto options, CancellationToken ct = default)
    {
        var assignments = await _engine.SegmentAsync(tenantId, options.LookbackDays, options.BranchId, ct);
        var existing = await _context.CustomerSegments
            .Where(s => s.TenantId == tenantId)
            .ToDictionaryAsync(s => s.CustomerId, ct);

        var newVips = 0;
        foreach (var assignment in assignments)
        {
            if (existing.TryGetValue(assignment.CustomerId, out var segment))
            {
                var wasVip = segment.Segment == CustomerSegmentType.VIP;
                segment.Update(assignment.Segment, assignment.Score, assignment.MetricsJson);
                if (!wasVip && assignment.Segment == CustomerSegmentType.VIP)
                    newVips++;
            }
            else
            {
                _context.CustomerSegments.Add(CustomerSegment.Create(
                    tenantId, assignment.CustomerId, assignment.Segment, assignment.Score, assignment.MetricsJson));
                if (assignment.Segment == CustomerSegmentType.VIP)
                    newVips++;
            }
        }

        await _context.SaveChangesAsync(ct);

        if (options.NotifyOnCritical && newVips > 0)
        {
            await _notifications.SendAsync(tenantId, new SendNotificationDto(
                "New VIP Customers",
                $"{newVips} customer(s) upgraded to VIP segment",
                NotificationType.System, NotificationChannel.InApp), ct);
        }

        var all = await GetSegmentsAsync(tenantId, new IntelligenceFilterDto(PageSize: 1000), ct);
        var distribution = all.GroupBy(s => s.Segment).ToDictionary(g => g.Key, g => g.Count());
        return new SegmentationResultDto(all, distribution, DateTimeOffset.UtcNow);
    }

    public async Task<IReadOnlyList<CustomerSegmentDto>> GetSegmentsAsync(
        Guid tenantId, IntelligenceFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.CustomerSegments.AsNoTracking().Where(s => s.TenantId == tenantId);
        if (filter.Segment.HasValue)
            query = query.Where(s => s.Segment == filter.Segment);

        var segments = await query
            .OrderByDescending(s => s.Score)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        var customerIds = segments.Select(s => s.CustomerId).ToList();
        var customers = await _context.Customers.AsNoTracking()
            .Where(c => customerIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.FullName, ct);

        return segments.Select(s => new CustomerSegmentDto(
            s.Id, s.CustomerId, customers.GetValueOrDefault(s.CustomerId, "Unknown"),
            s.Segment, s.Score, s.MetricsJson, s.UpdatedAt ?? s.CreatedAt)).ToList();
    }
}

public sealed class ChurnPredictionService : IChurnPredictionService
{
    private readonly IApplicationDbContext _context;
    private readonly IChurnPredictionEngine _engine;
    private readonly INotificationOrchestrator _notifications;

    public ChurnPredictionService(
        IApplicationDbContext context, IChurnPredictionEngine engine,
        INotificationOrchestrator notifications)
        => (_context, _engine, _notifications) = (context, engine, notifications);

    public async Task<ChurnAnalysisResultDto> RefreshAsync(
        Guid tenantId, RefreshIntelligenceDto options, CancellationToken ct = default)
    {
        var scores = await _engine.PredictAsync(tenantId, options.LookbackDays, options.BranchId, ct);
        var existing = await _context.ChurnPredictions
            .Where(p => p.TenantId == tenantId)
            .ToDictionaryAsync(p => p.CustomerId, ct);

        var highRisk = 0;
        foreach (var score in scores)
        {
            if (existing.TryGetValue(score.CustomerId, out var prediction))
            {
                prediction.Refresh(score.ChurnProbability, score.RiskLevel, score.Recommendation, score.MetricsJson);
            }
            else
            {
                _context.ChurnPredictions.Add(ChurnPrediction.Create(
                    tenantId, score.CustomerId, score.ChurnProbability, score.RiskLevel,
                    score.Recommendation, score.MetricsJson));
            }

            if (score.RiskLevel is ChurnRiskLevel.High or ChurnRiskLevel.Critical)
                highRisk++;
        }

        await _context.SaveChangesAsync(ct);

        if (options.NotifyOnCritical && highRisk > 0)
        {
            await _notifications.SendAsync(tenantId, new SendNotificationDto(
                "High Churn Risk",
                $"{highRisk} customer(s) have high churn probability",
                NotificationType.System, NotificationChannel.InApp), ct);
        }

        var predictions = await GetPredictionsAsync(tenantId, new IntelligenceFilterDto(PageSize: 1000), ct);
        return new ChurnAnalysisResultDto(predictions, highRisk, DateTimeOffset.UtcNow);
    }

    public async Task<IReadOnlyList<ChurnPredictionDto>> GetPredictionsAsync(
        Guid tenantId, IntelligenceFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.ChurnPredictions.AsNoTracking().Where(p => p.TenantId == tenantId);
        if (filter.MinChurnRisk.HasValue)
            query = query.Where(p => p.RiskLevel >= filter.MinChurnRisk);

        var items = await query
            .OrderByDescending(p => p.ChurnProbability)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        var customerIds = items.Select(i => i.CustomerId).ToList();
        var customers = await _context.Customers.AsNoTracking()
            .Where(c => customerIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.FullName, ct);

        return items.Select(p => new ChurnPredictionDto(
            p.Id, p.CustomerId, customers.GetValueOrDefault(p.CustomerId, "Unknown"),
            p.ChurnProbability, p.RiskLevel, p.Recommendation, p.MetricsJson, p.GeneratedAt)).ToList();
    }
}

public sealed class RecommendationEngineService : IRecommendationEngineService
{
    private readonly IApplicationDbContext _context;
    private readonly IProductRecommendationEngine _engine;

    public RecommendationEngineService(IApplicationDbContext context, IProductRecommendationEngine engine)
        => (_context, _engine) = (context, engine);

    public async Task<ProductRecommendationResultDto> RefreshAsync(
        Guid tenantId, RefreshIntelligenceDto options, CancellationToken ct = default)
    {
        var signals = await _engine.GenerateAsync(tenantId, options.LookbackDays, options.BranchId, ct);

        var existing = await _context.ProductRecommendations
            .Where(r => r.TenantId == tenantId)
            .ToListAsync(ct);
        _context.ProductRecommendations.RemoveRange(existing);

        foreach (var signal in signals)
        {
            var relatedJson = JsonSerializer.Serialize(signal.RelatedProducts.Select(r => new
            {
                ProductId = r.ProductId,
                ProductName = r.Name,
                Confidence = r.Confidence
            }));

            _context.ProductRecommendations.Add(ProductRecommendation.Create(
                tenantId, signal.ProductId, signal.RecommendationType, signal.Confidence,
                relatedJson, signal.TargetCustomerId, signal.BranchId));
        }

        await _context.SaveChangesAsync(ct);
        var recs = await GetRecommendationsAsync(tenantId, new IntelligenceFilterDto(PageSize: 500), ct);
        return new ProductRecommendationResultDto(recs, DateTimeOffset.UtcNow);
    }

    public async Task<IReadOnlyList<ProductRecommendationDto>> GetRecommendationsAsync(
        Guid tenantId, IntelligenceFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.ProductRecommendations.AsNoTracking().Where(r => r.TenantId == tenantId);
        if (filter.BranchId.HasValue)
            query = query.Where(r => r.BranchId == filter.BranchId || r.BranchId == null);
        if (filter.RecommendationType.HasValue)
            query = query.Where(r => r.RecommendationType == filter.RecommendationType);

        var items = await query
            .OrderByDescending(r => r.Confidence)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        var productIds = items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _context.Products.AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.NameAr, ct);

        return items.Select(r =>
        {
            var related = JsonSerializer.Deserialize<List<RelatedProductDto>>(r.RelatedProductsJson) ?? [];
            return new ProductRecommendationDto(
                r.Id, r.ProductId, products.GetValueOrDefault(r.ProductId, "Unknown"),
                r.RecommendationType, r.Confidence, related,
                r.TargetCustomerId, r.BranchId, r.CreatedAt);
        }).ToList();
    }
}

public sealed class IntelligenceDashboardService : IIntelligenceDashboardService
{
    private readonly IApplicationDbContext _context;
    private readonly IScheduledJobCatalog _jobs;

    public IntelligenceDashboardService(IApplicationDbContext context, IScheduledJobCatalog jobs)
        => (_context, _jobs) = (context, jobs);

    public async Task<IntelligenceDashboardDto> GetDashboardAsync(
        Guid tenantId, Guid? branchId = null, CancellationToken ct = default)
    {
        var fraudQuery = _context.FraudAlerts.AsNoTracking().Where(a => a.TenantId == tenantId);
        if (branchId.HasValue)
            fraudQuery = fraudQuery.Where(a => a.BranchId == branchId);

        var openFraud = await fraudQuery.CountAsync(a => a.Status == FraudAlertStatus.Open, ct);
        var criticalFraud = await fraudQuery.CountAsync(a => a.Severity == FraudSeverity.Critical, ct);

        var segmentQuery = _context.CustomerSegments.AsNoTracking().Where(s => s.TenantId == tenantId);
        var vip = await segmentQuery.CountAsync(s => s.Segment == CustomerSegmentType.VIP, ct);
        var atRisk = await segmentQuery.CountAsync(s => s.Segment == CustomerSegmentType.AtRisk, ct);

        var churnQuery = _context.ChurnPredictions.AsNoTracking().Where(p => p.TenantId == tenantId);
        var highRisk = await churnQuery.CountAsync(p => p.RiskLevel >= ChurnRiskLevel.High, ct);
        var avgChurn = await churnQuery.AnyAsync(ct)
            ? await churnQuery.AverageAsync(p => p.ChurnProbability, ct) : 0;

        var recCount = await _context.ProductRecommendations.AsNoTracking()
            .CountAsync(r => r.TenantId == tenantId, ct);

        return new IntelligenceDashboardDto(
            openFraud, criticalFraud, highRisk, atRisk, vip, recCount,
            Math.Round(avgChurn, 2), DateTimeOffset.UtcNow);
    }

    public async Task<IntelligenceMonitoringDto> GetMonitoringAsync(CancellationToken ct = default)
    {
        var jobNames = _jobs.GetRegisteredJobNames()
            .Where(n => n.StartsWith("AiIntelligence", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var logs = await _context.JobExecutionLogs.AsNoTracking()
            .Where(j => jobNames.Contains(j.JobName))
            .GroupBy(j => j.JobName)
            .Select(g => new { JobName = g.Key, LastRun = g.Max(x => x.StartedAt) })
            .ToListAsync(ct);

        DateTimeOffset? GetLast(string name) =>
            logs.FirstOrDefault(l => l.JobName.Equals(name, StringComparison.OrdinalIgnoreCase))?.LastRun;

        return new IntelligenceMonitoringDto(
            jobNames,
            GetLast("AiIntelligenceFraudAnalysis"),
            GetLast("AiIntelligenceSegmentation"),
            GetLast("AiIntelligenceChurnPrediction"),
            GetLast("AiIntelligenceProductRecommendations"),
            true);
    }
}

public sealed class AiIntelligenceJobExecutor : IAiIntelligenceJobExecutor
{
    private readonly IFraudDetectionService _fraud;
    private readonly ICustomerSegmentationService _segments;
    private readonly IChurnPredictionService _churn;
    private readonly IRecommendationEngineService _recommendations;

    public AiIntelligenceJobExecutor(
        IFraudDetectionService fraud, ICustomerSegmentationService segments,
        IChurnPredictionService churn, IRecommendationEngineService recommendations)
        => (_fraud, _segments, _churn, _recommendations) = (fraud, segments, churn, recommendations);

    private static RefreshIntelligenceDto DefaultOptions() => new(NotifyOnCritical: true);

    public Task RunFraudAnalysisAsync(Guid tenantId, CancellationToken ct = default)
        => _fraud.AnalyzeAsync(tenantId, DefaultOptions(), ct);

    public Task RunSegmentationAsync(Guid tenantId, CancellationToken ct = default)
        => _segments.RefreshAsync(tenantId, DefaultOptions(), ct);

    public Task RunChurnPredictionAsync(Guid tenantId, CancellationToken ct = default)
        => _churn.RefreshAsync(tenantId, DefaultOptions(), ct);

    public Task RunProductRecommendationsAsync(Guid tenantId, CancellationToken ct = default)
        => _recommendations.RefreshAsync(tenantId, DefaultOptions(), ct);
}
