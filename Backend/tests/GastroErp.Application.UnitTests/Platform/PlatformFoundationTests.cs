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
        var dto = CreateValidWizard() with
        {
            Admin = CreateValidWizard().Admin with { Password = "123", ConfirmPassword = "123" }
        };

        var result = await validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task RegisterCompanyValidator_ShouldRejectMismatchedPassword()
    {
        var validator = new RegisterCompanyValidator();
        var dto = CreateValidWizard() with
        {
            Admin = CreateValidWizard().Admin with { ConfirmPassword = "Different@123" }
        };

        var result = await validator.ValidateAsync(dto);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task RegisterCompanyValidator_ShouldAcceptValidWizard()
    {
        var validator = new RegisterCompanyValidator();
        var result = await validator.ValidateAsync(CreateValidWizard());

        Assert.True(result.IsValid);
    }

    private static RegisterCompanyDto CreateValidWizard() => new(
        new AdminAccountStepDto("Abdallah Admin", "owner@restaurant.test", "0501234567", "Admin@123", "Admin@123"),
        new CompanyDataStepDto(
            "مطعم جاسترو",
            "Gastro Restaurant",
            "Gastro Restaurant",
            null,
            "1010123456",
            "300000000000003",
            null,
            "+966501234567",
            "info@gastro.test",
            null,
            new CompanyAddressDto("SA", "Riyadh", "Riyadh", "Olaya", "King Fahd Rd", "12345", null, null)),
        new GeneralSettingsStepDto("ar", "SAR", false, null, "Asia/Riyadh", 1, "Gregorian"),
        new MainBranchStepDto("الفرع الرئيسي", "+966501234567", "branch@gastro.test", "King Fahd Road, Riyadh"));
}
