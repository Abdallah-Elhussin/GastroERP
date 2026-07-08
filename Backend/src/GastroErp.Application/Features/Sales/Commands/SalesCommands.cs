using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Sales.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Sales.Commands;

public record CreateOrderCommand(Guid TenantId, Guid CashierId, CreateOrderDto Dto) : IRequest<Result<OrderDto>>;
public record AddOrderItemCommand(Guid OrderId, AddOrderItemDto Dto) : IRequest<Result<OrderItemDto>>;
public record RemoveOrderItemCommand(Guid OrderId, Guid ItemId) : IRequest<Result>;
public record VoidOrderItemCommand(Guid OrderId, Guid ItemId, VoidOrderItemDto Dto) : IRequest<Result>;
public record ApplyOrderDiscountCommand(Guid OrderId, ApplyOrderDiscountDto Dto) : IRequest<Result>;
public record SubmitOrderCommand(Guid OrderId, Guid UserId, Guid DeviceId) : IRequest<Result>;
public record ConfirmOrderCommand(Guid OrderId, Guid UserId, Guid DeviceId) : IRequest<Result>;
public record UpdateOrderStatusCommand(Guid OrderId, Guid UserId, Guid DeviceId, UpdateOrderStatusDto Dto) : IRequest<Result>;
public record CompleteOrderCommand(Guid OrderId, Guid UserId, Guid DeviceId) : IRequest<Result>;
public record CancelOrderCommand(Guid OrderId, Guid UserId, Guid DeviceId, CancelOrderDto Dto) : IRequest<Result>;
public record ReopenOrderCommand(Guid OrderId, Guid UserId, Guid DeviceId, ReopenOrderDto Dto) : IRequest<Result>;
