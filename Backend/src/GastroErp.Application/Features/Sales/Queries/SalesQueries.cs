using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Sales.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Sales.Queries;

public record GetOrderByIdQuery(Guid Id) : IRequest<Result<OrderDetailDto>>;
public record GetOrdersQuery(Guid TenantId, OrderFilterDto Filter) : IRequest<PagedResult<OrderSummaryDto>>;
public record GetActiveOrdersByBranchQuery(Guid BranchId) : IRequest<Result<IReadOnlyList<OrderSummaryDto>>>;
public record GetOrderStatusHistoryQuery(Guid OrderId) : IRequest<Result<IReadOnlyList<OrderStatusHistoryDto>>>;
