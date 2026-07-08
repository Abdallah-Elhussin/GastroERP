using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Ai.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Ai.Commands;

public record RefreshRecommendationsCommand(Guid TenantId, RefreshRecommendationsDto Dto)
    : IRequest<Result<RefreshRecommendationsResultDto>>;

public record ApplyRecommendationCommand(Guid TenantId, Guid ActionId, Guid UserId, ApplyRecommendationDto? Dto = null)
    : IRequest<Result>;

public record DismissRecommendationCommand(Guid TenantId, Guid ActionId, Guid UserId, DismissRecommendationDto? Dto = null)
    : IRequest<Result>;
