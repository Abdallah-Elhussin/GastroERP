using GastroErp.Application.Features.Onboarding;
using GastroErp.Application.Features.Onboarding.Commands;
using GastroErp.Application.Features.Onboarding.DTOs;
using GastroErp.Application.Common.Responses;
using MediatR;

namespace GastroErp.Application.Features.Onboarding.Commands;

public sealed class RegisterCompanyCommandHandler : IRequestHandler<RegisterCompanyCommand, Result<RegisterCompanyResponseDto>>
{
    private readonly IRestaurantOnboardingService _onboardingService;

    public RegisterCompanyCommandHandler(IRestaurantOnboardingService onboardingService)
        => _onboardingService = onboardingService;

    public Task<Result<RegisterCompanyResponseDto>> Handle(RegisterCompanyCommand request, CancellationToken cancellationToken)
        => _onboardingService.ProvisionAsync(request.Dto, cancellationToken);
}
