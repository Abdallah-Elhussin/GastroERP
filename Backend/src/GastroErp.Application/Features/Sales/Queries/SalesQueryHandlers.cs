using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Sales.DTOs;
using GastroErp.Application.Features.Sales.Queries;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Sales.Queries;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDetailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetOrderByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<OrderDetailDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.SalesOrders
            .AsNoTracking()
            .Include(o => o.Items).ThenInclude(i => i.Modifiers)
            .Include(o => o.Discounts)
            .Include(o => o.Taxes)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        return order is null
            ? Result<OrderDetailDto>.Failure("NotFound", "Order not found.")
            : Result<OrderDetailDto>.Success(_mapper.Map<OrderDetailDto>(order));
    }
}

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, PagedResult<OrderSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetOrdersQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<OrderSummaryDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = _context.SalesOrders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.TenantId == request.TenantId);

        if (filter.BranchId.HasValue)
            query = query.Where(o => o.BranchId == filter.BranchId.Value);

        if (filter.Status.HasValue)
            query = query.Where(o => o.Status == filter.Status.Value);

        if (filter.OrderType.HasValue)
            query = query.Where(o => o.OrderType == filter.OrderType.Value);

        if (filter.SalesChannel.HasValue)
            query = query.Where(o => o.SalesChannel == filter.SalesChannel.Value);

        if (filter.FromDate.HasValue)
            query = query.Where(o => o.CreatedAt >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(o => o.CreatedAt <= filter.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            query = query.Where(o => o.OrderNumber.Contains(filter.SearchTerm));

        var totalCount = await query.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var page = Math.Max(filter.Page, 1);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<IReadOnlyList<OrderSummaryDto>>(orders);
        return PagedResult<OrderSummaryDto>.Success(dtos, page, pageSize, totalCount);
    }
}

public class GetActiveOrdersByBranchQueryHandler : IRequestHandler<GetActiveOrdersByBranchQuery, Result<IReadOnlyList<OrderSummaryDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetActiveOrdersByBranchQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<IReadOnlyList<OrderSummaryDto>>> Handle(GetActiveOrdersByBranchQuery request, CancellationToken cancellationToken)
    {
        var activeStatuses = new[]
        {
            OrderStatus.Draft, OrderStatus.Pending, OrderStatus.Confirmed,
            OrderStatus.Preparing, OrderStatus.Ready, OrderStatus.Served
        };

        var orders = await _context.SalesOrders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.BranchId == request.BranchId && activeStatuses.Contains(o.Status))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<OrderSummaryDto>>.Success(_mapper.Map<IReadOnlyList<OrderSummaryDto>>(orders));
    }
}

public class GetOrderStatusHistoryQueryHandler : IRequestHandler<GetOrderStatusHistoryQuery, Result<IReadOnlyList<OrderStatusHistoryDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetOrderStatusHistoryQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<IReadOnlyList<OrderStatusHistoryDto>>> Handle(GetOrderStatusHistoryQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.SalesOrders
            .AsNoTracking()
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
            return Result<IReadOnlyList<OrderStatusHistoryDto>>.Failure("NotFound", "Order not found.");

        var history = order.StatusHistory.OrderBy(h => h.ChangedAt).ToList();

        return Result<IReadOnlyList<OrderStatusHistoryDto>>.Success(
            _mapper.Map<IReadOnlyList<OrderStatusHistoryDto>>(history));
    }
}
