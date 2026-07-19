using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Finance;

/// <summary>
/// صندوق نقدي تشغيلي مرتبط بحساب دليل — تعريف الصناديق (Finance master).
/// منفصل عن Sales.CashRegister التشغيلي لنقطة البيع.
/// </summary>
public sealed class CashBox : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public int Number { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string NameAr { get; private set; } = string.Empty;
    public string? NameEn { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid BranchId { get; private set; }
    public string? LocationName { get; private set; }
    public Guid? PosDeviceId { get; private set; }
    public Guid ChartOfAccountId { get; private set; }
    public Guid CurrencyId { get; private set; }
    public decimal OpeningBalance { get; private set; }
    public DateOnly? OpeningDate { get; private set; }
    public string? Description { get; private set; }

    public bool IsActive { get; private set; } = true;
    public bool AllowReceive { get; private set; } = true;
    public bool AllowPay { get; private set; } = true;
    public bool AllowDeposit { get; private set; } = true;
    public bool AllowWithdraw { get; private set; } = true;
    public bool AllowTransfer { get; private set; } = true;
    public bool RequireShiftBeforeUse { get; private set; }
    public bool AllowNegativeBalance { get; private set; }
    public decimal? MinBalance { get; private set; }
    public decimal? MaxBalance { get; private set; }

    public decimal CurrentBalance { get; private set; }
    public Guid? CurrentUserId { get; private set; }
    public DateTimeOffset? LastOpenedAt { get; private set; }
    public DateTimeOffset? LastClosedAt { get; private set; }
    public DateTimeOffset? LastMovementAt { get; private set; }
    public DateTimeOffset? LastCountAt { get; private set; }
    public bool IsOpen { get; private set; }
    public bool HasHadMovement { get; private set; }
    public bool IsSystem { get; private set; }
    public int SortOrder { get; private set; }

    private readonly List<CashBoxUser> _authorizedUsers = [];
    public IReadOnlyCollection<CashBoxUser> AuthorizedUsers => _authorizedUsers.AsReadOnly();

    private readonly List<CashBoxDevice> _devices = [];
    public IReadOnlyCollection<CashBoxDevice> Devices => _devices.AsReadOnly();

    private CashBox() { }

    public static CashBox Create(
        Guid tenantId,
        int number,
        string code,
        string nameAr,
        Guid companyId,
        Guid branchId,
        Guid chartOfAccountId,
        Guid currencyId,
        string? nameEn = null,
        string? locationName = null,
        Guid? posDeviceId = null,
        decimal openingBalance = 0,
        DateOnly? openingDate = null,
        string? description = null,
        int sortOrder = 0,
        bool isSystem = false)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId required.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        if (string.IsNullOrWhiteSpace(code)) throw new BusinessException(ErrorCodes.RequiredField);
        if (companyId == Guid.Empty || branchId == Guid.Empty
            || chartOfAccountId == Guid.Empty || currencyId == Guid.Empty)
            throw new BusinessException(ErrorCodes.RequiredField);
        if (openingBalance < 0) throw new BusinessException(ErrorCodes.CashBoxOpeningBalanceInvalid);

        return new CashBox
        {
            TenantId = tenantId,
            Number = number,
            Code = code.Trim().ToUpperInvariant(),
            NameAr = nameAr.Trim(),
            NameEn = Normalize(nameEn),
            CompanyId = companyId,
            BranchId = branchId,
            LocationName = Normalize(locationName),
            PosDeviceId = posDeviceId == Guid.Empty ? null : posDeviceId,
            ChartOfAccountId = chartOfAccountId,
            CurrencyId = currencyId,
            OpeningBalance = openingBalance,
            CurrentBalance = openingBalance,
            OpeningDate = openingDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Description = Normalize(description),
            SortOrder = sortOrder,
            IsSystem = isSystem,
            IsActive = true
        };
    }

    public void Update(
        string nameAr,
        string? nameEn,
        Guid companyId,
        Guid branchId,
        string? locationName,
        Guid? posDeviceId,
        Guid currencyId,
        DateOnly? openingDate,
        string? description,
        int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        if (companyId == Guid.Empty || branchId == Guid.Empty || currencyId == Guid.Empty)
            throw new BusinessException(ErrorCodes.RequiredField);

        NameAr = nameAr.Trim();
        NameEn = Normalize(nameEn);
        CompanyId = companyId;
        BranchId = branchId;
        LocationName = Normalize(locationName);
        PosDeviceId = posDeviceId == Guid.Empty ? null : posDeviceId;
        CurrencyId = currencyId;
        OpeningDate = openingDate ?? OpeningDate;
        Description = Normalize(description);
        SortOrder = sortOrder;
    }

