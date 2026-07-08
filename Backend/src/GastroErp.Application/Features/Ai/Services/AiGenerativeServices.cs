using System.Text.Json;
using System.Text.RegularExpressions;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Interfaces.Ai;
using GastroErp.Application.Features.Ai.DTOs;
using GastroErp.Application.Features.Reporting.DTOs;
using GastroErp.Application.Features.Reporting.Services;
using GastroErp.Domain.Entities.Ai;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Ai.Services;

internal static class GenerativeAiPrompts
{
    public static string ManagementSystem(string language) => language.StartsWith("ar", StringComparison.OrdinalIgnoreCase)
        ? "أنت مساعد إداري لنظام GastroERP. أجب باختصار وبيانات دقيقة من السياق المقدم."
        : "You are a GastroERP management assistant. Answer briefly using only the provided context data.";

    public static string InsightsSystem(string language) => language.StartsWith("ar", StringComparison.OrdinalIgnoreCase)
        ? "أنت محلل أعمال لمطعم. لخّص المؤشرات والتنبيهات بلغة واضحة."
        : "You are a restaurant business analyst. Summarize KPIs and alerts clearly.";

    public static string QuerySystem(string language) => language.StartsWith("ar", StringComparison.OrdinalIgnoreCase)
        ? "أنت مساعد استعلامات. اشرح نتائج البيانات فقط دون تعديل."
        : "You are a read-only query assistant. Explain data results without modifying anything.";
}

internal static class GenerativeIntentMatcher
{
    public static string Classify(string text)
    {
        var q = text.ToLowerInvariant();
        if (ContainsAny(q, "top product", "best seller", "أكثر", "منتج", "مبيع"))
            return "top_products";
        if (ContainsAny(q, "revenue", "sales today", "مبيعات", "إيراد", "دخل"))
            return "revenue";
        if (ContainsAny(q, "order", "طلب", "orders"))
            return "orders";
        if (ContainsAny(q, "stock", "inventory", "مخزون", "نفاد"))
            return "inventory";
        if (ContainsAny(q, "customer", "عميل", "زبون"))
            return "customers";
        if (ContainsAny(q, "forecast", "توقع", "تنبؤ"))
            return "forecast";
        return "general";
    }

    private static bool ContainsAny(string text, params string[] terms)
        => terms.Any(t => text.Contains(t, StringComparison.OrdinalIgnoreCase));
}

public interface IManagementAiAssistantService
{
    Task<AiChatResponseDto> ChatAsync(Guid tenantId, AiChatRequestDto request, Guid? userId = null, CancellationToken ct = default);
    IAsyncEnumerable<string> ChatStreamAsync(Guid tenantId, AiChatRequestDto request, Guid? userId = null, CancellationToken ct = default);
}

public interface IAiDashboardInsightsService
{
    Task<AiDashboardInsightDto> GetDashboardInsightsAsync(Guid tenantId, AiInsightFilterDto filter, CancellationToken ct = default);
}

public interface INaturalLanguageQueryService
{
    Task<NaturalLanguageQueryResultDto> QueryAsync(Guid tenantId, NaturalLanguageQueryDto request, Guid? userId = null, CancellationToken ct = default);
}

public interface IVoiceOrderingService
{
    Task<VoiceOrderDraftDto> ParseVoiceOrderAsync(Guid tenantId, VoiceOrderRequestDto request, Guid? userId = null, CancellationToken ct = default);
    Task<VoiceOrderDraftDto> ConfirmDraftAsync(Guid tenantId, ConfirmVoiceOrderDto dto, Guid? userId = null, CancellationToken ct = default);
}

public sealed class ManagementAiAssistantService : IManagementAiAssistantService
{
    private readonly IApplicationDbContext _context;
    private readonly IDashboardService _dashboard;
    private readonly IGenerativeAiAdapter _adapter;
    private readonly ILogger<ManagementAiAssistantService> _logger;

    public ManagementAiAssistantService(
        IApplicationDbContext context, IDashboardService dashboard,
        IGenerativeAiAdapter adapter, ILogger<ManagementAiAssistantService> logger)
        => (_context, _dashboard, _adapter, _logger) = (context, dashboard, adapter, logger);

