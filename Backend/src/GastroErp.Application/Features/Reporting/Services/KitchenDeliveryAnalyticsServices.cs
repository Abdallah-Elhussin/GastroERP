using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Features.Reporting.DTOs;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Reporting.Services;

public sealed class KitchenAnalyticsService : IKitchenAnalyticsService
{
    private readonly IApplicationDbContext _context;
    public KitchenAnalyticsService(IApplicationDbContext context) => _context = context;

    private IQueryable<Domain.Entities.Sales.KitchenTicket> Tickets(Guid tenantId, ReportFilterDto filter)
    {
        var (from, to) = ReportQueryHelper.ResolveDateRange(filter);
        var q = _context.KitchenTickets.AsNoTracking()
            .Where(t => t.TenantId == tenantId && t.CreatedAt >= from && t.CreatedAt <= to);
        if (filter.BranchId.HasValue) q = q.Where(t => t.BranchId == filter.BranchId);
        return q;
    }

    public async Task<KitchenPerformanceDto> GetKitchenPerformanceAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var tickets = await Tickets(tenantId, filter)
            .Where(t => t.CompletedAt != null)
            .Select(t => new { t.StartedAt, t.CompletedAt, t.EstimatedPrepMinutes })
            .ToListAsync(ct);

        var total = tickets.Count;
        if (total == 0) return new KitchenPerformanceDto(0, 0, 0, 0, 100);

        var prepTimes = tickets
            .Where(t => t.StartedAt.HasValue && t.CompletedAt.HasValue)
            .Select(t => (t.CompletedAt!.Value - t.StartedAt!.Value).TotalMinutes).ToList();

        var avgPrep = prepTimes.Count > 0 ? prepTimes.Average() : 0;
        var delayed = tickets.Count(t => t.EstimatedPrepMinutes.HasValue && t.StartedAt.HasValue && t.CompletedAt.HasValue
            && (t.CompletedAt!.Value - t.StartedAt!.Value).TotalMinutes > t.EstimatedPrepMinutes.Value);

        return new KitchenPerformanceDto(total, total, avgPrep, delayed,
            total > 0 ? Math.Round((total - delayed) * 100.0 / total, 2) : 100);
    }

    public async Task<IReadOnlyList<DelayedOrderDto>> GetDelayedOrdersAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var tickets = await Tickets(tenantId, filter)
            .Where(t => t.CompletedAt != null && t.StartedAt != null && t.EstimatedPrepMinutes != null)
            .Select(t => new { t.Id, t.TicketNumber, t.StartedAt, t.CompletedAt, t.EstimatedPrepMinutes })
            .ToListAsync(ct);

        return tickets
            .Where(t => (t.CompletedAt!.Value - t.StartedAt!.Value).TotalMinutes > t.EstimatedPrepMinutes!.Value)
            .Select(t => new DelayedOrderDto(t.Id, t.TicketNumber, null,
                (t.CompletedAt!.Value - t.StartedAt!.Value).TotalMinutes, t.EstimatedPrepMinutes!.Value))
            .OrderByDescending(x => x.PrepMinutes).Take(100).ToList();
    }

    public async Task<IReadOnlyList<KitchenStationLoadDto>> GetStationLoadAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var data = await Tickets(tenantId, filter)
            .GroupBy(t => t.KitchenStationId)
            .Select(g => new
            {
                StationId = g.Key,
                Active = g.Count(t => t.Status == KitchenTicketStatus.Pending || t.Status == KitchenTicketStatus.InProgress),
                Completed = g.Count(t => t.Status == KitchenTicketStatus.Completed)
            }).ToListAsync(ct);

        var stationIds = data.Select(d => d.StationId).ToList();
        var stations = await _context.KitchenStations.AsNoTracking()
            .Where(s => stationIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id, s => s.NameAr, ct);

        return data.Select(d => new KitchenStationLoadDto(
            d.StationId, stations.GetValueOrDefault(d.StationId, "Unknown"),
            d.Active, d.Completed, 0)).OrderByDescending(x => x.ActiveTickets).ToList();
    }

    public async Task<IReadOnlyList<TopDelayedProductDto>> GetTopDelayedProductsAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var ticketIds = await Tickets(tenantId, filter).Select(t => t.Id).ToListAsync(ct);
        var items = await _context.KitchenTicketItems.AsNoTracking()
            .Where(i => ticketIds.Contains(i.KitchenTicketId) && i.CompletedAt != null && i.StartedAt != null)
            .Select(i => new { i.ProductNameAr, i.StartedAt, i.CompletedAt })
            .ToListAsync(ct);

        return items.GroupBy(i => i.ProductNameAr)
            .Select(g => new TopDelayedProductDto(g.Key, g.Count(),
                g.Average(x => (x.CompletedAt!.Value - x.StartedAt!.Value).TotalMinutes)))
            .OrderByDescending(x => x.DelayCount).Take(20).ToList();
    }
}

