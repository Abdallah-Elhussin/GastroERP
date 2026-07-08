using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Identity.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Identity.Commands.Roles;

public record CreateRoleCommand(CreateRoleDto Dto) : IRequest<Result<Guid>>;
public record UpdateRoleCommand(Guid Id, UpdateRoleDto Dto) : IRequest<Result>;
public record DeleteRoleCommand(Guid Id) : IRequest<Result>;
public record AssignPermissionsCommand(Guid Id, List<Guid> PermissionIds) : IRequest<Result>;
public record RemovePermissionsCommand(Guid Id, List<Guid> PermissionIds) : IRequest<Result>;
public record AssignRoleToUserCommand(Guid UserId, Guid RoleId) : IRequest<Result>;
public record RemoveRoleFromUserCommand(Guid UserId, Guid RoleId) : IRequest<Result>;
