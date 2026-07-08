using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Sales.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Sales.Commands;

// Kitchen
public record CreateKitchenStationCommand(Guid TenantId, CreateKitchenStationDto Dto) : IRequest<Result<KitchenStationDto>>;
public record UpdateKitchenStationCommand(Guid Id, UpdateKitchenStationDto Dto) : IRequest<Result>;
public record StartKitchenTicketCommand(Guid TicketId) : IRequest<Result>;
public record MarkKitchenTicketReadyCommand(Guid TicketId) : IRequest<Result>;
public record CompleteKitchenTicketCommand(Guid TicketId) : IRequest<Result>;
public record MarkKitchenItemReadyCommand(Guid ItemId) : IRequest<Result>;

// Floor Plan
public record CreateFloorPlanCommand(Guid TenantId, CreateFloorPlanDto Dto) : IRequest<Result<FloorPlanDto>>;
public record AddDiningAreaCommand(Guid FloorPlanId, AddDiningAreaDto Dto) : IRequest<Result<DiningAreaDto>>;
public record AddRestaurantTableCommand(Guid DiningAreaId, AddRestaurantTableDto Dto) : IRequest<Result<RestaurantTableDto>>;
public record OccupyTableCommand(Guid TableId, Guid OrderId) : IRequest<Result>;
public record ReleaseTableCommand(Guid TableId) : IRequest<Result>;
public record UpdateTableStatusCommand(Guid TableId, UpdateTableStatusDto Dto) : IRequest<Result>;

// Reservations
public record CreateTableReservationCommand(Guid TenantId, CreateTableReservationDto Dto) : IRequest<Result<TableReservationDto>>;
public record ConfirmTableReservationCommand(Guid Id) : IRequest<Result>;
public record CancelTableReservationCommand(Guid Id, CancelTableReservationDto Dto) : IRequest<Result>;
public record SeatTableReservationCommand(Guid Id, Guid TenantId, Guid CashierId, SeatReservationDto Dto) : IRequest<Result<OrderDto>>;
