using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Sales.Pricing;

/// <summary>
/// سعر بيع منتج (Aggregate Root) — منفصل تماماً عن بيانات المنتج الثابتة.
/// المنتج الواحد يمكنه امتلاك عدداً غير محدود من الأسعار.
/// </summary>
public sealed class ProductPrice : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }

    /// <summary>مرجع الصنف المخزني (Product Master منفصل عن التسعير).</summary>
    public Guid ProductId { get; private set; }

    public Guid? BranchId { get; private set; }
    public Guid PriceListId { get; private set; }
    public SalesChannel SalesChannel { get; private set; }
    public Guid UnitId { get; private set; }

    public PricingMethod PricingMethod { get; private set; }
    public ProductCostType CostType { get; private set; }

    public decimal Cost { get; private set; }
    public decimal ProfitMargin { get; private set; }
    public decimal ProfitAmount { get; private set; }
    public decimal SellingPrice { get; private set; }
    public decimal? MinimumPrice { get; private set; }
    public decimal? MaximumDiscount { get; private set; }

    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset? EndDate { get; private set; }

    public int Priority { get; private set; }
    public Guid? CurrencyId { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }
    public string? Notes { get; private set; }

    private ProductPrice() { }

    public ProductPrice(
        Guid tenantId,
        Guid productId,
        Guid priceListId,
        Guid unitId,
        PricingMethod pricingMethod,
        ProductCostType costType,
        decimal cost,
        decimal profitMargin,
        decimal profitAmount,
        decimal sellingPrice,
        DateTimeOffset startDate,
        Guid? branchId = null,
        SalesChannel salesChannel = SalesChannel.All,
        decimal? minimumPrice = null,
        decimal? maximumDiscount = null,
        DateTimeOffset? endDate = null,
        int priority = 0,
        Guid? currencyId = null,
        bool isDefault = false,
        string? notes = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (productId == Guid.Empty) throw new ArgumentException("ProductId cannot be empty.", nameof(productId));
        if (priceListId == Guid.Empty) throw new ArgumentException("PriceListId cannot be empty.", nameof(priceListId));
        if (unitId == Guid.Empty) throw new ArgumentException("UnitId cannot be empty.", nameof(unitId));

        TenantId = tenantId;
        ProductId = productId;
        BranchId = branchId;
        PriceListId = priceListId;
        SalesChannel = salesChannel;
        UnitId = unitId;
        PricingMethod = pricingMethod;
        CostType = costType;
        CurrencyId = currencyId;
        IsDefault = isDefault;
        Notes = notes?.Trim();
        IsActive = true;

        ApplyPricing(cost, profitMargin, profitAmount, sellingPrice, minimumPrice, maximumDiscount);
        SetValidity(startDate, endDate, priority);
    }

    public void Update(
        Guid? branchId,
        Guid priceListId,
        SalesChannel salesChannel,
        Guid unitId,
        PricingMethod pricingMethod,
        ProductCostType costType,
        decimal cost,
        decimal profitMargin,
        decimal profitAmount,
        decimal sellingPrice,
        decimal? minimumPrice,
        decimal? maximumDiscount,
        DateTimeOffset startDate,
        DateTimeOffset? endDate,
        int priority,
        Guid? currencyId,
        bool isDefault,
        string? notes)
    {
        if (priceListId == Guid.Empty) throw new ArgumentException("PriceListId cannot be empty.", nameof(priceListId));
        if (unitId == Guid.Empty) throw new ArgumentException("UnitId cannot be empty.", nameof(unitId));

        BranchId = branchId;
        PriceListId = priceListId;
        SalesChannel = salesChannel;
        UnitId = unitId;
        PricingMethod = pricingMethod;
        CostType = costType;
        CurrencyId = currencyId;
        IsDefault = isDefault;
        Notes = notes?.Trim();

        ApplyPricing(cost, profitMargin, profitAmount, sellingPrice, minimumPrice, maximumDiscount);
        SetValidity(startDate, endDate, priority);
    }

    public void RefreshCost(decimal cost)
    {
        if (cost < 0) throw new ArgumentOutOfRangeException(nameof(cost));
        Cost = decimal.Round(cost, 4, MidpointRounding.AwayFromZero);
        SellingPrice = CalculateSellingPrice(PricingMethod, Cost, ProfitMargin, ProfitAmount, SellingPrice);
        EnsureMinimumPrice();
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public void SoftDeletePrice(string? deletedBy)
    {
        SoftDelete(deletedBy);
        IsActive = false;
    }

    /// <summary>هل الفترة الزمنية متداخلة مع فترة أخرى (نفس المنتج/الوحدة/الفرع/القائمة/القناة).</summary>
    public bool OverlapsWith(DateTimeOffset otherStart, DateTimeOffset? otherEnd)
    {
        var thisEnd = EndDate ?? DateTimeOffset.MaxValue;
        var thatEnd = otherEnd ?? DateTimeOffset.MaxValue;
        return StartDate <= thatEnd && otherStart <= thisEnd;
    }

    public static decimal CalculateSellingPrice(
        PricingMethod method,
        decimal cost,
        decimal profitMargin,
        decimal profitAmount,
        decimal manualOrFixedPrice)
    {
        return method switch
        {
            PricingMethod.CostPlusMarginPercent =>
                decimal.Round(cost + cost * (profitMargin / 100m), 4, MidpointRounding.AwayFromZero),
            PricingMethod.CostPlusFixedProfit =>
                decimal.Round(cost + profitAmount, 4, MidpointRounding.AwayFromZero),
            PricingMethod.Fixed or PricingMethod.Manual =>
                decimal.Round(manualOrFixedPrice, 4, MidpointRounding.AwayFromZero),
            _ => decimal.Round(manualOrFixedPrice, 4, MidpointRounding.AwayFromZero)
        };
    }

    private void ApplyPricing(
        decimal cost,
        decimal profitMargin,
        decimal profitAmount,
        decimal sellingPrice,
        decimal? minimumPrice,
        decimal? maximumDiscount)
    {
        if (cost < 0) throw new ArgumentOutOfRangeException(nameof(cost));
        if (profitMargin < 0) throw new ArgumentOutOfRangeException(nameof(profitMargin));
        if (profitAmount < 0) throw new ArgumentOutOfRangeException(nameof(profitAmount));
        if (sellingPrice < 0) throw new ArgumentOutOfRangeException(nameof(sellingPrice));
        if (minimumPrice is < 0) throw new ArgumentOutOfRangeException(nameof(minimumPrice));
        if (maximumDiscount is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(maximumDiscount));

        Cost = decimal.Round(cost, 4, MidpointRounding.AwayFromZero);
        ProfitMargin = decimal.Round(profitMargin, 4, MidpointRounding.AwayFromZero);
        ProfitAmount = decimal.Round(profitAmount, 4, MidpointRounding.AwayFromZero);
        MinimumPrice = minimumPrice.HasValue
            ? decimal.Round(minimumPrice.Value, 4, MidpointRounding.AwayFromZero)
            : null;
        MaximumDiscount = maximumDiscount.HasValue
            ? decimal.Round(maximumDiscount.Value, 4, MidpointRounding.AwayFromZero)
            : null;

        SellingPrice = CalculateSellingPrice(PricingMethod, Cost, ProfitMargin, ProfitAmount, sellingPrice);
        EnsureMinimumPrice();
    }

    private void EnsureMinimumPrice()
    {
        if (MinimumPrice.HasValue && SellingPrice < MinimumPrice.Value)
            throw new BusinessException("SellingPriceBelowMinimum");
    }

    private void SetValidity(DateTimeOffset startDate, DateTimeOffset? endDate, int priority)
    {
        if (endDate.HasValue && endDate.Value < startDate)
            throw new BusinessException("InvalidPricePeriod");

        StartDate = startDate;
        EndDate = endDate;
        Priority = Math.Max(0, priority);
    }
}
