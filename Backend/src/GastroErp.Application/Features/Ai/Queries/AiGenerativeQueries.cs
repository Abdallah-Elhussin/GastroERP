using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Ai.DTOs;
using GastroErp.Application.Features.Ai.Services;
using MediatR;

namespace GastroErp.Application.Features.Ai.Queries;

public record GetDashboardInsightsQuery(Guid TenantId, AiInsightFilterDto Filter)
    : IRequest<Result<AiDashboardInsightDto>>;
