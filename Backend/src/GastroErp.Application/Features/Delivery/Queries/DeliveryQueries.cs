using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Delivery.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Delivery.Queries;

public record GetDeliveryZonesQuery(Guid TenantId, Guid? BranchId) : IRequest<Result<IReadOnlyList<DeliveryZoneDto>>>;
public record GetDeliveryZoneByIdQuery(Guid Id) : IRequest<Result<DeliveryZoneDto>>;
public record GetDeliveryZoneFeeQuery(Guid ZoneId, decimal Latitude, decimal Longitude) : IRequest<Result<DeliveryFeeQuoteDto>>;

public record GetDeliveryDriversQuery(Guid TenantId, Guid? BranchId) : IRequest<Result<IReadOnlyList<DeliveryDriverDto>>>;
public record GetAvailableDriversQuery(Guid TenantId, Guid BranchId) : IRequest<Result<IReadOnlyList<DeliveryDriverDto>>>;

public record GetDeliveryOrdersQuery(Guid TenantId, DeliveryOrderFilterDto Filter) : IRequest<PagedResult<DeliveryOrderDto>>;
public record GetDeliveryOrderByIdQuery(Guid Id) : IRequest<Result<DeliveryOrderDetailDto>>;
public record GetDeliveryBySalesOrderQuery(Guid SalesOrderId) : IRequest<Result<DeliveryOrderDetailDto>>;
public record GetActiveDeliveriesByDriverQuery(Guid DriverId) : IRequest<Result<IReadOnlyList<DeliveryOrderDto>>>;
public record GetDeliveryTrackingQuery(Guid DeliveryOrderId) : IRequest<Result<IReadOnlyList<DeliveryTrackingEventDto>>>;
