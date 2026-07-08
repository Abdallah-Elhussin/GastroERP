using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Ai.Commands;
using GastroErp.Application.Features.Ai.DTOs;
using GastroErp.Application.Features.Ai.Services;
using MediatR;

namespace GastroErp.Application.Features.Ai.Commands;

public sealed class RefreshRecommendationsCommandHandler
    : IRequestHandler<RefreshRecommendationsCommand, Result<RefreshRecommendationsResultDto>>
{
    private readonly IRecommendationActionService _service;
    public RefreshRecommendationsCommandHandler(IRecommendationActionService service) => _service = service;

    public async Task<Result<RefreshRecommendationsResultDto>> Handle(RefreshRecommendationsCommand request, CancellationToken ct)
    {
        var count = await _service.PersistRecommendationsAsync(request.TenantId, request.Dto, ct);
        return Result<RefreshRecommendationsResultDto>.Success(new RefreshRecommendationsResultDto(count));
    }
}

public sealed class ApplyRecommendationCommandHandler : IRequestHandler<ApplyRecommendationCommand, Result>
{
    private readonly IRecommendationActionService _service;
    public ApplyRecommendationCommandHandler(IRecommendationActionService service) => _service = service;

    public async Task<Result> Handle(ApplyRecommendationCommand request, CancellationToken ct)
    {
        await _service.ApplyAsync(request.TenantId, request.ActionId, request.UserId, ct);
        return Result.Success();
    }
}

public sealed class DismissRecommendationCommandHandler : IRequestHandler<DismissRecommendationCommand, Result>
{
    private readonly IRecommendationActionService _service;
    public DismissRecommendationCommandHandler(IRecommendationActionService service) => _service = service;

    public async Task<Result> Handle(DismissRecommendationCommand request, CancellationToken ct)
    {
        await _service.DismissAsync(request.TenantId, request.ActionId, request.UserId, request.Dto?.Reason, ct);
        return Result.Success();
    }
}
