using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Inventory.Settings;

/// <summary>
/// إعدادات المخزون للشركة/المستأجر (Aggregate Root) — سجل واحد لكل مستوى (Tenant أو فرع).
/// </summary>
public sealed class InventorySetting : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid? CompanyId { get; private set; }
    public Guid? BranchId { get; private set; }

    // ─── General ─────────────────────────────────────────────────────────────
    public Guid? DefaultWarehouseId { get; private set; }
    public Guid? DefaultUnitId { get; private set; }
    public string? DefaultCurrencyCode { get; private set; }
    public bool AutoGenerateItemCode { get; private set; }
    public bool EnableMultiWarehouse { get; private set; }
    public bool EnableWarehouseHierarchy { get; private set; }
    public bool EnableBatchTracking { get; private set; }
    public bool EnableSerialTracking { get; private set; }
    public bool EnableExpiryTracking { get; private set; }
    public bool EnableBarcode { get; private set; }
    public bool EnableQrCode { get; private set; }

    // ─── Costing ─────────────────────────────────────────────────────────────
    public InventoryCostingMethod CostingMethod { get; private set; }
    public byte CostPrecision { get; private set; }
    public bool RoundCost { get; private set; }
    public bool AutoRecalculateCost { get; private set; }

    // ─── Inventory Control ───────────────────────────────────────────────────
    public bool AllowNegativeStock { get; private set; }
    public bool CheckAvailableQuantity { get; private set; }
    public bool EnableReservation { get; private set; }
    public bool AutoReleaseReservation { get; private set; }
    public bool FreezeDuringCount { get; private set; }
    public bool AllowZeroCost { get; private set; }
    public bool AllowNegativeCost { get; private set; }
    public bool ValidateWarehouseBeforePosting { get; private set; }
    public bool AutoIssueRecipe { get; private set; }

    // ─── Posting ─────────────────────────────────────────────────────────────
    public bool RequireApprovalBeforePosting { get; private set; }
    public bool AutoPostAfterApproval { get; private set; }
    public bool AllowUnpost { get; private set; }
    public bool CreateReverseEntry { get; private set; }
    public bool LockPostedDocuments { get; private set; }
    public bool AllowEditDraft { get; private set; }
    public bool AllowDeleteDraft { get; private set; }

    // ─── Integrations ────────────────────────────────────────────────────────
    public bool EnablePurchasingIntegration { get; private set; }
    public bool EnablePosIntegration { get; private set; }
    public bool EnableProductionIntegration { get; private set; }
    public bool EnableAccountingIntegration { get; private set; }
    public bool EnableKitchenIntegration { get; private set; }
    public bool EnableDeliveryIntegration { get; private set; }

    // ─── Notifications ───────────────────────────────────────────────────────
    public bool LowStockAlert { get; private set; }
    public bool OutOfStockAlert { get; private set; }
    public bool NearExpiryAlert { get; private set; }
    public bool ExpiredItemsAlert { get; private set; }
    public bool CycleCountReminder { get; private set; }
    public bool EmailNotifications { get; private set; }
    public bool PushNotifications { get; private set; }

    // ─── Advanced ────────────────────────────────────────────────────────────
    public bool EnableMultiCompany { get; private set; }
    public bool EnableMultiBranch { get; private set; }
    public bool EnableWarehouseZones { get; private set; }
    public bool EnableShelves { get; private set; }
    public bool EnableBins { get; private set; }
    public bool EnableRfid { get; private set; }
    public bool EnableMobileScanner { get; private set; }

    public bool IsActive { get; private set; }

    private readonly List<InventoryDocumentNumberSeries> _documentSeries = [];
    public IReadOnlyCollection<InventoryDocumentNumberSeries> DocumentSeries => _documentSeries.AsReadOnly();

    private InventorySetting() { }

    public InventorySetting(Guid tenantId, Guid? branchId = null, Guid? companyId = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        TenantId = tenantId;
        BranchId = branchId;
        CompanyId = companyId;
        ApplyFactoryDefaults();
        EnsureDefaultDocumentSeries();
    }

    public void ResetToDefaults()
    {
        ApplyFactoryDefaults();
        foreach (var series in _documentSeries)
            series.ResetToDefault();
        EnsureDefaultDocumentSeries();
    }

    public void UpdateGeneral(
        Guid? defaultWarehouseId,
        Guid? defaultUnitId,
        string? defaultCurrencyCode,
        bool autoGenerateItemCode,
        bool enableMultiWarehouse,
        bool enableWarehouseHierarchy,
        bool enableBatchTracking,
        bool enableSerialTracking,
        bool enableExpiryTracking,
        bool enableBarcode,
        bool enableQrCode)
    {
        DefaultWarehouseId = defaultWarehouseId;
        DefaultUnitId = defaultUnitId;
        DefaultCurrencyCode = string.IsNullOrWhiteSpace(defaultCurrencyCode)
            ? null
            : defaultCurrencyCode.Trim().ToUpperInvariant();
        AutoGenerateItemCode = autoGenerateItemCode;
        EnableMultiWarehouse = enableMultiWarehouse;
        EnableWarehouseHierarchy = enableWarehouseHierarchy;
        EnableBatchTracking = enableBatchTracking;
        EnableSerialTracking = enableSerialTracking;
        EnableExpiryTracking = enableExpiryTracking;
        EnableBarcode = enableBarcode;
        EnableQrCode = enableQrCode;
    }

    public void UpdateCosting(
        InventoryCostingMethod costingMethod,
        byte costPrecision,
        bool roundCost,
        bool autoRecalculateCost,
        bool allowCostingChange)
    {
        if (!Enum.IsDefined(typeof(InventoryCostingMethod), costingMethod))
            throw new BusinessException(ErrorCodes.RequiredField);

        if (costingMethod != CostingMethod && !allowCostingChange)
            throw new BusinessException(ErrorCodes.CannotModifyApprovedDocument);

        if (costingMethod is not InventoryCostingMethod.WeightedAverage)
            throw new ArgumentException("Only Weighted Average costing is enabled currently.", nameof(costingMethod));

        CostingMethod = costingMethod;
        CostPrecision = Math.Clamp(costPrecision, (byte)0, (byte)6);
        RoundCost = roundCost;
        AutoRecalculateCost = autoRecalculateCost;
    }

    public void UpdateInventoryControl(
        bool allowNegativeStock,
        bool checkAvailableQuantity,
        bool enableReservation,
        bool autoReleaseReservation,
        bool freezeDuringCount,
        bool allowZeroCost,
        bool allowNegativeCost,
        bool validateWarehouseBeforePosting,
        bool autoIssueRecipe)
    {
        AllowNegativeStock = allowNegativeStock;
        CheckAvailableQuantity = checkAvailableQuantity;
        EnableReservation = enableReservation;
        AutoReleaseReservation = autoReleaseReservation;
        FreezeDuringCount = freezeDuringCount;
        AllowZeroCost = allowZeroCost;
        AllowNegativeCost = allowNegativeCost;
        ValidateWarehouseBeforePosting = validateWarehouseBeforePosting;
        AutoIssueRecipe = autoIssueRecipe;
    }

    public void UpdatePosting(
        bool requireApprovalBeforePosting,
        bool autoPostAfterApproval,
        bool allowUnpost,
        bool createReverseEntry,
        bool lockPostedDocuments,
        bool allowEditDraft,
        bool allowDeleteDraft)
    {
        RequireApprovalBeforePosting = requireApprovalBeforePosting;
        AutoPostAfterApproval = autoPostAfterApproval;
        AllowUnpost = allowUnpost;
        CreateReverseEntry = createReverseEntry;
        LockPostedDocuments = lockPostedDocuments;
        AllowEditDraft = allowEditDraft;
        AllowDeleteDraft = allowDeleteDraft;
    }

    public void UpdateIntegrations(
        bool purchasing,
        bool pos,
        bool production,
        bool accounting,
        bool kitchen,
        bool delivery)
    {
        EnablePurchasingIntegration = purchasing;
        EnablePosIntegration = pos;
        EnableProductionIntegration = production;
        EnableAccountingIntegration = accounting;
        EnableKitchenIntegration = kitchen;
        EnableDeliveryIntegration = delivery;
    }

    public void UpdateNotifications(
        bool lowStock,
        bool outOfStock,
        bool nearExpiry,
        bool expired,
        bool cycleCount,
        bool email,
        bool push)
    {
        LowStockAlert = lowStock;
        OutOfStockAlert = outOfStock;
        NearExpiryAlert = nearExpiry;
        ExpiredItemsAlert = expired;
        CycleCountReminder = cycleCount;
        EmailNotifications = email;
        PushNotifications = push;
    }

    public void UpdateAdvanced(
        bool multiCompany,
        bool multiBranch,
        bool zones,
        bool shelves,
        bool bins,
        bool rfid,
        bool mobileScanner)
    {
        EnableMultiCompany = multiCompany;
        EnableMultiBranch = multiBranch;
        EnableWarehouseZones = zones;
        EnableShelves = shelves;
        EnableBins = bins;
        EnableRfid = rfid;
        EnableMobileScanner = mobileScanner;
    }

    public void UpsertDocumentSeries(
        InventoryDocumentSeriesType documentType,
        string prefix,
        byte numberLength,
        long nextNumber,
        bool autoIncrement)
    {
        var existing = _documentSeries.FirstOrDefault(s => s.DocumentType == documentType);
        if (existing is null)
        {
            _documentSeries.Add(new InventoryDocumentNumberSeries(
                TenantId, Id, documentType, prefix, numberLength, nextNumber, autoIncrement));
            return;
        }

        existing.Update(prefix, numberLength, nextNumber, autoIncrement);
    }

    public void ReplaceDocumentSeries(IEnumerable<(InventoryDocumentSeriesType Type, string Prefix, byte Length, long Next, bool Auto)> series)
    {
        EnsureDefaultDocumentSeries();
        foreach (var item in series)
            UpsertDocumentSeries(item.Type, item.Prefix, item.Length, item.Next, item.Auto);
    }

    private void ApplyFactoryDefaults()
    {
        CostingMethod = InventoryCostingMethod.WeightedAverage;
        CostPrecision = 4;
        RoundCost = true;
        AutoRecalculateCost = true;

        AutoGenerateItemCode = true;
        EnableMultiWarehouse = true;
        EnableWarehouseHierarchy = true;
        EnableBatchTracking = false;
        EnableSerialTracking = false;
        EnableExpiryTracking = false;
        EnableBarcode = true;
        EnableQrCode = false;
        DefaultCurrencyCode = "SAR";

        AllowNegativeStock = false;
        CheckAvailableQuantity = true;
        EnableReservation = true;
        AutoReleaseReservation = true;
        FreezeDuringCount = true;
        AllowZeroCost = false;
        AllowNegativeCost = false;
        ValidateWarehouseBeforePosting = true;
        AutoIssueRecipe = true;

        RequireApprovalBeforePosting = false;
        AutoPostAfterApproval = true;
        AllowUnpost = false;
        CreateReverseEntry = true;
        LockPostedDocuments = true;
        AllowEditDraft = true;
        AllowDeleteDraft = true;

        EnablePurchasingIntegration = true;
        EnablePosIntegration = true;
        EnableProductionIntegration = true;
        EnableAccountingIntegration = true;
        EnableKitchenIntegration = true;
        EnableDeliveryIntegration = false;

        LowStockAlert = true;
        OutOfStockAlert = true;
        NearExpiryAlert = true;
        ExpiredItemsAlert = true;
        CycleCountReminder = false;
        EmailNotifications = false;
        PushNotifications = true;

        EnableMultiCompany = false;
        EnableMultiBranch = true;
        EnableWarehouseZones = true;
        EnableShelves = true;
        EnableBins = true;
        EnableRfid = false;
        EnableMobileScanner = true;

        IsActive = true;
        DefaultWarehouseId = null;
        DefaultUnitId = null;
    }

    private void EnsureDefaultDocumentSeries()
    {
        foreach (var type in Enum.GetValues<InventoryDocumentSeriesType>())
        {
            if (_documentSeries.Any(s => s.DocumentType == type)) continue;
            var (prefix, length) = InventoryDocumentNumberSeries.DefaultFor(type);
            _documentSeries.Add(new InventoryDocumentNumberSeries(
                TenantId, Id, type, prefix, length, 1, true));
        }
    }
}

