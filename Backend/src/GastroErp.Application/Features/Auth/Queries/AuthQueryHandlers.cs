using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Auth.Queries;

public class AuthQueryHandlers : IRequestHandler<GetCurrentUserQuery, Result<CurrentUserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public AuthQueryHandlers(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<CurrentUserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.Id is null || _currentUser.Id == Guid.Empty)
        {
            return Result<CurrentUserDto>.Failure("Unauthorized", "User is not authenticated.");
        }

        var user = await _context.AppUsers
            .FirstOrDefaultAsync(u => u.Id == _currentUser.Id, cancellationToken);

        if (user is null)
        {
            return Result<CurrentUserDto>.Failure("NotFound", "User not found.");
        }

        var roles = await (
            from userRole in _context.UserRoles
            join role in _context.Roles on userRole.RoleId equals role.Id
            where userRole.UserId == user.Id
            select role.Name
        ).ToArrayAsync(cancellationToken);

        return Result<CurrentUserDto>.Success(new CurrentUserDto(
            user.Id.ToString(),
            user.Email,
            user.FullName,
            user.TenantId,
            roles,
            Array.Empty<string>()));
    }
}
