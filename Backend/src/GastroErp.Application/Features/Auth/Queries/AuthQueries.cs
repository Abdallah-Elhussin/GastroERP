using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Auth.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Auth.Queries;

public record GetCurrentUserQuery() : IRequest<Result<CurrentUserDto>>;
