using GastroErp.Domain.Entities.Finance;

namespace GastroErp.Application.Features.Finance.DTOs;

public record TaxCodeRateDto(
    Guid? Id,
    DateOnly FromDate,
    DateOnly? ToDate,
    decimal Rate);

public record UpsertTaxCodeDto(
    Guid CompanyId,
    string Code,
    string NameAr,
    string? NameEn = null,
    Guid? BranchId = null,
    TaxAppliesTo AppliesTo = TaxAppliesTo.Both,
    TaxCodeCalculationMethod CalculationMethod = TaxCodeCalculationMethod.Standard,
    Guid? SalesAccountId = null,
    Guid? PurchaseAccountId = null,
    bool PriceIncludesTax = false,
    bool IsActive = true,
    IReadOnlyList<TaxCodeRateDto>? Rates = null);

public record TaxCodeDto(
    Guid Id,
    int Number,
    Guid CompanyId,
    string? CompanyNameAr,
    Guid? BranchId,
    string? BranchNameAr,
    string Code,
    string NameAr,
    string? NameEn,
    TaxAppliesTo AppliesTo,
    TaxCodeCalculationMethod CalculationMethod,
    Guid? SalesAccountId,
    string? SalesAccountNumber,
    string? SalesAccountNameAr,
    Guid? PurchaseAccountId,
    string? PurchaseAccountNumber,
    string? PurchaseAccountNameAr,
    bool PriceIncludesTax,
    bool IsActive,
    bool HasBeenUsed,
    decimal? CurrentRate,
    DateTimeOffset CreatedAt,
    string? CreatedBy,
    DateTimeOffset? UpdatedAt,
    string? UpdatedBy,
    IReadOnlyList<TaxCodeRateDto> Rates);

public record TaxCodeFilterDto(
    Guid? CompanyId = null,
    Guid? BranchId = null,
    TaxAppliesTo? AppliesTo = null,
    bool? IsActive = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 200);
