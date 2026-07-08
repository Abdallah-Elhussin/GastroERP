using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Delivery;

/// <summary>DeliveryZone — منطقة توصيل (Aggregate Root)</summary>
public sealed class DeliveryZone : AuditableBaseEntity, ITenantEntity, IBranchEntity
{
    public Guid TenantId { get; private set; }
    public Guid BranchId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public decimal CenterLatitude { get; private set; }
    public decimal CenterLongitude { get; private set; }
    public decimal RadiusKm { get; private set; }
    public DeliveryZoneFeeType FeeType { get; private set; }
    public decimal FixedFee { get; private set; }
    public decimal FeePerKm { get; private set; }
    public int EstimatedMinutes { get; private set; }
    public bool IsActive { get; private set; }

    private DeliveryZone() { NameAr = string.Empty; }

    public static DeliveryZone Create(
        Guid tenantId, Guid branchId, string nameAr, decimal centerLat, decimal centerLng,
        decimal radiusKm, DeliveryZoneFeeType feeType, decimal fixedFee, decimal feePerKm,
        int estimatedMinutes, string? nameEn = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (branchId == Guid.Empty) throw new ArgumentException("BranchId cannot be empty.", nameof(branchId));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        if (radiusKm <= 0) throw new ArgumentException("Radius must be positive.", nameof(radiusKm));

        return new DeliveryZone
        {
            TenantId = tenantId,
            BranchId = branchId,
            NameAr = nameAr,
            NameEn = nameEn,
            CenterLatitude = centerLat,
            CenterLongitude = centerLng,
            RadiusKm = radiusKm,
            FeeType = feeType,
            FixedFee = fixedFee,
            FeePerKm = feePerKm,
            EstimatedMinutes = estimatedMinutes,
            IsActive = true
        };
    }

    public void Update(string nameAr, decimal centerLat, decimal centerLng, decimal radiusKm,
        DeliveryZoneFeeType feeType, decimal fixedFee, decimal feePerKm, int estimatedMinutes, string? nameEn)
    {
        if (radiusKm <= 0) throw new ArgumentException("Radius must be positive.", nameof(radiusKm));
        NameAr = nameAr;
        NameEn = nameEn;
        CenterLatitude = centerLat;
        CenterLongitude = centerLng;
        RadiusKm = radiusKm;
        FeeType = feeType;
        FixedFee = fixedFee;
        FeePerKm = feePerKm;
        EstimatedMinutes = estimatedMinutes;
    }

    public bool ContainsPoint(decimal latitude, decimal longitude)
    {
        var distance = HaversineKm(CenterLatitude, CenterLongitude, latitude, longitude);
        return distance <= (double)RadiusKm;
    }

    public decimal CalculateFee(decimal latitude, decimal longitude)
    {
        if (!ContainsPoint(latitude, longitude))
            throw new BusinessException(ErrorCodes.DeliveryAddressOutOfZone);

        return FeeType switch
        {
            DeliveryZoneFeeType.Fixed => FixedFee,
            DeliveryZoneFeeType.PerKilometer => Math.Round(
                FixedFee + FeePerKm * (decimal)HaversineKm(CenterLatitude, CenterLongitude, latitude, longitude), 4),
            _ => FixedFee
        };
    }

    private static double HaversineKm(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
    {
        const double r = 6371;
        var dLat = ToRad((double)(lat2 - lat1));
        var dLon = ToRad((double)(lon2 - lon1));
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad((double)lat1)) * Math.Cos(ToRad((double)lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return r * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRad(double deg) => deg * Math.PI / 180;

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
