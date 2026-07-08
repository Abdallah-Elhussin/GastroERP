using AutoMapper;
using AutoMapper.QueryableExtensions;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Identity.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Identity.Queries.Users;

public class UserQueryHandlers : 
    IRequestHandler<GetUsersQuery, Result<PagedResult<UserDto>>>,
    IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UserQueryHandlers(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AppUsers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(u => u.FirstName.ToLower().Contains(searchTerm) || 
                                     u.LastName.ToLower().Contains(searchTerm) || 
                                     u.Email.ToLower().Contains(searchTerm));
        }

        var count = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var pagedResult = PagedResult<UserDto>.Success(items, request.PageNumber, request.PageSize, count);
        return Result<PagedResult<UserDto>>.Success(pagedResult);
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.AppUsers
            .AsNoTracking()
            .Where(u => u.Id == request.Id)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null) return Result<UserDto>.Failure("User not found.");

        return Result<UserDto>.Success(user);
    }
}
