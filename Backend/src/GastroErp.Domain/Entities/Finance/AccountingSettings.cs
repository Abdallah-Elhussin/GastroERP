using GastroErp.Domain.Common;

namespace GastroErp.Domain.Entities.Finance;

/// <summary>
/// Tenant-level chart-of-accounts settings: numbering policy, default GL mapping, and auto-posting flags.
/// One row per tenant (optional CompanyId reserved for future company overlay).
/// </summary>
public sealed class AccountingSettings : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }

    /// <summary>Reserved for multi-company overlay (null = tenant-wide defaults).</summary>
    public Guid? CompanyId { get; private set; }

    // ── Numbering policy ──────────────────────────────────────────────────────
    public int AccountNumberMaxLength { get; private set; } = 20;
    public int MaxTreeLevels { get; private set; } = 4;
    public string LevelLengthsCsv { get; private set; } = "4,2,3";
    public string LevelSeparator { get; private set; } = "-";

    // ── Default system accounts (nullable until mapped) ───────────────────────
    public Guid? CashAccountId { get; private set; }
    public Guid? BankAccountId { get; private set; }
    public Guid? InventoryAccountId { get; private set; }
    public Guid? CogsAccountId { get; private set; }
    public Guid? SalesRevenueAccountId { get; private set; }
    public Guid? PurchaseAccountId { get; private set; }
    public Guid? AccountsReceivableAccountId { get; private set; }
    public Guid? AccountsPayableAccountId { get; private set; }
    public Guid? VatInputAccountId { get; private set; }
    public Guid? VatOutputAccountId { get; private set; }
    public Guid? DiscountAccountId { get; private set; }
    public Guid? RoundOffAccountId { get; private set; }
    public Guid? OpeningBalanceAccountId { get; private set; }
    public Guid? RetainedEarningsAccountId { get; private set; }
    public Guid? PayrollExpenseAccountId { get; private set; }
    public Guid? PayrollLiabilityAccountId { get; private set; }
    public Guid? ProductionVarianceAccountId { get; private set; }
    public Guid? InventoryAdjustmentAccountId { get; private set; }
    public Guid? WasteAccountId { get; private set; }
    public Guid? DeliveryRevenueAccountId { get; private set; }
    public Guid? DeliveryExpenseAccountId { get; private set; }
    public Guid? KitchenConsumptionAccountId { get; private set; }
    public Guid? CustomerAdvancesAccountId { get; private set; }
    public Guid? SupplierAdvancesAccountId { get; private set; }
    public Guid? ExchangeDifferenceAccountId { get; private set; }
    /// <summary>Goods received not invoiced (GRNI) clearing account.</summary>
    public Guid? GrniAccountId { get; private set; }
    /// <summary>Default fixed-asset account for direct asset purchases.</summary>
    public Guid? FixedAssetAccountId { get; private set; }

    // ── Auto-posting switches ─────────────────────────────────────────────────
    public bool AutoPostSales { get; private set; } = true;
    public bool AutoPostPurchases { get; private set; }
    public bool AutoPostGoodsReceipt { get; private set; }
    public bool AutoPostGoodsIssue { get; private set; }
    public bool AutoPostStockTransfer { get; private set; }
    public bool AutoPostWaste { get; private set; }
    public bool AutoPostProduction { get; private set; }
    public bool AutoPostPayroll { get; private set; } = true;

    private AccountingSettings() { }

    public static AccountingSettings CreateDefault(Guid tenantId, Guid? companyId = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        return new AccountingSettings
        {
            TenantId = tenantId,
            CompanyId = companyId
        };
    }

    public void UpdateNumbering(int maxLength, int maxLevels, string levelLengthsCsv, string separator)
    {
        AccountNumberMaxLength = Math.Clamp(maxLength, 4, 40);
        MaxTreeLevels = Math.Clamp(maxLevels, 1, 10);
        LevelLengthsCsv = string.IsNullOrWhiteSpace(levelLengthsCsv) ? "4,2,3" : levelLengthsCsv.Trim();
        LevelSeparator = string.IsNullOrWhiteSpace(separator) ? "-" : separator.Trim();
    }

    public void UpdateAccountMappings(
        Guid? cash, Guid? bank, Guid? inventory, Guid? cogs, Guid? salesRevenue, Guid? purchase,
        Guid? ar, Guid? ap, Guid? vatInput, Guid? vatOutput, Guid? discount, Guid? roundOff,
        Guid? openingBalance, Guid? retainedEarnings, Guid? payrollExpense, Guid? payrollLiability,
        Guid? productionVariance, Guid? inventoryAdjustment, Guid? waste, Guid? deliveryRevenue,
        Guid? deliveryExpense, Guid? kitchenConsumption, Guid? customerAdvances, Guid? supplierAdvances,
        Guid? exchangeDifference, Guid? grni = null, Guid? fixedAsset = null)
    {
        CashAccountId = cash;
        BankAccountId = bank;
        InventoryAccountId = inventory;
        CogsAccountId = cogs;
        SalesRevenueAccountId = salesRevenue;
        PurchaseAccountId = purchase;
        AccountsReceivableAccountId = ar;
        AccountsPayableAccountId = ap;
        VatInputAccountId = vatInput;
        VatOutputAccountId = vatOutput;
        DiscountAccountId = discount;
        RoundOffAccountId = roundOff;
        OpeningBalanceAccountId = openingBalance;
        RetainedEarningsAccountId = retainedEarnings;
        PayrollExpenseAccountId = payrollExpense;
        PayrollLiabilityAccountId = payrollLiability;
        ProductionVarianceAccountId = productionVariance;
        InventoryAdjustmentAccountId = inventoryAdjustment;
        WasteAccountId = waste;
        DeliveryRevenueAccountId = deliveryRevenue;
        DeliveryExpenseAccountId = deliveryExpense;
        KitchenConsumptionAccountId = kitchenConsumption;
        CustomerAdvancesAccountId = customerAdvances;
        SupplierAdvancesAccountId = supplierAdvances;
        ExchangeDifferenceAccountId = exchangeDifference;
        GrniAccountId = grni;
        FixedAssetAccountId = fixedAsset;
    }

    public void UpdatePostingFlags(
        bool sales, bool purchases, bool goodsReceipt, bool goodsIssue,
        bool stockTransfer, bool waste, bool production, bool payroll)
    {
        AutoPostSales = sales;
        AutoPostPurchases = purchases;
        AutoPostGoodsReceipt = goodsReceipt;
        AutoPostGoodsIssue = goodsIssue;
        AutoPostStockTransfer = stockTransfer;
        AutoPostWaste = waste;
        AutoPostProduction = production;
        AutoPostPayroll = payroll;
    }
}
