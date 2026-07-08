using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Domain.Entities.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Identity.Commands.Roles;

public class RoleCommandHandlers : 
    IRequestHandler<CreateRoleCommand, Result<Guid>>,
    IRequestHandler<UpdateRoleCommand, Result>,
    IRequestHandler<DeleteRoleCommand, Result>,
    IRequestHandler<AssignPermissionsCommand, Result>,
    IRequestHandler<RemovePermissionsCommand, Result>,
    IRequestHandler<AssignRoleToUserCommand, Result>,
    IRequestHandler<RemoveRoleFromUserCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUserService;

    public RoleCommandHandlers(IApplicationDbContext context, ICurrentUser currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;
        if (tenantId == Guid.Empty) return Result<Guid>.Failure("Tenant ID is required.");

        var exists = await _context.Roles.AnyAsync(r => r.Name == request.Dto.Name && r.TenantId == tenantId, cancellationToken);
        if (exists) return Result<Guid>.Failure("Role with this name already exists.");

        var role = new Role(tenantId, request.Dto.Name, request.Dto.NameAr, request.Dto.Description);

        _context.Roles.Add(role);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(role.Id);
    }

    public async Task<Result> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (role == null) return Result.Failure("Role not found.");

        if (role.IsSystem) return Result.Failure("System roles cannot be updated.");

        var exists = await _context.Roles.AnyAsync(r => r.Name == request.Dto.Name && r.TenantId == role.TenantId && r.Id != role.Id, cancellationToken);
        if (exists) return Result.Failure("Role with this name already exists.");

        role.UpdateName(request.Dto.Name, request.Dto.NameAr);
        // Assuming Description update needs to be added, let's skip or modify entity later. We follow strict existing Domain.

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (role == null) return Result.Failure("Role not found.");

        if (role.IsSystem) return Result.Failure("System roles cannot be deleted.");

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(AssignPermissionsCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles.Include(r => r.Permissions).FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (role == null) return Result.Failure("Role not found.");

        if (role.IsSystem) return Result.Failure("System roles cannot be modified.");

        foreach (var permId in request.PermissionIds)
        {
            role.GrantPermission(permId);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(RemovePermissionsCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles.Include(r => r.Permissions).FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (role == null) return Result.Failure("Role not found.");

        if (role.IsSystem) return Result.Failure("System roles cannot be modified.");

        foreach (var permId in request.PermissionIds)
        {
            role.RevokePermission(permId);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.AppUsers.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null) return Result.Failure("User not found.");

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);
        if (role == null) return Result.Failure("Role not found.");

        user.AssignRole(request.RoleId);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(RemoveRoleFromUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.AppUsers.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null) return Result.Failure("User not found.");

        user.RemoveRole(request.RoleId);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