    public async Task<AiChatResponseDto> ChatAsync(
        Guid tenantId, AiChatRequestDto request, Guid? userId = null, CancellationToken ct = default)
    {
        var context = await BuildContextAsync(tenantId, request, ct);
        var system = GenerativeAiPrompts.ManagementSystem(request.Language);
        var reply = await _adapter.GenerateAsync(system, $"{context}\n\nUser: {request.Message}", ct);
        var log = await PersistLogAsync(tenantId, GenerativeInteractionType.Chat, request.Message, reply,
            request.BranchId, userId, ct);
        return new AiChatResponseDto(reply, AiModelProvider.Heuristic, EstimateTokens(request.Message, reply), log.Id);
    }

    public async IAsyncEnumerable<string> ChatStreamAsync(
        Guid tenantId, AiChatRequestDto request, Guid? userId = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        var context = await BuildContextAsync(tenantId, request, ct);
        var system = GenerativeAiPrompts.ManagementSystem(request.Language);
        var buffer = new System.Text.StringBuilder();
        await foreach (var chunk in _adapter.GenerateStreamAsync(system, $"{context}\n\nUser: {request.Message}", ct))
        {
            buffer.Append(chunk);
            yield return chunk;
        }
        await PersistLogAsync(tenantId, GenerativeInteractionType.Chat, request.Message, buffer.ToString(),
            request.BranchId, userId, ct);
    }

    private async Task<string> BuildContextAsync(Guid tenantId, AiChatRequestDto request, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var filter = new ReportFilterDto(FromDate: today, ToDate: today, BranchId: request.BranchId);
        var dash = await _dashboard.GetExecutiveDashboardAsync(tenantId, filter, ct);
        var intent = GenerativeIntentMatcher.Classify(request.Message);
        var ar = request.Language.StartsWith("ar", StringComparison.OrdinalIgnoreCase);

        var kpiLines = dash.Kpis.Select(k => $"- {k.Label}: {k.Value} {k.Unit}".Trim()).ToList();
        var topProducts = dash.TopProducts.Labels.Zip(dash.TopProducts.Series.FirstOrDefault()?.Data ?? [])
            .Take(5).Select(p => $"- {p.First}: {p.Second}");

        var sb = new System.Text.StringBuilder();
        sb.AppendLine(ar ? "سياق KPI:" : "KPI context:");
        foreach (var line in kpiLines) sb.AppendLine(line);
        sb.AppendLine(ar ? "أفضل المنتجات:" : "Top products:");
        foreach (var line in topProducts) sb.AppendLine(line);
        sb.AppendLine($"Intent: {intent}");
        if (!string.IsNullOrWhiteSpace(request.Context))
            sb.AppendLine($"Extra: {request.Context}");
        return sb.ToString();
    }

    private async Task<AiGenerativeLog> PersistLogAsync(
        Guid tenantId, GenerativeInteractionType type, string input, string output,
        Guid? branchId, Guid? userId, CancellationToken ct)
    {
        var log = AiGenerativeLog.Create(tenantId, type, input, output, AiModelProvider.Heuristic, userId, branchId);
        _context.AiGenerativeLogs.Add(log);
        await _context.SaveChangesAsync(ct);
        return log;
    }

    private static int EstimateTokens(string input, string output)
        => (input.Length + output.Length) / 4;
}

public sealed class AiDashboardInsightsService : IAiDashboardInsightsService
{
    private readonly IDashboardService _dashboard;
    private readonly IDataQualityService _dataQuality;
    private readonly IPurchaseRecommendationService _purchaseRecs;
    private readonly IInventoryForecastService _inventoryForecast;
    private readonly IGenerativeAiAdapter _adapter;
    private readonly IApplicationDbContext _context;

