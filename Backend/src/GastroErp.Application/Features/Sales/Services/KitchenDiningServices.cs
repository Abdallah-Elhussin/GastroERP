using GastroErp.Application.Common.Interfaces;
using GastroErp.Domain.Entities.Sales;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Sales.Services;

public sealed class KitchenRoutingService : IKitchenRoutingService
{
    private readonly IApplicationDbContext _context;

    public KitchenRoutingService(IApplicationDbContext context) => _context = context;

    public async Task RouteOrderAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _context.SalesOrders
            .Include(o => o.Items).ThenInclude(i => i.Modifiers)
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);

        if (order is null) return;

        var stations = await _context.KitchenStations
            .AsNoTracking()
            .Where(s => s.BranchId == order.BranchId && s.IsActive)
            .ToListAsync(ct);

        if (stations.Count == 0) return;

        var generalStation = stations.FirstOrDefault(s => s.StationType == KitchenStationType.General) ?? stations[0];
        var items = order.Items.Where(i => !i.IsVoided).ToList();
        if (items.Count == 0) return;

        var productIds = items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _context.Products.AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        var groups = new Dictionary<Guid, List<OrderItem>>();
        foreach (var item in items)
        {
            var categoryId = products.TryGetValue(item.ProductId, out var product) ? product.CategoryId : Guid.Empty;
            var station = stations.FirstOrDefault(s => s.CategoryId == categoryId) ?? generalStation;
            if (!groups.TryGetValue(station.Id, out var list))
            {
                list = [];
                groups[station.Id] = list;
            }
            list.Add(item);
        }

        var ticketCount = await _context.KitchenTickets.CountAsync(t => t.BranchId == order.BranchId, ct);
        var index = 0;

        foreach (var (stationId, stationItems) in groups)
        {
            index++;
            var ticketNumber = $"KT-{order.OrderNumber}-{index:D2}";
            var ticket = KitchenTicket.Create(
                order.TenantId, order.BranchId, order.Id, stationId, ticketNumber);

            foreach (var item in stationItems)
            {
                var mods = item.Modifiers.Count > 0
                    ? string.Join(", ", item.Modifiers.Select(m => m.ModifierNameAr))
                    : null;
                ticket.AddItem(item.Id, item.ProductNameAr, item.ProductNameEn, item.Quantity, mods);
            }

            _context.KitchenTickets.Add(ticket);
            ticketCount++;
        }
    }
}

public sealed class TableService : ITableService
{
    private readonly IApplicationDbContext _context;

    public TableService(IApplicationDbContext context) => _context = context;

    public async Task<RestaurantTable?> GetTableByIdAsync(Guid tableId, CancellationToken ct = default)
    {
        var plans = await _context.FloorPlans
            .Include(f => f.DiningAreas).ThenInclude(a => a.Tables)
            .Where(f => f.DiningAreas.Any(a => a.Tables.Any(t => t.Id == tableId)))
            .ToListAsync(ct);

        return plans.SelectMany(f => f.DiningAreas)
            .SelectMany(a => a.Tables)
            .FirstOrDefault(t => t.Id == tableId);
    }

    public async Task OccupyTableAsync(Guid tableId, Guid orderId, CancellationToken ct = default)
    {
        var plan = await _context.FloorPlans
            .Include(f => f.DiningAreas).ThenInclude(a => a.Tables)
            .FirstOrDefaultAsync(f => f.DiningAreas.Any(a => a.Tables.Any(t => t.Id == tableId)), ct);

        if (plan is null) return;

        var table = plan.DiningAreas.SelectMany(a => a.Tables).First(t => t.Id == tableId);
        table.Occupy(orderId);
        _context.FloorPlans.Update(plan);
    }

    public async Task ReleaseTableForOrderAsync(Guid? tableId, CancellationToken ct = default)
    {
        if (!tableId.HasValue) return;

        var plan = await _context.FloorPlans
            .Include(f => f.DiningAreas).ThenInclude(a => a.Tables)
            .FirstOrDefaultAsync(f => f.DiningAreas.Any(a => a.Tables.Any(t => t.Id == tableId)), ct);

        if (plan is null) return;

        var table = plan.DiningAreas.SelectMany(a => a.Tables).First(t => t.Id == tableId);
        table.Release();
        _context.FloorPlans.Update(plan);
    }
}
