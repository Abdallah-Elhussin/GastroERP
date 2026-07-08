using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Organization;

namespace GastroErp.Domain.Entities.Organization;

/// <summary>
/// Device — الجهاز (Aggregate Root)
/// يمثل كل جهاز مسجل في النظام (كاشير، شاشة مطبخ، طابعة، كيوسك).
/// كل جهاز له رمز تفعيل فريد يُستخدم لربطه بالنظام.
/// يدعم Offline-First من خلال تتبع حالة المزامنة.
/// </summary>
public sealed class Device : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public DeviceType DeviceType { get; private set; }
    public string? SerialNumber { get; private set; }
    public string? MacAddress { get; private set; }
    public string ActivationCode { get; private set; }
    public bool IsActivated { get; private set; }
    public DateTimeOffset? ActivatedAt { get; private set; }
    public DateTimeOffset? LastSyncAt { get; private set; }
    public bool IsOnline { get; private set; }
    public bool IsActive { get; private set; }

    private Device()
    {
        NameAr = string.Empty;
        ActivationCode = string.Empty;
    }

    public Device(Guid tenantId, string nameAr, DeviceType deviceType,
                  string? serialNumber = null, string? macAddress = null, string? nameEn = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);

        TenantId = tenantId;
        NameAr = nameAr;
        NameEn = nameEn;
        DeviceType = deviceType;
        SerialNumber = serialNumber;
        MacAddress = macAddress;
        ActivationCode = GenerateActivationCode();
        IsActivated = false;
        IsActive = true;
        IsOnline = false;

        RaiseDomainEvent(new DeviceRegisteredEvent(Id, TenantId, ActivationCode));
    }

    public void Activate(Guid branchId)
    {
        if (IsActivated)
            throw new InvalidOperationException("Device is already activated.");
        IsActivated = true;
        ActivatedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new DeviceActivatedEvent(Id, TenantId, branchId));
    }

    public void RecordSync()
    {
        LastSyncAt = DateTimeOffset.UtcNow;
    }

    public void SetOnline() => IsOnline = true;
    public void SetOffline() => IsOnline = false;
    public void Deactivate() => IsActive = false;
    public void Rename(string nameAr, string? nameEn)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        NameAr = nameAr;
        NameEn = nameEn;
    }

    private static string GenerateActivationCode()
    {
        var chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}

/// <summary>BranchDevice — ربط الجهاز بالفرع</summary>
public sealed class BranchDevice
{
    public Guid BranchId { get; private set; }
    public Guid DeviceId { get; private set; }
    public Guid TenantId { get; private set; }
    public DateTimeOffset AssignedAt { get; private set; }
    public string? AssignedBy { get; private set; }
    public DateTimeOffset? UnassignedAt { get; private set; }
    public string? UnassignedBy { get; private set; }
    public bool IsActive { get; private set; }

    private BranchDevice() { }

    public BranchDevice(Guid branchId, Guid deviceId, Guid tenantId, string? assignedBy = null)
    {
        if (branchId == Guid.Empty) throw new ArgumentException("BranchId cannot be empty.", nameof(branchId));
        if (deviceId == Guid.Empty) throw new ArgumentException("DeviceId cannot be empty.", nameof(deviceId));
        BranchId = branchId;
        DeviceId = deviceId;
        TenantId = tenantId;
        AssignedAt = DateTimeOffset.UtcNow;
        AssignedBy = assignedBy;
        IsActive = true;
    }

    public void Unassign(string? unassignedBy = null)
    {
        IsActive = false;
        UnassignedAt = DateTimeOffset.UtcNow;
        UnassignedBy = unassignedBy;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
