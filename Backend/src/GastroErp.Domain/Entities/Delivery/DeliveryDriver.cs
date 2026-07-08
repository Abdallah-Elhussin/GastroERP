using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Delivery;

namespace GastroErp.Domain.Entities.Delivery;

/// <summary>DeliveryDriver — سائق توصيل (Aggregate Root)</summary>
public sealed class DeliveryDriver : AuditableBaseEntity, ITenantEntity, IBranchEntity
{
    public Guid TenantId { get; private set; }
    public Guid BranchId { get; private set; }
    public Guid? UserId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string Phone { get; private set; }
    public DriverStatus Status { get; private set; }
    public string? VehiclePlate { get; private set; }
    public bool IsActive { get; private set; }

    private DeliveryDriver()
    {
        NameAr = string.Empty;
        Phone = string.Empty;
    }

    public static DeliveryDriver Create(
        Guid tenantId, Guid branchId, string nameAr, string phone,
        Guid? userId = null, string? nameEn = null, string? vehiclePlate = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (branchId == Guid.Empty) throw new ArgumentException("BranchId cannot be empty.", nameof(branchId));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        if (string.IsNullOrWhiteSpace(phone)) throw new BusinessException(ErrorCodes.RequiredField);

        return new DeliveryDriver
        {
            TenantId = tenantId,
            BranchId = branchId,
            UserId = userId,
            NameAr = nameAr,
            NameEn = nameEn,
            Phone = phone,
            VehiclePlate = vehiclePlate,
            Status = DriverStatus.Available,
            IsActive = true
        };
    }

    public void Update(string nameAr, string phone, string? nameEn, string? vehiclePlate, Guid? userId)
    {
        NameAr = nameAr;
        NameEn = nameEn;
        Phone = phone;
        VehiclePlate = vehiclePlate;
        UserId = userId;
    }

    public void SetStatus(DriverStatus status)
    {
        if (Status == status) return;
        var previous = Status;
        Status = status;
        RaiseDomainEvent(new DriverStatusChangedEvent(Id, previous, status));
    }

    public void MarkOnDelivery() => SetStatus(DriverStatus.OnDelivery);
    public void MarkAvailable() => SetStatus(DriverStatus.Available);

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
