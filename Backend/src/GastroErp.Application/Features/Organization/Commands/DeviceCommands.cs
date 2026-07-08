using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Organization.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Organization.Commands;

public record CreateDeviceCommand(CreateDeviceDto Dto) : IRequest<Result<DeviceDto>>;
public record UpdateDeviceCommand(Guid Id, UpdateDeviceDto Dto) : IRequest<Result>;
public record ActivateDeviceCommand(Guid Id, Guid BranchId) : IRequest<Result>;
public record DeactivateDeviceCommand(Guid Id) : IRequest<Result>;
public record AssignDeviceCommand(Guid DeviceId, Guid BranchId, string? AssignedBy) : IRequest<Result>;
public record UnassignDeviceCommand(Guid DeviceId, Guid BranchId, string? UnassignedBy) : IRequest<Result>;
