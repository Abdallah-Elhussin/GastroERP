using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Onboarding.DTOs;

namespace GastroErp.Application.Features.Onboarding;

public interface IRestaurantOnboardingService
{
    Task<Result<RegisterCompanyResponseDto>> ProvisionAsync(
        RegisterCompanyDto wizard,
        CancellationToken cancellationToken = default);
}
