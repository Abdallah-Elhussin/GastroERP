using System.Text.Json;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Interfaces.Ai;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Infrastructure.Ai;

public sealed class HeuristicFraudAnalysisEngine : IFraudAnalysisEngine
{
    private readonly IApplicationDbContext _context;

    public HeuristicFraudAnalysisEngine(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<FraudSignal>> AnalyzeAsync(
        Guid tenantId, int lookbackDays, Guid? branchId = null, CancellationToken ct = default)
    {
        var from = DateTimeOffset.UtcNow.AddDays(-lookbackDays);
        var signals = new List<FraudSignal>();

        var ordersQuery = _context.SalesOrders.AsNoTracking()
            .Where(o => o.TenantId == tenantId && o.CreatedAt >= from);
        if (branchId.HasValue)
            ordersQuery = ordersQuery.Where(o => o.BranchId == branchId);

        var orders = await ordersQuery
            .Select(o => new
            {
                o.Id, o.BranchId, o.SubTotal, o.DiscountTotal, o.GrandTotal, o.CashierId, o.CreatedAt
            })
            .ToListAsync(ct);

        foreach (var order in orders.Where(o => o.SubTotal > 0 && o.DiscountTotal / o.SubTotal > 0.35m))
        {
            var ratio = order.DiscountTotal / order.SubTotal;
            var score = Math.Min(100, 40 + ratio * 100);
            signals.Add(new FraudSignal(
                FraudType.DiscountFraud, score, MapSeverity(score), "HeuristicFraudEngine",
                JsonSerializer.Serialize(new { order.Id, DiscountRatio = ratio, order.DiscountTotal }),
                "SalesOrder", order.Id, order.BranchId));
        }

        var voidItems = await (
            from item in _context.OrderItems.AsNoTracking()
            join order in ordersQuery on item.SalesOrderId equals order.Id
            where item.IsVoided
            group item by new { order.BranchId, order.CashierId } into g
            select new { g.Key.BranchId, g.Key.CashierId, Count = g.Count() })
            .Where(x => x.Count >= 5)
            .ToListAsync(ct);

        foreach (var v in voidItems)
        {
            var score = Math.Min(100, 30 + v.Count * 8m);
            signals.Add(new FraudSignal(
                FraudType.VoidFraud, score, MapSeverity(score), "HeuristicFraudEngine",
                JsonSerializer.Serialize(new { v.CashierId, VoidCount = v.Count }),
                "Cashier", v.CashierId, v.BranchId));
        }

        var paymentsQuery = _context.Payments.AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.CreatedAt >= from);
        if (branchId.HasValue)
            paymentsQuery = paymentsQuery.Where(p => p.BranchId == branchId);

        var voidedPayments = await paymentsQuery.CountAsync(p => p.Status == PaymentStatus.Voided, ct);
        if (voidedPayments >= 3)
        {
            var score = Math.Min(100, 25 + voidedPayments * 10m);
            signals.Add(new FraudSignal(
                FraudType.PaymentFraud, score, MapSeverity(score), "HeuristicFraudEngine",
                JsonSerializer.Serialize(new { VoidedPayments = voidedPayments }),
                "Tenant", tenantId, branchId));
        }

        var refundCount = await paymentsQuery
            .CountAsync(p => p.Status == PaymentStatus.Refunded || p.Status == PaymentStatus.PartiallyRefunded, ct);
        if (refundCount >= 5)
        {
            var score = Math.Min(100, 20 + refundCount * 8m);
            signals.Add(new FraudSignal(
                FraudType.RefundFraud, score, MapSeverity(score), "HeuristicFraudEngine",
                JsonSerializer.Serialize(new { RefundCount = refundCount }),
                "Tenant", tenantId, branchId));
        }

        var duplicatePayments = await (
            from p in paymentsQuery
            where p.Status == PaymentStatus.Completed
            from a in p.Allocations
            group a by a.SalesOrderId into g
            where g.Select(x => x.PaymentId).Distinct().Count() > 1
            select new { SalesOrderId = g.Key, Count = g.Select(x => x.PaymentId).Distinct().Count() })
            .ToListAsync(ct);

        foreach (var dup in duplicatePayments)
        {
            var score = Math.Min(100, 50 + dup.Count * 15m);
            signals.Add(new FraudSignal(
                FraudType.DuplicatePayment, score, MapSeverity(score), "HeuristicFraudEngine",
                JsonSerializer.Serialize(new { dup.SalesOrderId, dup.Count }),
                "SalesOrder", dup.SalesOrderId, branchId));
        }

        return signals.OrderByDescending(s => s.RiskScore).ToList();
    }

