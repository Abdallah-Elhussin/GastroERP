using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Invoicing;

/// <summary>TaxRate — معدل ضريبة (Aggregate Root)</summary>
public sealed class TaxRate : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string Code { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public TaxType TaxType { get; private set; }
    public TaxCalculationMethod CalculationMethod { get; private set; }
    public decimal Rate { get; private set; }
    public decimal? FixedAmount { get; private set; }
    public bool IsInclusive { get; private set; }
    public bool IsActive { get; private set; }
    public string? Description { get; private set; }

    private TaxRate()
    {
        Code = string.Empty;
        NameAr = string.Empty;
    }

    public static TaxRate Create(
        Guid tenantId, string code, string nameAr, TaxType taxType,
        TaxCalculationMethod calculationMethod, decimal rate, bool isInclusive = false,
        string? nameEn = null, decimal? fixedAmount = null, string? description = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(code)) throw new BusinessException(ErrorCodes.RequiredField);
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        if (rate < 0 || (fixedAmount.HasValue && fixedAmount < 0))
            throw new BusinessException(ErrorCodes.InvalidTaxAmount);

        return new TaxRate
        {
            TenantId = tenantId,
            Code = code.ToUpperInvariant(),
            NameAr = nameAr,
            NameEn = nameEn,
            TaxType = taxType,
            CalculationMethod = calculationMethod,
            Rate = rate,
            FixedAmount = fixedAmount,
            IsInclusive = isInclusive,
            IsActive = true,
            Description = description
        };
    }

    public void Update(string nameAr, TaxType taxType, TaxCalculationMethod calculationMethod,
        decimal rate, bool isInclusive, string? nameEn = null, decimal? fixedAmount = null, string? description = null)
    {
        if (rate < 0 || (fixedAmount.HasValue && fixedAmount < 0))
            throw new BusinessException(ErrorCodes.InvalidTaxAmount);

        NameAr = nameAr;
        NameEn = nameEn;
        TaxType = taxType;
        CalculationMethod = calculationMethod;
        Rate = rate;
        FixedAmount = fixedAmount;
        IsInclusive = isInclusive;
        Description = description;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public decimal CalculateTax(decimal taxableAmount)
    {
        if (taxableAmount < 0) throw new BusinessException(ErrorCodes.InvalidTaxAmount);

        return CalculationMethod switch
        {
            TaxCalculationMethod.Percentage => Math.Round(taxableAmount * Rate / 100m, 4),
            TaxCalculationMethod.FixedAmount => FixedAmount ?? 0,
            _ => 0
        };
    }
}
