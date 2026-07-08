using GastroErp.Application.Common.Authorization;
using GastroErp.Application.Features.Onboarding.DTOs;
using GastroErp.Application.Features.Onboarding.Validators;
using Xunit;

namespace GastroErp.Application.UnitTests.Platform;

public class PlatformFoundationTests
{
    [Fact]
    public void PermissionCatalog_ShouldExposeCorePermissions()
    {
        var permissions = PermissionCatalog.GetAll();

        Assert.NotEmpty(permissions);
        Assert.Contains(permissions, p => p.Name == Permissions.Organization.View);
        Assert.Contains(permissions, p => p.Name == Permissions.Identity.RolesView);
        Assert.Contains(permissions, p => p.Name == Permissions.Reporting.View);
    }

    [Fact]
    public async Task RegisterCompanyValidator_ShouldRejectShortPassword()
    {
        var validator = new RegisterCompanyValidator();
        RegisterCompanyDto dto = new(
            "Restaurant One",
            "Ahmed",
            "admin@restaurant.com",
            "123",
            "0500000000",
            "SA",
            "Trial");
        var result = await validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
    }
}