    public AiDashboardInsightsService(
        IDashboardService dashboard, IDataQualityService dataQuality,
        IPurchaseRecommendationService purchaseRecs, IInventoryForecastService inventoryForecast,
        IGenerativeAiAdapter adapter, IApplicationDbContext context)
        => (_dashboard, _dataQuality, _purchaseRecs, _inventoryForecast, _adapter, _context)
            = (dashboard, dataQuality, purchaseRecs, inventoryForecast, adapter, context);

    public async Task<AiDashboardInsightDto> GetDashboardInsightsAsync(
        Guid tenantId, AiInsightFilterDto filter, CancellationToken ct = default)
    {
        var ar = filter.Language.StartsWith("ar", StringComparison.OrdinalIgnoreCase);
        var to = DateOnly.FromDateTime(DateTime.UtcNow);
        var from = to.AddDays(-Math.Max(1, filter.Days));
        var reportFilter = new ReportFilterDto(FromDate: from, ToDate: to, BranchId: filter.BranchId);

        var dash = await _dashboard.GetExecutiveDashboardAsync(tenantId, reportFilter, ct);
        var quality = await _dataQuality.EvaluateAsync(tenantId, ct);
        var purchases = await _purchaseRecs.GetRecommendationsAsync(tenantId, filter.BranchId, ct);
        var inventory = await _inventoryForecast.ForecastAsync(tenantId, new ForecastFilterDto(BranchId: filter.BranchId), ct);

        var highlights = new List<string>();
        var alerts = new List<string>();

        foreach (var kpi in dash.Kpis.Take(5))
        {
            highlights.Add(ar
                ? $"{kpi.Label}: {kpi.Value} {kpi.Unit}".Trim()
                : $"{kpi.Label}: {kpi.Value} {kpi.Unit}".Trim());
        }

        if (quality.OverallScore < 80)
            alerts.Add(ar ? $"جودة البيانات منخفضة ({quality.OverallScore:F0}%)" : $"Data quality is low ({quality.OverallScore:F0}%)");

        if (purchases.ItemCount > 0)
            alerts.Add(ar
                ? $"{purchases.ItemCount} توصية شراء عاجلة"
                : $"{purchases.ItemCount} urgent purchase recommendations");

        var criticalStock = inventory.Items.Count(i => i.RiskLevel is StockOutRiskLevel.Critical or StockOutRiskLevel.High);
        if (criticalStock > 0)
            alerts.Add(ar
                ? $"{criticalStock} صنف معرض لنفاد المخزون"
                : $"{criticalStock} items at stock-out risk");

        var context = string.Join("\n", highlights.Concat(alerts.Select(a => "ALERT: " + a)));
        var summary = await _adapter.GenerateAsync(
            GenerativeAiPrompts.InsightsSystem(filter.Language),
            context, ct);

        var log = AiGenerativeLog.Create(tenantId, GenerativeInteractionType.Insight, "dashboard", summary,
            AiModelProvider.Heuristic, branchId: filter.BranchId);
        _context.AiGenerativeLogs.Add(log);
        await _context.SaveChangesAsync(ct);

        return new AiDashboardInsightDto(summary, filter.Language, highlights, alerts, DateTimeOffset.UtcNow);
    }
}

public sealed class NaturalLanguageQueryService : INaturalLanguageQueryService
{
    private readonly ISalesAnalyticsService _sales;
    private readonly IInventoryForecastService _inventoryForecast;
    private readonly IDashboardService _dashboard;
    private readonly IGenerativeAiAdapter _adapter;
    private readonly IApplicationDbContext _context;

    public NaturalLanguageQueryService(
        ISalesAnalyticsService sales, IInventoryForecastService inventoryForecast,
        IDashboardService dashboard, IGenerativeAiAdapter adapter, IApplicationDbContext context)
        => (_sales, _inventoryForecast, _dashboard, _adapter, _context) = (sales, inventoryForecast, dashboard, adapter, context);

