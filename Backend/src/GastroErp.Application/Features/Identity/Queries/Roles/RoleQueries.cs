using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Identity.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Identity.Queries.Roles;

public record GetRolesQuery() : IRequest<Result<IReadOnlyList<RoleDto>>>;
public record GetRoleByIdQuery(Guid Id) : IRequest<Result<RoleDto>>;

public record PermissionDto(Guid Id, string Module, string Name, string DisplayName);
public record GetPermissionsQuery() : IRequest<Result<IReadOnlyList<PermissionDto>>>;
public record GetRolePermissionsQuery(Guid RoleId) : IRequest<Result<IReadOnlyList<Guid>>>;
