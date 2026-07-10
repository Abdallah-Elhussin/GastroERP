using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Features.Sales.DTOs;
using GastroErp.Domain.Entities.Sales;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Sales.Services;

public interface IKdsBoardProjectionService
{
    Task<IReadOnlyList<KdsTicketViewDto>> GetActiveBoardAsync(
        Guid tenantId,
        Guid? branchId = null,
        Guid? stationId = null,
        CancellationToken cancellationToken = default);

    Task<KdsTicketViewDto?> ProjectTicketAsync(Guid ticketId, CancellationToken cancellationToken = default);
}

public sealed class KdsBoardProjectionService(IApplicationDbContext context) : IKdsBoardProjectionService
{
    private static readonly KitchenTicketStatus[] ActiveStatuses =
    [
        KitchenTicketStatus.Pending,
        KitchenTicketStatus.InProgress,
        KitchenTicketStatus.Ready
    ];

    public async Task<IReadOnlyList<KdsTicketViewDto>> GetActiveBoardAsync(
        Guid tenantId,
        Guid? branchId = null,
        Guid? stationId = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.KitchenTickets.AsNoTracking()
            .Include(t => t.Items)
            .Where(t => t.TenantId == tenantId && ActiveStatuses.Contains(t.Status));

        if (branchId.HasValue)
            query = query.Where(t => t.BranchId == branchId.Value);

        if (stationId.HasValue)
            query = query.Where(t => t.KitchenStationId == stationId.Value);

        var tickets = await query
            .OrderBy(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        return await ProjectManyAsync(tickets, cancellationToken);
    }

    public async Task<KdsTicketViewDto?> ProjectTicketAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var ticket = await context.KitchenTickets.AsNoTracking()
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);

        if (ticket is null)
            return null;

        var projected = await ProjectManyAsync([ticket], cancellationToken);
        return projected.FirstOrDefault();
    }

    private async Task<IReadOnlyList<KdsTicketViewDto>> ProjectManyAsync(
        IReadOnlyList<KitchenTicket> tickets,
        CancellationToken cancellationToken)
    {
        if (tickets.Count == 0)
            return [];

        var orderIds = tickets.Select(t => t.SalesOrderId).Distinct().ToList();
        var stationIds = tickets.Select(t => t.KitchenStationId).Distinct().ToList();

        var orders = await context.SalesOrders.AsNoTracking()
            .Where(o => orderIds.Contains(o.Id))
            .ToDictionaryAsync(o => o.Id, cancellationToken);

        var stations = await context.KitchenStations.AsNoTracking()
            .Where(s => stationIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, cancellationToken);

        var tableIds = orders.Values
            .Where(o => o.TableId.HasValue)
            .Select(o => o.TableId!.Value)
            .Distinct()
            .ToList();

        var tableLabels = new Dictionary<Guid, string>();
        if (tableIds.Count > 0)
        {
            var plans = await context.FloorPlans.AsNoTracking()
                .Include(f => f.DiningAreas).ThenInclude(a => a.Tables)
                .Where(f => f.DiningAreas.Any(a => a.Tables.Any(t => tableIds.Contains(t.Id))))
                .ToListAsync(cancellationToken);

            foreach (var table in plans.SelectMany(f => f.DiningAreas).SelectMany(a => a.Tables))
                tableLabels[table.Id] = table.TableNumber;
        }

        var now = DateTimeOffset.UtcNow;
        return tickets.Select(ticket =>
        {
            orders.TryGetValue(ticket.SalesOrderId, out var order);
            stations.TryGetValue(ticket.KitchenStationId, out var station);

            var tableLabel = ResolveTableLabel(order, tableLabels);
            var orderType = order?.OrderType.ToString() ?? "Takeaway";
            var elapsedBase = ticket.StartedAt ?? ticket.CreatedAt;
            var timerSeconds = Math.Max(0, (int)(now - elapsedBase).TotalSeconds);

            return new KdsTicketViewDto(
                ticket.Id,
                ticket.TicketNumber,
                tableLabel,
                orderType,
                ticket.KitchenStationId,
                station?.StationType ?? KitchenStationType.General,
                station?.NameAr ?? "محطة المطبخ",
                station?.NameEn,
                MapKdsStatus(ticket.Status),
                timerSeconds,
                ticket.CreatedAt,
                ticket.Items.Select(i => new KdsTicketItemViewDto(
                    i.Id,
                    i.ProductNameAr,
                    i.Quantity,
                    BuildNotes(i.ModifiersSummary, i.ProductNameEn))).ToList());
        }).ToList();
    }

    private static string ResolveTableLabel(SalesOrder? order, IReadOnlyDictionary<Guid, string> tableLabels)
    {
        if (order is null)
            return "—";

        if (!string.IsNullOrWhiteSpace(order.Notes) && order.Notes.StartsWith("KDS:", StringComparison.Ordinal))
            return order.Notes["KDS:".Length..].Trim();

        if (order.TableId.HasValue && tableLabels.TryGetValue(order.TableId.Value, out var tableNo))
            return $"طاولة {tableNo}";

        return order.OrderType switch
        {
            OrderType.DineIn => "محلي",
            OrderType.TakeAway => "سفري",
            OrderType.Delivery => "توصيل",
            _ => order.OrderNumber
        };
    }

    private static IReadOnlyList<string> BuildNotes(string? modifiersSummary, string? productNameEn)
    {
        var notes = new List<string>();
        if (!string.IsNullOrWhiteSpace(modifiersSummary))
            notes.Add(modifiersSummary);
        if (!string.IsNullOrWhiteSpace(productNameEn))
            notes.Add(productNameEn);
        return notes;
    }

    private static string MapKdsStatus(KitchenTicketStatus status) => status switch
    {
        KitchenTicketStatus.Pending => "new",
        KitchenTicketStatus.InProgress => "preparing",
        KitchenTicketStatus.Ready => "ready",
        _ => "completed"
    };
}