    private static FraudSeverity MapSeverity(decimal score) => score switch
    {
        >= 85 => FraudSeverity.Critical,
        >= 65 => FraudSeverity.High,
        >= 40 => FraudSeverity.Medium,
        _ => FraudSeverity.Low
    };
}

public sealed class HeuristicCustomerSegmentationEngine : ICustomerSegmentationEngine
{
    private readonly IApplicationDbContext _context;

    public HeuristicCustomerSegmentationEngine(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<SegmentAssignment>> SegmentAsync(
        Guid tenantId, int lookbackDays, Guid? branchId = null, CancellationToken ct = default)
    {
        var customers = await _context.Customers.AsNoTracking()
            .Where(c => c.TenantId == tenantId && c.Status == CustomerStatus.Active)
            .Select(c => new { c.Id, c.TotalOrders, c.TotalSpending, c.LastVisit, c.CreatedAt })
            .ToListAsync(ct);

        var now = DateTimeOffset.UtcNow;
        var results = new List<SegmentAssignment>();

        foreach (var c in customers)
        {
            var recencyDays = c.LastVisit.HasValue ? (now - c.LastVisit.Value).TotalDays : 999;
            var frequency = c.TotalOrders;
            var monetary = c.TotalSpending;
            var accountAgeDays = (now - c.CreatedAt).TotalDays;

            var segment = Classify(recencyDays, frequency, monetary, accountAgeDays);
            var score = ComputeScore(recencyDays, frequency, monetary);
            var metrics = JsonSerializer.Serialize(new { RecencyDays = recencyDays, Frequency = frequency, Monetary = monetary });

            results.Add(new SegmentAssignment(c.Id, segment, score, metrics));
        }

        return results;
    }

    private static CustomerSegmentType Classify(double recencyDays, int frequency, decimal monetary, double accountAgeDays)
    {
        if (accountAgeDays <= 30 && frequency <= 2) return CustomerSegmentType.New;
        if (recencyDays >= 365) return CustomerSegmentType.Lost;
        if (recencyDays >= 180) return CustomerSegmentType.Dormant;
        if (recencyDays >= 60 && frequency >= 3) return CustomerSegmentType.AtRisk;
        if (monetary >= 5000 && frequency >= 10) return CustomerSegmentType.VIP;
        if (frequency >= 8) return CustomerSegmentType.Loyal;
        if (recencyDays <= 30) return CustomerSegmentType.Active;
        return CustomerSegmentType.Active;
    }

    private static decimal ComputeScore(double recencyDays, int frequency, decimal monetary)
    {
        var recencyScore = (decimal)Math.Max(0, 100 - recencyDays);
        var freqScore = Math.Min(100, frequency * 5m);
        var moneyScore = Math.Min(100, monetary / 100m);
        return Math.Round((recencyScore + freqScore + moneyScore) / 3m, 2);
    }
}

public sealed class HeuristicChurnPredictionEngine : IChurnPredictionEngine
{
    private readonly IApplicationDbContext _context;

    public HeuristicChurnPredictionEngine(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<ChurnScore>> PredictAsync(
        Guid tenantId, int lookbackDays, Guid? branchId = null, CancellationToken ct = default)
    {
        var segments = await _context.Customers.AsNoTracking()
            .Where(c => c.TenantId == tenantId && c.Status == CustomerStatus.Active && c.TotalOrders > 0)
            .Select(c => new { c.Id, c.TotalOrders, c.TotalSpending, c.LastVisit, c.AverageTicket })
            .ToListAsync(ct);

        var now = DateTimeOffset.UtcNow;
        var results = new List<ChurnScore>();

        foreach (var c in segments)
        {
            var recencyDays = c.LastVisit.HasValue ? (now - c.LastVisit.Value).TotalDays : 999;
            var frequencyScore = Math.Min(100, c.TotalOrders * 4m);
            var recencyPenalty = Math.Min(100, (decimal)recencyDays * 0.8m);
            var spendFactor = c.TotalSpending > 0 ? Math.Min(30, c.TotalSpending / 200m) : 0;

            var probability = Math.Clamp(recencyPenalty - frequencyScore * 0.3m + 20 - spendFactor, 0, 100);
            var risk = MapRisk(probability);
            var recommendation = risk switch
            {
                ChurnRiskLevel.Critical or ChurnRiskLevel.High => "Send retention coupon or special offer",
                ChurnRiskLevel.Medium => "Send personalized engagement message",
                _ => "Continue standard loyalty program"
            };

            var metrics = JsonSerializer.Serialize(new
            {
                RecencyDays = recencyDays,
                c.TotalOrders,
                c.TotalSpending,
                c.AverageTicket
            });

            results.Add(new ChurnScore(c.Id, Math.Round(probability, 2), risk, recommendation, metrics));
        }

        return results.OrderByDescending(r => r.ChurnProbability).ToList();
    }

    private static ChurnRiskLevel MapRisk(decimal probability) => probability switch
    {
        >= 80 => ChurnRiskLevel.Critical,
        >= 60 => ChurnRiskLevel.High,
        >= 35 => ChurnRiskLevel.Medium,
        _ => ChurnRiskLevel.Low
    };
}

public sealed class HeuristicProductRecommendationEngine : IProductRecommendationEngine
{
    private readonly IApplicationDbContext _context;