/// <summary>سلسلة ترقيم مستند مخزني.</summary>
public sealed class InventoryDocumentNumberSeries : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid InventorySettingId { get; private set; }
    public InventoryDocumentSeriesType DocumentType { get; private set; }
    public string Prefix { get; private set; }
    public byte NumberLength { get; private set; }
    public long NextNumber { get; private set; }
    public bool AutoIncrement { get; private set; }

    private InventoryDocumentNumberSeries()
    {
        Prefix = string.Empty;
    }

    internal InventoryDocumentNumberSeries(
        Guid tenantId,
        Guid settingId,
        InventoryDocumentSeriesType documentType,
        string prefix,
        byte numberLength,
        long nextNumber,
        bool autoIncrement)
    {
        TenantId = tenantId;
        InventorySettingId = settingId;
        DocumentType = documentType;
        Prefix = string.Empty;
        Update(prefix, numberLength, nextNumber, autoIncrement);
    }

    public void Update(string prefix, byte numberLength, long nextNumber, bool autoIncrement)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            throw new BusinessException(ErrorCodes.RequiredField);
        Prefix = prefix.Trim().ToUpperInvariant();
        NumberLength = Math.Clamp(numberLength, (byte)1, (byte)12);
        NextNumber = Math.Max(1, nextNumber);
        AutoIncrement = autoIncrement;
    }

    public void ResetToDefault()
    {
        var (prefix, length) = DefaultFor(DocumentType);
        Update(prefix, length, 1, true);
    }

    /// <summary>يخصّص الرقم التالي ويعيد المستند المنسّق (مثل OB000001).</summary>
    public string AllocateNext()
    {
        var number = NextNumber;
        if (AutoIncrement)
            NextNumber = checked(NextNumber + 1);
        return $"{Prefix}{number.ToString().PadLeft(NumberLength, '0')}";
    }

    public static (string Prefix, byte Length) DefaultFor(InventoryDocumentSeriesType type) => type switch
    {
        InventoryDocumentSeriesType.GoodsReceipt => ("GR", 6),
        InventoryDocumentSeriesType.GoodsIssue => ("GI", 6),
        InventoryDocumentSeriesType.StockTransfer => ("TR", 6),
        InventoryDocumentSeriesType.InventoryAdjustment => ("ADJ", 6),
        InventoryDocumentSeriesType.InventoryCount => ("CNT", 6),
        InventoryDocumentSeriesType.Waste => ("WST", 6),
        InventoryDocumentSeriesType.OpeningBalance => ("OB", 6),
        InventoryDocumentSeriesType.Reservation => ("RSV", 6),
        InventoryDocumentSeriesType.ProductionIssue => ("PI", 6),
        InventoryDocumentSeriesType.ProductionReceipt => ("PR", 6),
        _ => ("DOC", 6)
    };
}
