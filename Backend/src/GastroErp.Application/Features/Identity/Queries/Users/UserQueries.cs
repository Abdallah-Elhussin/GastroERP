using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Identity.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Identity.Queries.Users;

public record GetUsersQuery(int PageNumber = 1, int PageSize = 10, string? SearchTerm = null) : IRequest<Result<PagedResult<UserDto>>>;
public record GetUserByIdQuery(Guid Id) : IRequest<Result<UserDto>>;
