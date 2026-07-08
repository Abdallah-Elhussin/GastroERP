using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Delivery.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Delivery.Commands;

// Zones
public record CreateDeliveryZoneCommand(Guid TenantId, CreateDeliveryZoneDto Dto) : IRequest<Result<DeliveryZoneDto>>;
public record UpdateDeliveryZoneCommand(Guid Id, UpdateDeliveryZoneDto Dto) : IRequest<Result>;

// Drivers
public record CreateDeliveryDriverCommand(Guid TenantId, CreateDeliveryDriverDto Dto) : IRequest<Result<DeliveryDriverDto>>;
public record UpdateDeliveryDriverCommand(Guid Id, UpdateDeliveryDriverDto Dto) : IRequest<Result>;
public record UpdateDriverStatusCommand(Guid Id, UpdateDriverStatusDto Dto) : IRequest<Result>;

// Orders
public record CreateDeliveryOrderCommand(Guid TenantId, CreateDeliveryOrderDto Dto) : IRequest<Result<DeliveryOrderDto>>;
public record AssignDeliveryCommand(Guid Id, Guid UserId, AssignDeliveryDto Dto) : IRequest<Result>;
public record PickUpDeliveryCommand(Guid Id, PickUpDeliveryDto Dto) : IRequest<Result>;
public record CompleteDeliveryCommand(Guid Id, Guid UserId, CompleteDeliveryDto Dto) : IRequest<Result>;
public record FailDeliveryCommand(Guid Id, FailDeliveryDto Dto) : IRequest<Result>;
public record CancelDeliveryCommand(Guid Id, CancelDeliveryDto Dto) : IRequest<Result>;