public sealed class DeliveryAnalyticsService : IDeliveryAnalyticsService
{
    private readonly IApplicationDbContext _context;
    public DeliveryAnalyticsService(IApplicationDbContext context) => _context = context;

    private IQueryable<Domain.Entities.Delivery.DeliveryOrder> Deliveries(Guid tenantId, ReportFilterDto filter)
    {
        var (from, to) = ReportQueryHelper.ResolveDateRange(filter);
        var q = _context.DeliveryOrders.AsNoTracking()
            .Where(d => d.TenantId == tenantId && d.CreatedAt >= from && d.CreatedAt <= to);
        if (filter.BranchId.HasValue) q = q.Where(d => d.BranchId == filter.BranchId);
        return q;
    }

    public async Task<DeliverySummaryDto> GetDeliverySummaryAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var deliveries = await Deliveries(tenantId, filter)
            .Select(d => new { d.Status, d.DeliveryFee, d.AssignedAt, d.DeliveredAt })
            .ToListAsync(ct);

        var delivered = deliveries.Where(d => d.Status == DeliveryStatus.Delivered).ToList();
        var avgMinutes = delivered
            .Where(d => d.AssignedAt.HasValue && d.DeliveredAt.HasValue)
            .Select(d => (d.DeliveredAt!.Value - d.AssignedAt!.Value).TotalMinutes)
            .DefaultIfEmpty(0).Average();

        return new DeliverySummaryDto(
            deliveries.Count,
            delivered.Count,
            deliveries.Count(d => d.Status == DeliveryStatus.Failed),
            delivered.Sum(_ => 0m),
            deliveries.Sum(d => d.DeliveryFee),
            avgMinutes);
    }

    public async Task<IReadOnlyList<DriverPerformanceDto>> GetDriverPerformanceAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var data = await Deliveries(tenantId, filter)
            .Where(d => d.CurrentDriverId != null)
            .GroupBy(d => d.CurrentDriverId!.Value)
            .Select(g => new
            {
                DriverId = g.Key,
                Deliveries = g.Count(d => d.Status == DeliveryStatus.Delivered),
                Failures = g.Count(d => d.Status == DeliveryStatus.Failed),
                Fees = g.Sum(d => d.DeliveryFee)
            }).ToListAsync(ct);

        var driverIds = data.Select(d => d.DriverId).ToList();
        var drivers = await _context.DeliveryDrivers.AsNoTracking()
            .Where(d => driverIds.Contains(d.Id)).ToDictionaryAsync(d => d.Id, d => d.NameAr, ct);

        return data.Select(d => new DriverPerformanceDto(
            d.DriverId, drivers.GetValueOrDefault(d.DriverId, "Unknown"),
            d.Deliveries, d.Failures, 0, d.Fees)).OrderByDescending(x => x.Deliveries).ToList();
    }

    public async Task<IReadOnlyList<DeliveryZoneReportDto>> GetDeliveryByZoneAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
    {
        var data = await Deliveries(tenantId, filter)
            .Where(d => d.DeliveryZoneId != null)
            .GroupBy(d => d.DeliveryZoneId!.Value)
            .Select(g => new { ZoneId = g.Key, Count = g.Count(), Fees = g.Sum(d => d.DeliveryFee) })
            .ToListAsync(ct);

        var zoneIds = data.Select(d => d.ZoneId).ToList();
        var zones = await _context.DeliveryZones.AsNoTracking()
            .Where(z => zoneIds.Contains(z.Id)).ToDictionaryAsync(z => z.Id, z => z.NameAr, ct);

        return data.Select(d => new DeliveryZoneReportDto(
            d.ZoneId, zones.GetValueOrDefault(d.ZoneId, "Unknown"), d.Count, 0, d.Fees))
            .OrderByDescending(x => x.OrderCount).ToList();
    }

    public async Task<IReadOnlyList<FailedDeliveryDto>> GetFailedDeliveriesAsync(Guid tenantId, ReportFilterDto filter, CancellationToken ct = default)
        => await Deliveries(tenantId, filter)
            .Where(d => d.Status == DeliveryStatus.Failed)
            .Select(d => new FailedDeliveryDto(d.Id, d.DeliveryNumber, d.FailureReason, d.FailedAt!.Value))
            .OrderByDescending(x => x.FailedAt).Take(200).ToListAsync(ct);
}
