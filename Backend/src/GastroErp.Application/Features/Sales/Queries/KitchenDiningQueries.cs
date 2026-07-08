using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Sales.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Sales.Queries;

public record GetKitchenStationsQuery(Guid TenantId, Guid? BranchId) : IRequest<Result<IReadOnlyList<KitchenStationDto>>>;
public record GetKitchenTicketsQuery(Guid TenantId, KitchenTicketFilterDto Filter) : IRequest<PagedResult<KitchenTicketDto>>;
public record GetKitchenTicketByIdQuery(Guid Id) : IRequest<Result<KitchenTicketDto>>;
public record GetActiveKitchenTicketsByStationQuery(Guid StationId) : IRequest<Result<IReadOnlyList<KitchenTicketDto>>>;

public record GetFloorPlansQuery(Guid TenantId, Guid? BranchId) : IRequest<Result<IReadOnlyList<FloorPlanDto>>>;
public record GetFloorPlanByIdQuery(Guid Id) : IRequest<Result<FloorPlanDetailDto>>;
public record GetTablesByFloorPlanQuery(Guid FloorPlanId) : IRequest<Result<IReadOnlyList<RestaurantTableDto>>>;
public record GetTableByIdQuery(Guid Id) : IRequest<Result<RestaurantTableDto>>;

public record GetTableReservationsQuery(Guid TenantId, TableReservationFilterDto Filter) : IRequest<PagedResult<TableReservationDto>>;
public record GetTableReservationByIdQuery(Guid Id) : IRequest<Result<TableReservationDto>>;