    public HeuristicProductRecommendationEngine(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<ProductRecommendationSignal>> GenerateAsync(
        Guid tenantId, int lookbackDays, Guid? branchId = null, CancellationToken ct = default)
    {
        var from = DateTimeOffset.UtcNow.AddDays(-lookbackDays);
        var orderIds = _context.SalesOrders.AsNoTracking()
            .Where(o => o.TenantId == tenantId && o.CreatedAt >= from);
        if (branchId.HasValue)
            orderIds = orderIds.Where(o => o.BranchId == branchId);

        var items = await (
            from item in _context.OrderItems.AsNoTracking()
            join order in orderIds on item.SalesOrderId equals order.Id
            where !item.IsVoided
            select new { item.SalesOrderId, item.ProductId, item.ProductNameAr, item.LineTotal, item.Quantity })
            .ToListAsync(ct);

        var products = await _context.Products.AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.IsAvailable)
            .Select(p => new { p.Id, p.NameAr, p.BasePrice, p.CategoryId })
            .ToListAsync(ct);

        var productLookup = products.ToDictionary(p => p.Id);
        var topProducts = items.GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Revenue = g.Sum(x => x.LineTotal), Qty = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.Revenue)
            .Take(20)
            .ToList();

        var coPurchase = items.GroupBy(i => i.SalesOrderId)
            .Select(g => g.Select(x => x.ProductId).Distinct().OrderBy(x => x).ToList())
            .Where(g => g.Count >= 2)
            .ToList();

        var pairCounts = new Dictionary<(Guid, Guid), int>();
        foreach (var order in coPurchase)
        {
            for (var i = 0; i < order.Count; i++)
            for (var j = i + 1; j < order.Count; j++)
            {
                var key = order[i].CompareTo(order[j]) < 0 ? (order[i], order[j]) : (order[j], order[i]);
                pairCounts[key] = pairCounts.GetValueOrDefault(key) + 1;
            }
        }

        var signals = new List<ProductRecommendationSignal>();

        foreach (var top in topProducts)
        {
            if (!productLookup.TryGetValue(top.ProductId, out var product))
                continue;

            var upsell = products
                .Where(p => p.CategoryId == product.CategoryId && p.BasePrice > product.BasePrice)
                .OrderBy(p => p.BasePrice)
                .Take(3)
                .Select(p => (p.Id, p.NameAr, 70m))
                .ToList();

            if (upsell.Count > 0)
            {
                signals.Add(new ProductRecommendationSignal(
                    top.ProductId, ProductRecommendationType.Upsell, 72,
                    upsell, BranchId: branchId));
            }

            var fbt = pairCounts
                .Where(kv => kv.Key.Item1 == top.ProductId || kv.Key.Item2 == top.ProductId)
                .OrderByDescending(kv => kv.Value)
                .Take(3)
                .Select(kv =>
                {
                    var otherId = kv.Key.Item1 == top.ProductId ? kv.Key.Item2 : kv.Key.Item1;
                    var name = productLookup.GetValueOrDefault(otherId)?.NameAr ?? "Product";
                    var conf = Math.Min(95, 50 + kv.Value * 5m);
                    return (otherId, name, conf);
                })
                .ToList();

            if (fbt.Count > 0)
            {
                signals.Add(new ProductRecommendationSignal(
                    top.ProductId, ProductRecommendationType.FrequentlyBoughtTogether, 80,
                    fbt, BranchId: branchId));

                signals.Add(new ProductRecommendationSignal(
                    top.ProductId, ProductRecommendationType.CrossSell, 75,
                    fbt, BranchId: branchId));
            }

            var similar = products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id)
                .Take(3)
                .Select(p => (p.Id, p.NameAr, 65m))
                .ToList();

            if (similar.Count > 0)
            {
                signals.Add(new ProductRecommendationSignal(
                    top.ProductId, ProductRecommendationType.SimilarProduct, 68,
                    similar, BranchId: branchId));
            }
        }

        return signals;
    }
}