    public void SetOpeningBalance(decimal openingBalance)
    {
        if (HasHadMovement)
            throw new BusinessException(ErrorCodes.CashBoxOpeningBalanceLocked);
        if (openingBalance < 0)
            throw new BusinessException(ErrorCodes.CashBoxOpeningBalanceInvalid);
        OpeningBalance = openingBalance;
        if (!HasHadMovement)
            CurrentBalance = openingBalance;
    }

    public void ChangeChartOfAccount(Guid chartOfAccountId)
    {
        if (HasHadMovement)
            throw new BusinessException(ErrorCodes.CashBoxGlAccountLocked);
        if (chartOfAccountId == Guid.Empty)
            throw new BusinessException(ErrorCodes.RequiredField);
        ChartOfAccountId = chartOfAccountId;
    }

    public void SetOperatingFlags(
        bool allowReceive,
        bool allowPay,
        bool allowDeposit,
        bool allowWithdraw,
        bool allowTransfer,
        bool requireShiftBeforeUse,
        bool allowNegativeBalance)
    {
        AllowReceive = allowReceive;
        AllowPay = allowPay;
        AllowDeposit = allowDeposit;
        AllowWithdraw = allowWithdraw;
        AllowTransfer = allowTransfer;
        RequireShiftBeforeUse = requireShiftBeforeUse;
        AllowNegativeBalance = allowNegativeBalance;
    }

    public void SetLimits(decimal? minBalance, decimal? maxBalance)
    {
        if (minBalance is decimal min && maxBalance is decimal max && min > max)
            throw new BusinessException(ErrorCodes.CashBoxLimitInvalid);
        MinBalance = minBalance;
        MaxBalance = maxBalance;
    }

    public void Activate() => IsActive = true;

    public void Deactivate()
    {
        if (IsOpen)
            throw new BusinessException(ErrorCodes.CashBoxOpenCannotDeactivate);
        IsActive = false;
    }

    public void ReplaceAuthorizedUsers(IEnumerable<CashBoxUser> users)
    {
        _authorizedUsers.Clear();
        var list = users.ToList();
        if (list.Count(u => u.IsDefault) > 1)
            throw new BusinessException(ErrorCodes.CashBoxUserDefaultInvalid);
        foreach (var u in list)
            _authorizedUsers.Add(u);
    }

    public void ReplaceDevices(IEnumerable<CashBoxDevice> devices)
    {
        _devices.Clear();
        foreach (var d in devices)
            _devices.Add(d);
    }

    public void MarkMovement()
    {
        HasHadMovement = true;
        LastMovementAt = DateTimeOffset.UtcNow;
    }

    public void EnsureCanDelete()
    {
        if (IsSystem)
            throw new BusinessException(ErrorCodes.CashBoxProtected);
        if (HasHadMovement || IsOpen)
            throw new BusinessException(ErrorCodes.CashBoxInUse);
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

/// <summary>مستخدم مصرح له على الصندوق.</summary>
public sealed class CashBoxUser
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid CashBoxId { get; private set; }
    public Guid UserId { get; private set; }
    public string? RoleName { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsManager { get; private set; }
    public bool IsCustodian { get; private set; }

    private CashBoxUser() { }

    public static CashBoxUser Create(
        Guid cashBoxId,
        Guid userId,
        string? roleName = null,
        bool isDefault = false,
        bool isManager = false,
        bool isCustodian = false)
    {
        if (userId == Guid.Empty) throw new BusinessException(ErrorCodes.RequiredField);
        return new CashBoxUser
        {
            CashBoxId = cashBoxId,
            UserId = userId,
            RoleName = string.IsNullOrWhiteSpace(roleName) ? null : roleName.Trim(),
            IsDefault = isDefault,
            IsManager = isManager,
            IsCustodian = isCustodian
        };
    }
}

/// <summary>دور الجهاز المرتبط بالصندوق.</summary>
public enum CashBoxDeviceRole
{
    PosTerminal = 1,
    CashDrawer = 2,
    ReceiptPrinter = 3,
    CustomerDisplay = 4,
    BarcodeReader = 5
}

/// <summary>جهاز مرتبط بالصندوق.</summary>
public sealed class CashBoxDevice
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid CashBoxId { get; private set; }
    public Guid? DeviceId { get; private set; }
    public CashBoxDeviceRole DeviceRole { get; private set; }
    public string? Label { get; private set; }

    private CashBoxDevice() { }

    public static CashBoxDevice Create(
        Guid cashBoxId,
        CashBoxDeviceRole deviceRole,
        Guid? deviceId = null,
        string? label = null)
    {
        return new CashBoxDevice
        {
            CashBoxId = cashBoxId,
            DeviceId = deviceId == Guid.Empty ? null : deviceId,
            DeviceRole = deviceRole,
            Label = string.IsNullOrWhiteSpace(label) ? null : label.Trim()
        };
    }
}
