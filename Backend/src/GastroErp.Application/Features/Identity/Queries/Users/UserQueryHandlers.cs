using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Identity.DTOs;
using GastroErp.Domain.Common.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Identity.Queries.Users;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetUsersQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;
        var query = _context.AppUsers.AsNoTracking().AsQueryable();
        if (tenantId != Guid.Empty)
            query = query.Where(u => u.TenantId == tenantId);

        if (request.IsActive == true)
            query = query.Where(u => u.IsActive);
        if (request.IsActive == false)
            query = query.Where(u => !u.IsActive);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var s = request.SearchTerm.Trim().ToLower();
            query = query.Where(u =>
                u.UserName.ToLower().Contains(s) ||
                u.FirstName.ToLower().Contains(s) ||
                u.LastName.ToLower().Contains(s) ||
                u.Email.ToLower().Contains(s) ||
                (u.Code != null && u.Code.ToLower().Contains(s)) ||
                (u.MobileNumber != null && u.MobileNumber.Contains(s)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(s)));
        }

        if (request.BranchId.HasValue)
        {
            var branchId = request.BranchId.Value;
            query = query.Where(u =>
                _context.UserBranches.Any(ub => ub.UserId == u.Id && ub.BranchId == branchId));
        }

        if (request.RoleId.HasValue)
        {
            var roleId = request.RoleId.Value;
            query = query.Where(u =>
                _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == roleId));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(request.PageSize, 1, 500);
        var page = Math.Max(request.PageNumber, 1);

        var users = await query
            .OrderBy(u => u.Code)
            .ThenBy(u => u.UserName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var userIds = users.Select(u => u.Id).ToList();
        var roleLinks = await _context.UserRoles.AsNoTracking()
            .Where(ur => userIds.Contains(ur.UserId))
            .ToListAsync(cancellationToken);
        var branchLinks = await _context.UserBranches.AsNoTracking()
            .Where(ub => userIds.Contains(ub.UserId))
            .ToListAsync(cancellationToken);

        var roleIds = roleLinks.Select(r => r.RoleId).Distinct().ToList();
        var branchIds = branchLinks.Select(b => b.BranchId).Distinct().ToList();

        var roles = await _context.Roles.AsNoTracking()
            .Where(r => roleIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, cancellationToken);
        var branches = await _context.Branches.AsNoTracking()
            .Where(b => branchIds.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, cancellationToken);

        var dtos = users.Select((u, index) =>
        {
            var roleLink = roleLinks.FirstOrDefault(r => r.UserId == u.Id);
            var branchLink = branchLinks.FirstOrDefault(b => b.UserId == u.Id && b.IsDefault)
                             ?? branchLinks.FirstOrDefault(b => b.UserId == u.Id);
            roles.TryGetValue(roleLink?.RoleId ?? Guid.Empty, out var role);
            branches.TryGetValue(branchLink?.BranchId ?? Guid.Empty, out var branch);
            var number = int.TryParse(u.Code, out var parsed)
                ? parsed
                : ((page - 1) * pageSize) + index + 1;

            return new UserDto
            {
                Id = u.Id,
                Number = number,
                Code = u.Code,
                UserName = u.UserName,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                FullName = u.FullName,
                PhoneNumber = u.PhoneNumber,
                MobileNumber = u.MobileNumber,
                AvatarUrl = u.AvatarUrl,
                PreferredLanguage = u.PreferredLanguage,
                IsActive = u.IsActive,
                IsEmailVerified = u.IsEmailVerified,
                IsPosUser = u.IsPosUser,
                MustChangePassword = u.MustChangePassword,
                LastLoginAt = u.LastLoginAt,
                CreatedAt = u.CreatedAt,
                IsLocked = u.IsLocked,
                BranchId = branchLink?.BranchId,
                BranchNameAr = branch?.NameAr,
                RoleId = roleLink?.RoleId,
                RoleName = role?.Name,
                RoleNameAr = role?.NameAr
            };
        }).ToList();

        return PagedResult<UserDto>.Success(dtos, page, pageSize, totalCount);
    }
}

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUserByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.AppUsers.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user is null)
            return Result<UserDto>.Failure(ErrorCodes.UserNotFound, "User not found.");

        var roleLink = await _context.UserRoles.AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == user.Id, cancellationToken);
        var branchLink = await _context.UserBranches.AsNoTracking()
            .Where(b => b.UserId == user.Id)
            .OrderByDescending(b => b.IsDefault)
            .FirstOrDefaultAsync(cancellationToken);

        string? roleName = null, roleNameAr = null;
        if (roleLink is not null)
        {
            var role = await _context.Roles.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == roleLink.RoleId, cancellationToken);
            roleName = role?.Name;
            roleNameAr = role?.NameAr;
        }

        string? branchName = null;
        if (branchLink is not null)
        {
            branchName = await _context.Branches.AsNoTracking()
                .Where(b => b.Id == branchLink.BranchId)
                .Select(b => b.NameAr)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return Result<UserDto>.Success(new UserDto
        {
            Id = user.Id,
            Number = int.TryParse(user.Code, out var n) ? n : 0,
            Code = user.Code,
            UserName = user.UserName,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            MobileNumber = user.MobileNumber,
            AvatarUrl = user.AvatarUrl,
            PreferredLanguage = user.PreferredLanguage,
            IsActive = user.IsActive,
            IsEmailVerified = user.IsEmailVerified,
            IsPosUser = user.IsPosUser,
            MustChangePassword = user.MustChangePassword,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            IsLocked = user.IsLocked,
            BranchId = branchLink?.BranchId,
            BranchNameAr = branchName,
            RoleId = roleLink?.RoleId,
            RoleName = roleName,
            RoleNameAr = roleNameAr
        });
    }
}

public class GetUserLicenseStatusQueryHandler : IRequestHandler<GetUserLicenseStatusQuery, Result<UserLicenseStatusDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetUserLicenseStatusQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<UserLicenseStatusDto>> Handle(
        GetUserLicenseStatusQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;
        var currentUsers = await _context.AppUsers.AsNoTracking()
            .CountAsync(u => tenantId == Guid.Empty || u.TenantId == tenantId, cancellationToken);

        var subscription = await _context.Subscriptions.AsNoTracking()
            .Where(s => tenantId == Guid.Empty || s.TenantId == tenantId)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync(cancellationToken);

        var maxUsers = subscription?.MaxUsers ?? 999999;
        var isUnlimited = maxUsers < 0;
        var displayMax = isUnlimited ? 999999 : maxUsers;
        var statusText = subscription?.Status.ToString() ?? "Trial";
        var isTrial = subscription is null
            || statusText.Contains("Trial", StringComparison.OrdinalIgnoreCase)
            || displayMax >= 999999;

        var label = isTrial
            ? $"Users: {currentUsers} / {displayMax} (local trial mode)"
            : $"Users: {currentUsers} / {displayMax}";

        return Result<UserLicenseStatusDto>.Success(
            new UserLicenseStatusDto(currentUsers, displayMax, isUnlimited, isTrial, label));
    }
}
