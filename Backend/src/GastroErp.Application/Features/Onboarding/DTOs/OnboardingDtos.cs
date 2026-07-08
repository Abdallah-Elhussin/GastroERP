namespace GastroErp.Application.Features.Onboarding.DTOs;

public record RegisterCompanyDto(
    string CompanyName,
    string OwnerName,
    string Email,
    string Password,
    string? Phone,
    string Country,
    string Subscription = "Trial");

public record RegisterCompanyResponseDto(
    Guid TenantId,
    Guid CompanyId,
    Guid UserId,
    string Token,
    string RefreshToken);