    public async Task<NaturalLanguageQueryResultDto> QueryAsync(
        Guid tenantId, NaturalLanguageQueryDto request, Guid? userId = null, CancellationToken ct = default)
    {
        var queryType = GenerativeIntentMatcher.Classify(request.Question);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var filter = new ReportFilterDto(FromDate: today.AddDays(-30), ToDate: today, BranchId: request.BranchId);
        var ar = request.Language.StartsWith("ar", StringComparison.OrdinalIgnoreCase);

        object? data = null;
        string factualAnswer;

        switch (queryType)
        {
            case "top_products":
            {
                var products = await _sales.GetSalesByProductAsync(tenantId, filter, ct);
                var top = products.Take(5).ToList();
                data = top;
                factualAnswer = ar
                    ? "أفضل المنتجات: " + string.Join(", ", top.Select(p => $"{p.ProductName} ({p.Revenue})"))
                    : "Top products: " + string.Join(", ", top.Select(p => $"{p.ProductName} ({p.Revenue})"));
                break;
            }
            case "inventory":
            {
                var forecast = await _inventoryForecast.ForecastAsync(tenantId, new ForecastFilterDto(BranchId: request.BranchId), ct);
                var risky = forecast.Items.Where(i => i.RiskLevel is StockOutRiskLevel.High or StockOutRiskLevel.Critical).Take(10).ToList();
                data = risky;
                factualAnswer = ar
                    ? $"أصناف عالية المخاطر: {risky.Count}"
                    : $"High-risk inventory items: {risky.Count}";
                break;
            }
            case "revenue":
            case "orders":
            default:
            {
                var dashFilter = filter with { FromDate = today, ToDate = today };
                var dash = await _dashboard.GetExecutiveDashboardAsync(tenantId, dashFilter, ct);
                data = dash.Kpis;
                var revenue = dash.Kpis.FirstOrDefault(k => k.Key == "revenue_today");
                var orders = dash.Kpis.FirstOrDefault(k => k.Key == "orders_today");
                factualAnswer = ar
                    ? $"مبيعات اليوم: {revenue?.Value ?? 0}، الطلبات: {orders?.Value ?? 0}"
                    : $"Today's revenue: {revenue?.Value ?? 0}, orders: {orders?.Value ?? 0}";
                break;
            }
        }

        var narrative = await _adapter.GenerateAsync(
            GenerativeAiPrompts.QuerySystem(request.Language),
            $"Facts:\n{factualAnswer}\n\nQuestion: {request.Question}", ct);

        var log = AiGenerativeLog.Create(tenantId, GenerativeInteractionType.Query, request.Question, narrative,
            AiModelProvider.Heuristic, userId, request.BranchId, JsonSerializer.Serialize(new { queryType }));
        _context.AiGenerativeLogs.Add(log);
        await _context.SaveChangesAsync(ct);

        return new NaturalLanguageQueryResultDto(request.Question, narrative, queryType, data, AiModelProvider.Heuristic, log.Id);
    }
}

public sealed class VoiceOrderingService : IVoiceOrderingService
{
    private static readonly Regex QuantityPattern = new(
        @"(?<qty>\d+|one|two|three|four|five|six|seven|eight|nine|ten|واحد|اثن|ثلاث|أربع|خمس|ست|سبع|ثمان|تسع|عشر)\s*(?<name>.+?)(?=\s*(?:and|و|,|$))",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly IApplicationDbContext _context;
    private readonly ILogger<VoiceOrderingService> _logger;

    public VoiceOrderingService(IApplicationDbContext context, ILogger<VoiceOrderingService> logger)
        => (_context, _logger) = (context, logger);

    public async Task<VoiceOrderDraftDto> ParseVoiceOrderAsync(
        Guid tenantId, VoiceOrderRequestDto request, Guid? userId = null, CancellationToken ct = default)
    {
        var products = await _context.Products.AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.IsAvailable)
            .Select(p => new { p.Id, p.NameAr, p.NameEn, p.BasePrice, p.Currency })
            .ToListAsync(ct);

        var lines = ParseTranscript(request.Transcript);
        var items = new List<VoiceOrderLineDto>();
        foreach (var (qty, name) in lines)
        {
            var match = products
                .OrderByDescending(p => ScoreMatch(name, p.NameAr, p.NameEn))
                .FirstOrDefault(p => ScoreMatch(name, p.NameAr, p.NameEn) > 0);
            if (match is null) continue;
            var lineTotal = match.BasePrice * qty;
            items.Add(new VoiceOrderLineDto(match.Id, match.NameAr, qty, match.BasePrice, lineTotal));
        }

        var total = items.Sum(i => i.LineTotal);
        var currency = items.Count > 0
            ? products.First(p => p.Id == items[0].ProductId).Currency
            : "SAR";
        var json = JsonSerializer.Serialize(items);
        var draft = VoiceOrderDraft.Create(tenantId, request.BranchId, request.Transcript, json, total, userId, currency);

        _context.VoiceOrderDrafts.Add(draft);
        var log = AiGenerativeLog.Create(tenantId, GenerativeInteractionType.Voice, request.Transcript,
            json, AiModelProvider.Heuristic, userId, request.BranchId);
        _context.AiGenerativeLogs.Add(log);
        await _context.SaveChangesAsync(ct);

        return MapDraft(draft, items);
    }

