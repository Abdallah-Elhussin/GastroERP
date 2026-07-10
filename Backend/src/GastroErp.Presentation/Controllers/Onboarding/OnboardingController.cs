using Asp.Versioning;
using GastroErp.Application.Features.Onboarding.Commands;
using GastroErp.Application.Features.Onboarding.DTOs;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Onboarding;

/// <summary>
/// Tenant onboarding and company registration
/// </summary>
[ApiVersion("1.0")]
public class OnboardingController : BaseApiController
{
    /// <summary>
    /// Register a new restaurant company via the 4-step onboarding wizard
    /// </summary>
    [HttpPost(ApiRoutes.Onboarding.RegisterCompany)]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterCompany([FromBody] RegisterCompanyDto request)
        => HandleResult(await Mediator.Send(new RegisterCompanyCommand(request)));

    /// <summary>
    /// Alias for the restaurant setup wizard (same payload as register-company)
    /// </summary>
    [HttpPost(ApiRoutes.Onboarding.SetupRestaurant)]
    [AllowAnonymous]
    public async Task<IActionResult> SetupRestaurant([FromBody] RegisterCompanyDto request)
        => HandleResult(await Mediator.Send(new RegisterCompanyCommand(request)));
}
