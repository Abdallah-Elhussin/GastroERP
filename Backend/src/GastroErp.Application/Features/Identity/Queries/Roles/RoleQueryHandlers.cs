using AutoMapper;
using AutoMapper.QueryableExtensions;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Identity.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Identity.Queries.Roles;

public class RoleQueryHandlers : 
    IRequestHandler<GetRolesQuery, Result<IReadOnlyList<RoleDto>>>,
    IRequestHandler<GetRoleByIdQuery, Result<RoleDto>>,
    IRequestHandler<GetPermissionsQuery, Result<IReadOnlyList<PermissionDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUserService;

    public RoleQueryHandlers(IApplicationDbContext context, IMapper mapper, ICurrentUser currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IReadOnlyList<RoleDto>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        // Get roles for current tenant + system roles
        var tenantId = _currentUserService.TenantId;
        var roles = await _context.Roles
            .AsNoTracking()
            .Where(r => r.IsSystem || r.TenantId == tenantId)
            .ProjectTo<RoleDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<RoleDto>>.Success(roles);
    }

    public async Task<Result<RoleDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .AsNoTracking()
            .Where(r => r.Id == request.Id)
            .ProjectTo<RoleDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (role == null) return Result<RoleDto>.Failure("Role not found.");

        return Result<RoleDto>.Success(role);
    }

    public async Task<Result<IReadOnlyList<PermissionDto>>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await _context.Permissions
            .AsNoTracking()
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Name)
            .Select(p => new PermissionDto(p.Id, p.Module, p.Name, p.DisplayName))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<PermissionDto>>.Success(permissions);
    }
}