    public async Task<VoiceOrderDraftDto> ConfirmDraftAsync(
        Guid tenantId, ConfirmVoiceOrderDto dto, Guid? userId = null, CancellationToken ct = default)
    {
        var draft = await _context.VoiceOrderDrafts
            .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.Id == dto.DraftId && d.Status == VoiceOrderDraftStatus.Draft, ct)
            ?? throw new InvalidOperationException("Voice order draft not found.");

        draft.Confirm();
        await _context.SaveChangesAsync(ct);

        var items = JsonSerializer.Deserialize<List<VoiceOrderLineDto>>(draft.ParsedItemsJson) ?? [];
        return MapDraft(draft, items);
    }

    private static VoiceOrderDraftDto MapDraft(VoiceOrderDraft draft, IReadOnlyList<VoiceOrderLineDto> items)
        => new(draft.Id, draft.Transcript, draft.Status, items, draft.EstimatedTotal, draft.Currency,
            draft.Status == VoiceOrderDraftStatus.Draft);

    private static IReadOnlyList<(decimal Qty, string Name)> ParseTranscript(string transcript)
    {
        var results = new List<(decimal, string)>();
        foreach (Match m in QuantityPattern.Matches(transcript))
        {
            var qty = ParseQuantity(m.Groups["qty"].Value);
            var name = m.Groups["name"].Value.Trim();
            if (!string.IsNullOrWhiteSpace(name))
                results.Add((qty, name));
        }

        if (results.Count == 0)
        {
            foreach (var part in transcript.Split(new[] { "،", ",", " and ", " و " }, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = part.Trim();
                if (trimmed.Length > 1)
                    results.Add((1, trimmed));
            }
        }
        return results;
    }

    private static decimal ParseQuantity(string token) => token.ToLowerInvariant() switch
    {
        "one" or "واحد" or "واحدة" => 1,
        "two" or "اثن" or "اثنان" or "اثنتين" => 2,
        "three" or "ثلاث" or "ثلاثة" => 3,
        "four" or "أربع" or "أربعة" => 4,
        "five" or "خمس" or "خمسة" => 5,
        "six" or "ست" or "ستة" => 6,
        "seven" or "سبع" or "سبعة" => 7,
        "eight" or "ثمان" or "ثمانية" => 8,
        "nine" or "تسع" or "تسعة" => 9,
        "ten" or "عشر" or "عشرة" => 10,
        _ => decimal.TryParse(token, out var n) ? n : 1
    };

    private static int ScoreMatch(string spoken, string nameAr, string? nameEn)
    {
        var s = spoken.Trim().ToLowerInvariant();
        if (nameAr.Contains(s, StringComparison.OrdinalIgnoreCase) || s.Contains(nameAr, StringComparison.OrdinalIgnoreCase))
            return 3;
        if (!string.IsNullOrWhiteSpace(nameEn) &&
            (nameEn.Contains(s, StringComparison.OrdinalIgnoreCase) || s.Contains(nameEn, StringComparison.OrdinalIgnoreCase)))
            return 2;
        return 0;
    }
}
