using System;

namespace GastroErp.Application.Features.Organization.DTOs;

public record SubscriptionDto(
    Guid Id,
    Guid TenantId,
    Guid PlanId,
    string Status,
    string BillingCycle,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    int MaxBranches,
    int MaxUsers,
    int MaxDevices,
    decimal PriceAmount,
    string PriceCurrency,
    string? Notes
);

public record SubscriptionPlanDto(
    Guid Id,
    string Name,
    string NameAr,
    string? Description,
    string PlanType,
    decimal MonthlyPrice,
    decimal YearlyPrice,
    string Currency,
    int MaxBranches,
    int MaxUsers,
    int MaxDevices,
    int MaxProducts,
    bool IsActive,
    bool IsSystem,
    int SortOrder
);

public record CreateSubscriptionDto(
    Guid TenantId,
    Guid PlanId,
    string BillingCycle,
    int MaxBranches,
    int MaxUsers,
    int MaxDevices,
    decimal Price,
    string? Notes
);
