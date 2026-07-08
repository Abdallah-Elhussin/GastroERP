using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Ai.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Ai.Commands;

public record RefreshForecastsCommand(Guid TenantId, RefreshForecastsDto Dto) : IRequest<Result<RefreshForecastsResultDto>>;
